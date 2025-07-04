using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyCombat : Combat
{
    public EnemyData enemyData;
    private float attackCooldownTimer = 0f;
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void Update()
    {
        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;
    }

    public bool CanAttack()
    {
        return attackCooldownTimer <= 0f && health != null && health.IsAlive;
    }

    /// <summary>
    /// Perform attack on the target if possible.
    /// </summary>
    public void Attack(Transform target)
    {
        if (!CanAttack() || target == null || enemyData.equippedWeapon == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        float weaponRange = enemyData.equippedWeapon.range;  // Make sure your weapon has a range field

        if (distanceToTarget > weaponRange)
        {
            // Target too far away, don't attack
            return;
        }

        Debug.Log($"{gameObject.name} is attacking with weapon: {enemyData.equippedWeapon.name} (Damage Type: {enemyData.equippedWeapon.damageType})");

        switch (enemyData.equippedWeapon.damageType)
        {
            case DamageType.Melee:
                PerformMeleeAttack(target);
                break;

            case DamageType.Ranged:
                PerformRangedAttack(target);
                break;

            default:
                Debug.LogWarning($"{gameObject.name}: Unsupported weapon damage type {enemyData.equippedWeapon.damageType}.");
                break;
        }

        attackCooldownTimer = enemyData.attackCooldown;
    }

    private void PerformMeleeAttack(Transform target)
    {
        DamageData damageData = new DamageData(
            enemyData.equippedWeapon.baseDamage,
            enemyData.equippedWeapon.damageType,
            gameObject);

        Vector3 knockbackDir = (target.position - transform.position).normalized;
        float knockbackForce = enemyData.equippedWeapon.knockbackForce;

        ApplyDamage(target.gameObject, damageData, knockbackDir, knockbackForce);

        Debug.Log($"{gameObject.name} melee attacked {target.name} with {enemyData.equippedWeapon.name}");
    }

    private void PerformRangedAttack(Transform target)
    {
        Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = target.position + Vector3.up * 1.5f;

        PerformRangedAttack(enemyData.equippedWeapon, spawnPos, targetPos);

        Debug.Log($"{gameObject.name} ranged attacked {target.name} with {enemyData.equippedWeapon.name}");
    }
}
