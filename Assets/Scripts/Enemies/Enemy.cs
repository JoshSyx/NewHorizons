using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : Health
{
    public EnemyData data;

    private GameObject playerObject;
    private Rigidbody rb;

    private enum DodgeState { Idle, DodgeLeft, DodgeRight, AdjustDistance }
    private DodgeState currentState = DodgeState.Idle;

    private float stateTimer = 0f;
    private float dodgeDuration = 0.5f;
    private float idleDuration = 0.3f;

    private Vector3 currentMoveDir = Vector3.zero;
    private static readonly Collider[] avoidanceResults = new Collider[10];
    private float avoidanceUpdateTimer = 0f;
    private Vector3 lastAvoidanceForce = Vector3.zero;
    private Vector3 smoothedAvoidanceForce = Vector3.zero;

    private int enemyID;
    private static int enemyCounter = 0;

    // Knockback handling
    private float knockbackTimer = 0f;
    private bool IsKnockedBack => knockbackTimer > 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Fix gravity usage for flying vs ground enemies
        if (!data.isFlying)
            rb.useGravity = true;
        else
            rb.useGravity = false;

        enemyID = enemyCounter++;

        if (GameManager.Instance != null && GameManager.Instance._player != null)
            playerObject = GameManager.Instance._player;
        else
            Debug.LogWarning("Player GameObject not found in GameManager!");
    }

    void Update()
    {
        if (IsKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            return; // Skip AI state updates while knocked back
        }

        if (playerObject == null) return;
        if ((Time.frameCount + enemyID) % 3 != 0) return;

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            Vector3 toPlayer = playerObject.transform.position - transform.position;
            float distance = toPlayer.magnitude;
            ChooseNextState(distance);
        }
    }

    void FixedUpdate()
    {
        if (IsKnockedBack) return;
        if (playerObject == null || data == null) return;

        if (data.isFlying)
        {
            // Flying enemies do NOT move here; handled by EnemyCombat
            return;
        }

        Vector3 toPlayer = playerObject.transform.position - transform.position;
        float distance = toPlayer.magnitude;
        Vector3 direction = toPlayer.normalized;

        Vector3 targetMoveDir = currentState switch
        {
            DodgeState.DodgeLeft => Vector3.Cross(Vector3.up, direction).normalized * 1.2f + AdjustDistance(direction, distance) * 0.5f,
            DodgeState.DodgeRight => -Vector3.Cross(Vector3.up, direction).normalized * 1.2f + AdjustDistance(direction, distance) * 0.5f,
            _ => AdjustDistance(direction, distance)
        };

        currentMoveDir = Vector3.Lerp(currentMoveDir, targetMoveDir, Time.fixedDeltaTime / data.directionSmoothness);

        // Avoidance (throttled and smoothed)
        avoidanceUpdateTimer -= Time.fixedDeltaTime;
        if (avoidanceUpdateTimer <= 0f)
        {
            Vector3 newAvoidance = CalculateAvoidanceForce();
            lastAvoidanceForce = Vector3.Lerp(lastAvoidanceForce, newAvoidance, 0.5f);
            avoidanceUpdateTimer = 0.2f + Random.Range(0f, 0.05f);
        }
        smoothedAvoidanceForce = Vector3.Lerp(smoothedAvoidanceForce, lastAvoidanceForce, Time.fixedDeltaTime / data.directionSmoothness);

        Vector3 finalMoveDir = currentMoveDir + smoothedAvoidanceForce;

        // For ground enemies discard vertical force
        finalMoveDir.y = 0f;

        Vector3 move = finalMoveDir.normalized * data.speed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + move);

        Vector3 flatDir = new Vector3(toPlayer.x, 0f, toPlayer.z);
        if (flatDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.fixedDeltaTime);
        }
    }

    private Vector3 CalculateAvoidanceForce()
    {
        Vector3 force = Vector3.zero;
        int count = Physics.OverlapSphereNonAlloc(transform.position, data.avoidanceRadius, avoidanceResults, data.enemyLayer);

        for (int i = 0; i < count; i++)
        {
            Collider other = avoidanceResults[i];
            if (other == null || other.gameObject == gameObject) continue;

            Vector3 away = transform.position - other.transform.position;
            float dist = away.magnitude;
            if (dist < 0.01f) continue;

            float strength = Mathf.Clamp01((data.avoidanceRadius - dist) / data.avoidanceRadius);
            force += away.normalized * strength * data.avoidanceStrength;
        }

        force.y = 0f; // ensure no vertical force for ground enemies

        return force;
    }

    private void ChooseNextState(float distance)
    {
        if (!data.keepDistance)
        {
            currentState = DodgeState.DodgeLeft;
            stateTimer = dodgeDuration;
            return;
        }

        if (distance < data.followDistance - data.distanceTolerance || distance > data.followDistance + data.distanceTolerance)
        {
            currentState = DodgeState.AdjustDistance;
            stateTimer = 0.3f;
        }
        else
        {
            float rand = Random.value;
            if (rand < 0.4f)
            {
                currentState = DodgeState.DodgeLeft;
                stateTimer = dodgeDuration;
            }
            else if (rand < 0.8f)
            {
                currentState = DodgeState.DodgeRight;
                stateTimer = dodgeDuration;
            }
            else
            {
                currentState = DodgeState.Idle;
                stateTimer = idleDuration;
            }
        }
    }

    private Vector3 AdjustDistance(Vector3 direction, float distance)
    {
        if (distance < data.followDistance - data.distanceTolerance)
            return -direction;
        else if (distance > data.followDistance + data.distanceTolerance)
            return direction;
        else
            return Vector3.zero;
    }

    // Call this method to apply knockback force & start knockback timer
    public void ApplyKnockback(Vector3 force, float duration = 1f)
    {
        rb.AddForce(force, ForceMode.Impulse);
        knockbackTimer = duration;
    }
}
