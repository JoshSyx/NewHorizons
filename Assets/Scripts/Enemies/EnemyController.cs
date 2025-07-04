﻿using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    public EnemyData enemyData;

    private GameObject playerObject;
    private Rigidbody rb;

    private enum DodgeState { Idle, DodgeLeft, DodgeRight, AdjustDistance, Diving }
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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = !enemyData.isFlying;

        enemyID = enemyCounter++;

        if (GameManager.Instance != null && GameManager.Instance._player != null)
            playerObject = GameManager.Instance._player;
        else
            Debug.LogWarning("Player GameObject not found in GameManager!");
    }

    void Update()
    {
        if (playerObject == null) return;
        if ((Time.frameCount + enemyID) % 3 != 0) return;

        if (!CanSeePlayer())
        {
            currentState = DodgeState.Idle;
            stateTimer = idleDuration;
            return;
        }

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
        if (playerObject == null || enemyData == null) return;

        Vector3 toPlayer = playerObject.transform.position - transform.position;
        float distance = toPlayer.magnitude;
        Vector3 directionToPlayer = toPlayer.normalized;

        // ✅ FIXED: Proper 3D diving behavior
        if (currentState == DodgeState.Diving && enemyData.isFlying)
        {
            Vector3 diveTarget = playerObject.transform.position;
            Vector3 diveDir = (diveTarget - rb.position).normalized;

            Vector3 nextPos = rb.position + diveDir * enemyData.diveSpeed * Time.fixedDeltaTime;

            // Optional: Clamp to stay above ground
            nextPos.y = Mathf.Max(nextPos.y, 0.5f);

            rb.MovePosition(nextPos);

            if (diveDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(diveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.fixedDeltaTime);
            }

            return;
        }

        Vector3 targetMoveDir = currentState switch
        {
            DodgeState.DodgeLeft => Vector3.Cross(Vector3.up, directionToPlayer).normalized * 1.2f + AdjustDistance(directionToPlayer, distance) * 0.5f,
            DodgeState.DodgeRight => -Vector3.Cross(Vector3.up, directionToPlayer).normalized * 1.2f + AdjustDistance(directionToPlayer, distance) * 0.5f,
            _ => AdjustDistance(directionToPlayer, distance)
        };

        currentMoveDir = Vector3.Lerp(currentMoveDir, targetMoveDir.normalized, Time.fixedDeltaTime / enemyData.directionSmoothness);

        avoidanceUpdateTimer -= Time.fixedDeltaTime;
        if (avoidanceUpdateTimer <= 0f)
        {
            Vector3 newAvoidance = CalculateAvoidanceForce();
            lastAvoidanceForce = Vector3.Lerp(lastAvoidanceForce, newAvoidance, 0.5f);
            avoidanceUpdateTimer = 0.2f + Random.Range(0f, 0.05f);
        }

        smoothedAvoidanceForce = Vector3.Lerp(smoothedAvoidanceForce, lastAvoidanceForce, Time.fixedDeltaTime / enemyData.directionSmoothness);

        Vector3 finalMoveDir = (currentMoveDir + smoothedAvoidanceForce).normalized;

        if (enemyData.isFlying)
        {
            float currentY = transform.position.y;
            float targetY = enemyData.flyingHeight;
            float smoothY = Mathf.Lerp(currentY, targetY, enemyData.verticalSmoothness);
            finalMoveDir.y = (smoothY - currentY) / Time.fixedDeltaTime;
        }
        else
        {
            finalMoveDir.y = 0f;
        }

        Vector3 move = finalMoveDir * enemyData.speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        Vector3 flatMoveDir = new Vector3(finalMoveDir.x, 0f, finalMoveDir.z);
        if (flatMoveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatMoveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * Time.fixedDeltaTime);
        }
        else
        {
            Vector3 flatToPlayer = new Vector3(toPlayer.x, 0f, toPlayer.z);
            if (flatToPlayer.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.fixedDeltaTime);
            }
        }
    }

    private Vector3 CalculateAvoidanceForce()
    {
        Vector3 force = Vector3.zero;
        int count = Physics.OverlapSphereNonAlloc(transform.position, enemyData.avoidanceRadius, avoidanceResults, enemyData.enemyLayer);

        for (int i = 0; i < count; i++)
        {
            Collider other = avoidanceResults[i];
            if (other == null || other.gameObject == gameObject) continue;

            Vector3 away = transform.position - other.transform.position;
            float dist = away.magnitude;
            if (dist < 0.01f) continue;

            float strength = Mathf.Clamp01((enemyData.avoidanceRadius - dist) / enemyData.avoidanceRadius);
            force += away.normalized * strength * enemyData.avoidanceStrength;
        }

        force.y = 0f;
        return force;
    }

    private void ChooseNextState(float distance)
    {
        if (enemyData.isFlying && distance < enemyData.diveTriggerDistance)
        {
            currentState = DodgeState.Diving;
            stateTimer = 1.5f;
            return;
        }

        if (!enemyData.keepDistance)
        {
            currentState = DodgeState.DodgeLeft;
            stateTimer = dodgeDuration;
            return;
        }

        if (distance < enemyData.followDistance - enemyData.distanceTolerance ||
            distance > enemyData.followDistance + enemyData.distanceTolerance)
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

    public void ResetStateAfterDive()
    {
        currentState = DodgeState.Idle;
        stateTimer = idleDuration;
    }

    private Vector3 AdjustDistance(Vector3 direction, float distance)
    {
        if (distance < enemyData.followDistance - enemyData.distanceTolerance)
            return -direction;
        else if (distance > enemyData.followDistance + enemyData.distanceTolerance)
            return direction;
        else
            return Vector3.zero;
    }

    private bool CanSeePlayer()
    {
        if (playerObject == null || enemyData == null) return false;

        Vector3 toPlayer = playerObject.transform.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer > enemyData.visionDistance) return false;

        float angleToPlayer = Vector3.Angle(transform.forward, toPlayer);
        if (angleToPlayer > enemyData.visionAngle / 2f) return false;

        RaycastHit hit;
        Vector3 origin = transform.position;
        Vector3 direction = toPlayer.normalized;

        if (Physics.Raycast(origin, direction, out hit, enemyData.visionDistance))
        {
            return hit.collider.gameObject == playerObject;
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        // Max distance gizmo removed
    }
}
