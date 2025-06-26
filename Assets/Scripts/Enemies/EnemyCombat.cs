using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : Combat
{
    private GameObject player;
    private EnemyData enemyData;

    private float meleeTimer = 0f;
    private float rangedTimer = 0f;

    private bool meleeAttackPending = false;
    private bool rangedAttackPending = false;

    private void Awake()
    {
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null) enemyData = enemy.enemyData;
    }
    private void Start()
    {
        if (player == null && GameManager.Instance != null)
        {
            player = GameManager.Instance._player;
        }
    }

    private void Update()
    {
        meleeTimer -= Time.deltaTime;
        rangedTimer -= Time.deltaTime;

        if (!CanSeePlayer())
        {
            // Reset pending flags so it won't try to attack when player not visible
            meleeAttackPending = false;
            rangedAttackPending = false;
            return;
        }

        // Melee attack logic
        if (meleeTimer <= 0f && !meleeAttackPending && enemyData != null && enemyData.equippedWeapon != null && enemyData.equippedWeapon.IsMelee)
        {
            meleeAttackPending = true;
            float delay = Random.Range(0f, 1f);
            StartCoroutine(DelayedMeleeAttack(delay));
        }

        // Ranged attack logic
        if (rangedTimer <= 0f && !rangedAttackPending && enemyData != null && enemyData.equippedWeapon != null && enemyData.equippedWeapon.IsRanged && player != null)
        {
            rangedAttackPending = true;
            float delay = Random.Range(0f, 1f);
            StartCoroutine(DelayedRangedAttack(delay));
        }
    }


    private IEnumerator DelayedMeleeAttack(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (enemyData != null && enemyData.equippedWeapon != null && enemyData.equippedWeapon.IsMelee)
        {
            PerformMeleeAttack(enemyData.equippedWeapon);
            meleeTimer = enemyData.attackCooldown;
        }

        meleeAttackPending = false;
    }

    private IEnumerator DelayedRangedAttack(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (enemyData != null && enemyData.equippedWeapon != null && enemyData.equippedWeapon.IsRanged && player != null)
        {
            PerformRangedAttack(enemyData.equippedWeapon);
            rangedTimer = enemyData.attackCooldown;
        }

        rangedAttackPending = false;
    }

    private void PerformMeleeAttack(WeaponItem weapon)
    {
        Vector3 origin = transform.position + Vector3.up * 1.0f;
        Vector3 forward = transform.forward;
        float meleeRange = weapon.MeleeRange;
        float meleeAngle = weapon.MeleeAngle;

        List<GameObject> hitPlayers = GetMeleeHits(origin, forward, meleeRange, meleeAngle, ~0, "Player");

        bool hitAny = false;
        foreach (var playerObj in hitPlayers)
        {
            DamageData data = weapon.GetDamageData(gameObject);
            Vector3 knockbackDir = (playerObj.transform.position - transform.position).normalized;
            float knockbackForce = 5f;

            ApplyDamage(playerObj, data, knockbackDir, knockbackForce);
            hitAny = true;
        }

        if (hitAny)
            Debug.Log($"{gameObject.name} hit player with melee weapon {weapon.itemName}");
        else
            Debug.Log($"{gameObject.name} missed melee attack.");
    }

    private void PerformRangedAttack(WeaponItem weapon)
    {
        if (player == null) return;

        float spawnHeight = 1.5f;
        Vector3 spawnPosition = transform.position + Vector3.up * spawnHeight;
        Vector3 aimTarget = new Vector3(player.transform.position.x, spawnHeight, player.transform.position.z);

        base.PerformRangedAttack(weapon, spawnPosition, aimTarget);
    }

    private bool CanSeePlayer()
    {
        if (player == null || enemyData == null) return false;

        Vector3 toPlayer = player.transform.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        // Use visionDistance from EnemyData
        if (distanceToPlayer > enemyData.visionDistance)
            return false;

        // Use visionAngle from EnemyData
        Vector3 forward = transform.forward;
        float angleToPlayer = Vector3.Angle(forward, toPlayer);
        if (angleToPlayer > enemyData.visionAngle / 2f)
            return false;

        // Line of Sight check
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 1.5f;  // eye height for raycast
        Vector3 direction = toPlayer.normalized;

        if (Physics.Raycast(origin, direction, out hit, enemyData.visionDistance))
        {
            if (hit.collider.gameObject != player)
            {
                // Hit something else blocking view
                return false;
            }
        }
        else
        {
            // Nothing hit (shouldn't happen if player is within maxViewDistance)
            return false;
        }

        return true;
    }
}
