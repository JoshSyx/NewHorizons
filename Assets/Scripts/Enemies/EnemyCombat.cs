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

    [Header("Melee Sweep Visual")]
    [SerializeField] private GameObject sliceMeshPrefab;
    [SerializeField] private float meleeVisualDuration = 0.5f;

    private void Awake()
    {
        EnemyController enemy = GetComponent<EnemyController>();
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
        if (enemyData == null || player == null) return;

        meleeTimer -= Time.deltaTime;
        rangedTimer -= Time.deltaTime;

        if (!CanSeePlayer())
        {
            meleeAttackPending = false;
            rangedAttackPending = false;
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // FLYING ENEMY: Dive attack logic
        if (enemyData.isFlying && enemyData.equippedWeapon != null && enemyData.equippedWeapon.IsMelee)
        {
            if (distanceToPlayer <= enemyData.diveTriggerDistance && meleeTimer <= 0f && !meleeAttackPending)
            {
                meleeAttackPending = true;
                StartCoroutine(PerformDiveAttack());
            }
        }

        // MELEE ATTACK (ground or close-range flying if not diving)
        if (!enemyData.isFlying && enemyData.equippedWeapon != null && enemyData.equippedWeapon.IsMelee)
        {
            if (meleeTimer <= 0f && !meleeAttackPending)
            {
                meleeAttackPending = true;
                float delay = Random.Range(0f, 1f);
                StartCoroutine(DelayedMeleeAttack(delay));
            }
        }

        // RANGED ATTACK
        if (enemyData.equippedWeapon != null && enemyData.equippedWeapon.IsRanged)
        {
            if (rangedTimer <= 0f && !rangedAttackPending)
            {
                rangedAttackPending = true;
                float delay = Random.Range(0f, 1f);
                StartCoroutine(DelayedRangedAttack(delay));
            }
        }

        // EXPLOSION ATTACK (suicide bomb enemy)
        if (enemyData.equippedWeapon != null &&
            enemyData.equippedWeapon.damageType == DamageType.Explosion &&
            distanceToPlayer <= enemyData.equippedWeapon.explosionRadius)
        {
            StartCoroutine(ExplodeAfterDelay(enemyData.equippedWeapon.fuseTime));
        }
    }


    private IEnumerator ExplodeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (enemyData == null || enemyData.equippedWeapon == null) yield break;

        WeaponItem weapon = enemyData.equippedWeapon;

        PerformExplosionAttack(
            transform.position,
            weapon.explosionRadius,       // Use explosionRadius
            weapon.explosionDamage,       // Use explosionDamage
            DamageType.Explosion,         // Correct damage type enum
            gameObject,
            weapon.knockbackForce         // Use weapon knockbackForce
        );

        Destroy(gameObject);
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
        float meleeRange = weapon.range;
        float meleeAngle = weapon.MeleeAngle;

        List<GameObject> hitPlayers = GetMeleeHits(origin, forward, meleeRange, meleeAngle, ~0, "Player");

        foreach (var playerObj in hitPlayers)
        {
            DamageData data = weapon.GetDamageData(gameObject);
            Vector3 knockbackDir = (playerObj.transform.position - transform.position).normalized;
            float knockbackForce = 5f;

            ApplyDamage(playerObj, data, knockbackDir, knockbackForce);

            // Retreat only if flying enemy and retreat enabled
            if (enemyData.isFlying && enemyData.retreatAfterHit)
            {
                StartCoroutine(RetreatAfterHit());
            }
        }

        ShowMeleeSweepVisual(meleeRange, meleeAngle);
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

    private void ShowMeleeSweepVisual(float range, float angle)
    {
        if (sliceMeshPrefab == null) return;

        Vector3 position = transform.position + Vector3.up * 1.0f;
        Quaternion rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
        GameObject slice = Instantiate(sliceMeshPrefab, position, rotation);

        if (slice.TryGetComponent(out SliceMesh sliceMesh))
        {
            sliceMesh.radius = range;
            sliceMesh.angle = angle;
            sliceMesh.duration = meleeVisualDuration;
        }
    }

    private IEnumerator PerformDiveAttack()
    {
        Vector3 targetPosition = player.transform.position;
        float elapsedTime = 0f;
        float diveDuration = 1f; // Adjust as needed
        Vector3 startPosition = transform.position;

        float targetY = enemyData.flyingHeight;

        while (elapsedTime < diveDuration)
        {
            float t = elapsedTime / diveDuration;

            // Lerp horizontal (x,z) normally
            Vector3 horizontalPos = Vector3.Lerp(
                new Vector3(startPosition.x, 0, startPosition.z),
                new Vector3(targetPosition.x, 0, targetPosition.z),
                t
            );

            // Keep vertical pos at flyingHeight (or lerp smoothly if you want)
            float lerpedY = Mathf.Lerp(startPosition.y, targetY, t);

            transform.position = new Vector3(horizontalPos.x, lerpedY, horizontalPos.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Snap to final position above flyingHeight
        transform.position = new Vector3(targetPosition.x, targetY, targetPosition.z);

        PerformMeleeAttack(enemyData.equippedWeapon);

        meleeTimer = enemyData.attackCooldown;
        meleeAttackPending = false;

        GetComponent<EnemyController>()?.ResetStateAfterDive();
    }


    private IEnumerator RetreatAfterHit()
    {
        float retreatDist = enemyData.retreatDistance;
        float retreatSpeed = enemyData.retreatSpeed;

        Vector3 retreatDir = (transform.position - player.transform.position).normalized;
        Vector3 retreatTarget = transform.position + retreatDir * retreatDist;

        float distanceToTarget = Vector3.Distance(transform.position, retreatTarget);
        float elapsed = 0f;
        float duration = distanceToTarget / retreatSpeed;

        while (elapsed < duration)
        {
            transform.position = Vector3.MoveTowards(transform.position, retreatTarget, retreatSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

}
