using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(EnemyCombat))]
public class EnemyController : MonoBehaviour
{
    public EnemyData enemyData;
    public Transform player;

    private CharacterController controller;
    private EnemyCombat combat;

    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 desiredDirection = Vector3.zero;
    private float verticalVelocity = 0f;

    private float retreatTimer = 0f;
    private Vector3 startPosition;

    private Vector3 wanderTarget;
    private float wanderTimer = 0f;

    private enum FlyingState { Patrolling, Diving, Attacking, Ascending }
    private FlyingState flyingState = FlyingState.Patrolling;

    private float flyingHoverTimer = 0f;
    private Vector3 patrolCenter;
    private float patrolAngle = 0f;

    private float attackCooldownTimer = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        combat = GetComponent<EnemyCombat>();

        startPosition = transform.position;
        wanderTarget = startPosition;

        if (enemyData.isFlying)
            patrolCenter = transform.position;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance._player == null)
        {
            if (enemyData.isFlying)
                FlyingPatrol();
            else
                HandleWandering();
            return;
        }

        player = GameManager.Instance._player.transform;

        float distanceFromStart = Vector3.Distance(transform.position, startPosition);
        if (distanceFromStart > enemyData.maxWanderDistance)
        {
            ReturnToStartPosition();
            return;
        }

        Vector3 toPlayer = player.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        bool canSeePlayer = CanSeePlayer(toPlayer, distanceToPlayer);

        if (enemyData.isFlying)
        {
            // Debug player visibility and current flying state
            Debug.Log($"[Flying AI] CanSeePlayer: {canSeePlayer}, FlyingState: {flyingState}, DistanceToPlayer: {distanceToPlayer:F2}");

            HandleFlyingAI(toPlayer, distanceToPlayer, canSeePlayer);
        }
        else
        {
            if (canSeePlayer)
                HandleCombatAndMovement(toPlayer, distanceToPlayer);
            else
                HandleWandering();
        }
    }

    private bool CanSeePlayer(Vector3 toPlayer, float distance)
    {
        if (distance > enemyData.visionDistance)
            return false;

        Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
        Vector3 toPlayerDir = (player.position - eyePosition).normalized;

        if (enemyData.isFlying)
        {
            float angle3D = Vector3.Angle(transform.forward, toPlayerDir);
            if (angle3D > enemyData.visionAngle * 0.5f)
                return false;

            RaycastHit hit;
            if (Physics.Raycast(eyePosition, toPlayerDir, out hit, enemyData.visionDistance))
            {
                if (hit.transform == player)
                    return true;
                else
                    return false;
            }
            return false;
        }
        else
        {
            Vector3 forward = transform.forward;
            Vector3 flatToPlayer = toPlayer;
            flatToPlayer.y = 0f;
            flatToPlayer.Normalize();

            float angle = Vector3.Angle(forward, flatToPlayer);
            if (angle > enemyData.visionAngle * 0.5f)
                return false;

            RaycastHit hit2;
            if (Physics.Raycast(eyePosition, toPlayerDir, out hit2, enemyData.visionDistance))
            {
                if (hit2.transform != player)
                    return false;
            }

            return true;
        }
    }

    private void HandleFlyingAI(Vector3 toPlayer, float distanceToPlayer, bool canSeePlayer)
    {
        switch (flyingState)
        {
            case FlyingState.Patrolling:
                FlyingPatrol();
                if (canSeePlayer)
                {
                    Debug.Log("[Flying AI] Player spotted: Switching to Diving");
                    flyingState = FlyingState.Diving;
                }
                break;

            case FlyingState.Diving:
                Vector3 diveTarget = player.position + Vector3.up * 1.5f;
                Vector3 diveDirection = (diveTarget - transform.position).normalized;

                Debug.Log($"[Flying AI] Diving towards player at {diveTarget}, CurrentPos: {transform.position}, Direction: {diveDirection}");

                MoveInDirection(diveDirection, enemyData.diveSpeed);
                RotateTowards(toPlayer);

                if (distanceToPlayer < enemyData.attackRange)
                {
                    Debug.Log("[Flying AI] In attack range: Switching to Attacking");
                    flyingState = FlyingState.Attacking;
                    flyingHoverTimer = enemyData.hoverAfterAttackTime;
                    attackCooldownTimer = 0f; // reset attack cooldown
                    combat.Attack(player);
                }
                break;

            case FlyingState.Attacking:
                flyingHoverTimer -= Time.deltaTime;
                attackCooldownTimer -= Time.deltaTime;

                Debug.Log($"[Flying AI] Attacking. HoverTimer: {flyingHoverTimer:F2}, AttackCooldownTimer: {attackCooldownTimer:F2}");

                if (attackCooldownTimer <= 0f)
                {
                    Debug.Log("[Flying AI] Performing repeated attack");
                    combat.Attack(player);
                    attackCooldownTimer = enemyData.attackCooldown; // Make sure this exists in EnemyData
                }

                if (flyingHoverTimer <= 0f)
                {
                    Debug.Log("[Flying AI] Hover time over: Ascending");
                    flyingState = FlyingState.Ascending;
                }
                break;

            case FlyingState.Ascending:
                Vector3 ascendTarget = patrolCenter + Vector3.up * enemyData.patrolHeight;
                Vector3 ascendDirection = (ascendTarget - transform.position).normalized;

                Debug.Log($"[Flying AI] Ascending to {ascendTarget}, CurrentPos: {transform.position}");

                MoveInDirection(ascendDirection, enemyData.ascendSpeed);
                RotateTowards(ascendTarget - transform.position);

                if (Mathf.Abs(transform.position.y - ascendTarget.y) < 0.5f)
                {
                    Debug.Log("[Flying AI] Reached patrol height: Switching to Patrolling");
                    flyingState = FlyingState.Patrolling;
                }
                break;
        }
    }

    private void FlyingPatrol()
    {
        patrolAngle += Time.deltaTime * enemyData.speed;
        float radius = enemyData.wanderRadius;

        float x = Mathf.Cos(patrolAngle) * radius;
        float z = Mathf.Sin(patrolAngle) * radius;

        Vector3 circlePoint = patrolCenter + new Vector3(x, 0f, z);
        Vector3 targetPosition = new Vector3(circlePoint.x, patrolCenter.y + enemyData.patrolHeight, circlePoint.z);

        Debug.Log($"[Flying AI] Patrolling around {targetPosition}");

        MoveInDirection((targetPosition - transform.position).normalized, enemyData.speed);
        RotateTowards(targetPosition - transform.position);
    }

    private void RotateTowards(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, enemyData.rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleCombatAndMovement(Vector3 toPlayer, float distance)
    {
        if (enemyData.isFlying && retreatTimer > 0f)
        {
            retreatTimer -= Time.deltaTime;
            Vector3 retreatDir = -toPlayer.normalized;
            MoveInDirection(retreatDir, enemyData.retreatSpeed);
            return;
        }

        Vector3 moveDir = Vector3.zero;

        if (enemyData.keepDistance)
        {
            if (distance < enemyData.minDistance)
                moveDir = -toPlayer.normalized * enemyData.adjustDistanceAmount;
            else if (distance > enemyData.maxDistance)
                moveDir = toPlayer.normalized * enemyData.adjustDistanceAmount;
            else
                moveDir = Vector3.zero;
        }
        else
        {
            moveDir = toPlayer.normalized;
        }

        if (moveDir.sqrMagnitude > 0.01f)
        {
            MoveInDirection(moveDir, enemyData.speed);
            RotateTowards(toPlayer);
        }
        else
        {
            RotateTowards(toPlayer);
            MoveInDirection(Vector3.zero, 0f);
        }

        if (distance <= enemyData.maxDistance)
            combat.Attack(player);
    }

    private void HandleWandering()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            Vector2 randomCircle = Random.insideUnitCircle * enemyData.wanderRadius;
            wanderTarget = startPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
            wanderTimer = enemyData.wanderInterval;

            Debug.Log($"[Wandering] New wander target: {wanderTarget}");
        }

        Vector3 toTarget = wanderTarget - transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        if (distance > 0.1f)
        {
            Vector3 moveDir = toTarget.normalized;
            RotateTowards(moveDir);
            MoveInDirection(moveDir, enemyData.speed);
        }
        else
        {
            MoveInDirection(Vector3.zero, 0f);
        }
    }

    private void MoveInDirection(Vector3 direction, float speed)
    {
        desiredDirection = Vector3.SmoothDamp(desiredDirection, direction, ref currentVelocity, enemyData.directionSmoothness);
        Vector3 move = desiredDirection * speed;

        if (!enemyData.isFlying)
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
            if (controller.isGrounded && verticalVelocity < 0)
                verticalVelocity = -1f;

            move.y = verticalVelocity;
        }
        else
        {
            float targetHeight = startPosition.y + enemyData.flightHeight;
            float currentY = transform.position.y;
            float heightDelta = targetHeight - currentY;

            float verticalSpeed = Mathf.Clamp(heightDelta, -5f, 5f);
            move.y = verticalSpeed;

            Debug.Log($"[Flying Movement] Moving with horizontal speed {speed:F2} and vertical speed {verticalSpeed:F2}. TargetHeight: {targetHeight:F2}, CurrentY: {currentY:F2}");
        }

        controller.Move(move * Time.deltaTime);
    }

    private void ReturnToStartPosition()
    {
        Vector3 toStart = startPosition - transform.position;
        float distance = toStart.magnitude;

        if (distance > 0.1f)
        {
            Vector3 moveDir = toStart.normalized;
            MoveInDirection(moveDir, enemyData.speed * enemyData.returnSpeedMultiplier);
            RotateTowards(moveDir);
        }
        else
        {
            MoveInDirection(Vector3.zero, 0f);
        }
    }

    public void TriggerRetreat()
    {
        if (enemyData.isFlying && enemyData.retreatAfterHit)
            retreatTimer = 1f;
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(startPosition, enemyData.maxWanderDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(startPosition, enemyData.wanderRadius);

        if (enemyData.isFlying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(patrolCenter, enemyData.wanderRadius);

            Gizmos.color = Color.blue;
            Vector3 flightLevel = startPosition + Vector3.up * enemyData.flightHeight;
            Gizmos.DrawLine(startPosition, flightLevel);
            Gizmos.DrawWireSphere(flightLevel, 0.3f);

            Vector3 diveStart = patrolCenter + Vector3.up * enemyData.patrolHeight;
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(patrolCenter, diveStart);
            Gizmos.DrawWireSphere(diveStart, 0.3f);
        }

#if UNITY_EDITOR
        if (player != null && enemyData != null)
        {
            Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);

            if (enemyData.isFlying)
            {
                Draw3DVisionCone(eyePosition, transform.forward, enemyData.visionAngle, enemyData.visionDistance);
            }
            else
            {
                float halfFOV = enemyData.visionAngle * 0.5f;

                Handles.color = new Color(1f, 0.5f, 0f, 0.2f);
                Handles.DrawSolidArc(eyePosition, Vector3.up, Quaternion.Euler(0, -halfFOV, 0) * transform.forward,
                    enemyData.visionAngle, enemyData.visionDistance);
            }
        }
#endif
    }

#if UNITY_EDITOR
    private void Draw3DVisionCone(Vector3 origin, Vector3 forward, float visionAngle, float visionDistance)
    {
        int segments = 24;
        float halfAngle = visionAngle * 0.5f;

        Vector3 up = Vector3.up;
        Vector3 right = Vector3.Cross(forward, up).normalized;

        Vector3 prevPoint = Vector3.zero;
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Lerp(-halfAngle, halfAngle, (float)i / segments);
            Quaternion rot = Quaternion.AngleAxis(angle, up);
            Vector3 dir = rot * forward;

            Vector3 point = origin + dir * visionDistance;

            if (i > 0)
            {
                Handles.DrawLine(origin, point);
                Handles.DrawLine(prevPoint, point);
            }

            prevPoint = point;
        }
    }
#endif
}
