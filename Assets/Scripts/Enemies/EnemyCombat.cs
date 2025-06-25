using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    private Enemy enemy;
    private EnemyData enemyData;
    private GameObject player;

    private float attackCooldownTimer = 0f;

    // Flying behavior states
    private enum State { Approaching, Attacking, Retreating }
    private State currentState = State.Approaching;

    // Retreat parameters
    [SerializeField] private float retreatDuration = 2f;
    private float retreatTimer = 0f;

    // Retreat distance multiplier (how far to fly away)
    [SerializeField] private float retreatDistance = 5f;

    // Movement speed (fallback if EnemyData speed not used)
    [SerializeField] private float moveSpeed = 5f;

    private Vector3 retreatDirection;

    private void Start()
    {
        enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("Enemy component missing on " + gameObject.name);
            enabled = false;
            return;
        }

        enemyData = enemy.data;
        if (enemyData == null)
        {
            Debug.LogError("EnemyData is missing on Enemy component for " + gameObject.name);
            enabled = false;
            return;
        }

        player = GameManager.Instance?._player;

        if (enemyData != null && enemyData.speed > 0f)
            moveSpeed = enemyData.speed;

        Debug.Log($"{gameObject.name} moveSpeed: {moveSpeed}, isFlying: {enemyData.isFlying}");
    }

    private void Update()
    {
        if (player == null || enemyData.equippedWeapon == null)
            return;

        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;

        if (!enemyData.isFlying)
        {
            TryAttackAtCurrentDistance();
            return;
        }

        switch (currentState)
        {
            case State.Approaching:
                HandleApproach();
                break;
            case State.Attacking:
                currentState = State.Retreating;
                retreatTimer = retreatDuration;
                retreatDirection = (transform.position - player.transform.position).normalized;
                break;
            case State.Retreating:
                HandleRetreat();
                break;
        }
    }

    private void TryAttackAtCurrentDistance()
    {
        Vector3 toPlayer = player.transform.position - transform.position;
        float distance = toPlayer.magnitude;

        if (CanAttack(distance) && attackCooldownTimer <= 0f)
        {
            Attack();
            attackCooldownTimer = enemyData.attackCooldown;
        }
    }

    private void HandleApproach()
    {
        Vector3 toPlayer = player.transform.position - transform.position;
        float distance = toPlayer.magnitude;

        MoveTowards(player.transform.position);

        if (CanAttack(distance) && attackCooldownTimer <= 0f)
        {
            Attack();
            attackCooldownTimer = enemyData.attackCooldown;
            currentState = State.Attacking;
        }
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position);
        float distance = direction.magnitude;

        if (distance < 0.01f)
            return;

        direction.Normalize();
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);
    }

    private void HandleRetreat()
    {
        if (retreatTimer > 0f)
        {
            retreatTimer -= Time.deltaTime;
            Vector3 retreatTarget = transform.position + retreatDirection * retreatDistance;
            MoveTowards(retreatTarget);
        }
        else
        {
            currentState = State.Approaching;
        }
    }

    private bool CanAttack(float distance)
    {
        WeaponItem weapon = enemyData.equippedWeapon;
        return weapon.IsMelee ? distance <= weapon.MeleeRange : distance <= weapon.RangedDistance;
    }

    private void Attack()
    {
        WeaponItem weapon = enemyData.equippedWeapon;
        if (weapon.IsMelee)
            PerformMeleeAttack(weapon);
        else
            PerformRangedAttack(weapon);
    }

    private void PerformMeleeAttack(WeaponItem weapon)
    {
        Vector3 origin = transform.position + Vector3.up * 1.0f;
        Vector3 forward = transform.forward;
        float meleeRange = weapon.MeleeRange;
        float meleeAngle = weapon.MeleeAngle;

        Collider[] hits = Physics.OverlapSphere(origin, meleeRange);
        bool hitAny = false;

        foreach (var hit in hits)
        {
            GameObject rootObj = hit.transform.root.gameObject;
            if (rootObj.CompareTag("Player"))
            {
                Vector3 directionToTarget = (rootObj.transform.position - origin).normalized;
                float angleToTarget = Vector3.Angle(forward, directionToTarget);

                if (angleToTarget <= meleeAngle / 2f)
                {
                    DamageData data = weapon.GetDamageData(gameObject);
                    Vector3 knockbackDir = (rootObj.transform.position - transform.position).normalized;
                    float knockbackForce = 5f;
                    rootObj.GetComponent<Health>()?.InflictDamage(data, knockbackDir, knockbackForce);
                    hitAny = true;
                }
            }
        }

        if (hitAny)
            Debug.Log($"{gameObject.name} hit player with melee weapon {weapon.itemName}");
        else
            Debug.Log($"{gameObject.name} missed melee attack.");
    }

    private void PerformRangedAttack(WeaponItem weapon)
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;
        Vector3 directionToPlayer = (player.transform.position - rayOrigin).normalized;
        float maxDistance = weapon.RangedDistance;

        if (Physics.Raycast(rayOrigin, directionToPlayer, out RaycastHit hit, maxDistance))
        {
            if (hit.collider.gameObject == player)
            {
                DamageData data = weapon.GetDamageData(gameObject);
                Vector3 knockbackDir = (player.transform.position - transform.position).normalized;
                float knockbackForce = 3f;
                player.GetComponent<Health>()?.InflictDamage(data, knockbackDir, knockbackForce);
                Debug.Log($"{gameObject.name} hit player with ranged weapon {weapon.itemName} | Damage: {data.FinalAmount}");
                return;
            }
        }

        Debug.Log($"{gameObject.name} missed ranged attack (no line of sight).");
    }
}
