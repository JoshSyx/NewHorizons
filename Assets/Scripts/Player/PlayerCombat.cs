using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : Combat
{
    [SerializeField] private PlayerInventory inventory;

    [Header("Melee Sweep Visual")]
    [SerializeField] private GameObject sliceMeshPrefab;
    [SerializeField] private float meleeVisualDuration = 0.5f;

    [Header("Aim Assist Settings")]
    [SerializeField] private float aimAssistMaxDistance = 10f;
    [SerializeField] private float aimAssistAngle = 15f;

    [Header("Melee Settings")]
    [SerializeField] private LayerMask enemyLayerMask; // Assign Enemy layer in Inspector

    private WeaponItem lastUsedWeapon;
    public WeaponItem LastUsedWeapon => lastUsedWeapon;

    private Dictionary<WeaponItem, float> weaponCooldownTimers = new Dictionary<WeaponItem, float>();

    private void Update()
    {
        List<WeaponItem> keys = new List<WeaponItem>(weaponCooldownTimers.Keys);
        foreach (var weapon in keys)
        {
            weaponCooldownTimers[weapon] -= Time.deltaTime;
            if (weaponCooldownTimers[weapon] <= 0f)
                weaponCooldownTimers.Remove(weapon);
        }
    }

    public void AttackWithSlot(WeaponSlot slot)
    {
        if (inventory == null)
        {
            Debug.LogWarning("PlayerInventory reference is missing.");
            return;
        }

        WeaponItem weapon = inventory.GetEquippedWeapon(slot);
        if (weapon == null)
        {
            Debug.Log($"No weapon equipped in the {slot} slot.");
            return;
        }

        if (IsWeaponOnCooldown(weapon))
        {
            Debug.Log($"Weapon {weapon.itemName} is on cooldown.");
            return;
        }

        lastUsedWeapon = weapon;

        if (weapon.IsMelee)
        {
            PerformMeleeAttack(weapon);
        }
        else
        {
            GameObject enemy = DetectEnemyWithAimAssist(aimAssistMaxDistance, aimAssistAngle);
            if (enemy != null)
            {
                DamageData data = weapon.GetDamageData(gameObject);
                Vector3 knockbackDir = (enemy.transform.position - transform.position).normalized;
                float knockbackForce = 3f;

                ApplyDamage(enemy, data, knockbackDir, knockbackForce);

                Debug.Log($"Used {slot} weapon: {weapon.itemName} | Damage: {data.FinalAmount}");
            }
            else
            {
                Debug.Log("No enemy detected in front.");
            }
        }

        StartWeaponCooldown(weapon);
    }

    private bool IsWeaponOnCooldown(WeaponItem weapon)
    {
        return weaponCooldownTimers.ContainsKey(weapon) && weaponCooldownTimers[weapon] > 0f;
    }

    private void StartWeaponCooldown(WeaponItem weapon)
    {
        weaponCooldownTimers[weapon] = weapon.cooldownDuration;
    }

    private void PerformMeleeAttack(WeaponItem meleeWeapon)
    {
        Vector3 origin = transform.position + Vector3.up * 1.0f;
        Vector3 forward = transform.forward;
        float meleeRange = meleeWeapon.MeleeRange;
        float meleeAngle = meleeWeapon.MeleeAngle;

        List<GameObject> hitEnemies = GetMeleeHits(origin, forward, meleeRange, meleeAngle, enemyLayerMask, "Enemy");

        int hitCount = 0;
        foreach (var enemyObj in hitEnemies)
        {
            DamageData data = meleeWeapon.GetDamageData(gameObject);
            Vector3 knockbackDir = (enemyObj.transform.position - transform.position).normalized;
            float knockbackForce = 5f;

            ApplyDamage(enemyObj, data, knockbackDir, knockbackForce);
            hitCount++;
        }

        Debug.Log(hitCount > 0
            ? $"Melee attack hit {hitCount} enemies with {meleeWeapon.itemName}"
            : "Melee attack hit no enemies.");

        ShowMeleeSweepVisual(meleeRange, meleeAngle);
    }

    private void ShowMeleeSweepVisual(float range, float angle)
    {
        if (sliceMeshPrefab == null)
        {
            Debug.LogWarning("SliceMesh prefab not assigned!");
            return;
        }

        Vector3 position = transform.position + Vector3.up * 1.0f;
        Quaternion rotation = Quaternion.LookRotation(transform.forward, Vector3.up);

        GameObject slice = Instantiate(sliceMeshPrefab, position, rotation);

        SliceMesh sliceMesh = slice.GetComponent<SliceMesh>();
        if (sliceMesh != null)
        {
            sliceMesh.radius = range;
            sliceMesh.angle = angle;
            sliceMesh.duration = meleeVisualDuration;
        }
    }

    private GameObject DetectEnemyWithAimAssist(float maxDistance = 10f, float aimAssistAngle = 15f)
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;
        Vector3 forward = transform.forward;

        Collider[] hitColliders = Physics.OverlapSphere(rayOrigin, maxDistance);
        GameObject bestTarget = null;
        float closestAngle = aimAssistAngle;

        foreach (var collider in hitColliders)
        {
            GameObject rootObj = collider.transform.root.gameObject;
            if (!rootObj.CompareTag("Enemy"))
                continue;

            Vector3 directionToEnemy = (rootObj.transform.position - rayOrigin).normalized;
            float angle = Vector3.Angle(forward, directionToEnemy);

            if (angle < closestAngle)
            {
                if (Physics.Raycast(rayOrigin, directionToEnemy, out RaycastHit hit, maxDistance))
                {
                    if (hit.collider.transform.root.gameObject == rootObj)
                    {
                        closestAngle = angle;
                        bestTarget = rootObj;
                    }
                }
            }
        }

        return bestTarget;
    }

    public GameObject GetAimAssistTarget()
    {
        return DetectEnemyWithAimAssist(aimAssistMaxDistance, aimAssistAngle);
    }

    private void OnDrawGizmosSelected()
    {
        if (lastUsedWeapon == null || !lastUsedWeapon.IsMelee)
            return;

        Gizmos.color = Color.red;
        Vector3 origin = transform.position + Vector3.up * 1.0f;
        float range = lastUsedWeapon.MeleeRange;
        float angle = lastUsedWeapon.MeleeAngle;

        Gizmos.DrawWireSphere(origin, range);

        Vector3 forward = transform.forward;
        Vector3 leftDir = Quaternion.Euler(0, -angle / 2f, 0) * forward * range;
        Vector3 rightDir = Quaternion.Euler(0, angle / 2f, 0) * forward * range;

        Gizmos.DrawLine(origin, origin + leftDir);
        Gizmos.DrawLine(origin, origin + rightDir);
    }

    private void OnDrawGizmos()
    {
        if (lastUsedWeapon == null || !lastUsedWeapon.IsMelee)
            return;

        Gizmos.color = Color.green;

        Vector3 origin = transform.position + Vector3.up * 1.0f;
        float range = lastUsedWeapon.MeleeRange;
        float angle = lastUsedWeapon.MeleeAngle;

        Vector3 forward = transform.forward;

        Vector3 leftEdgeDir = Quaternion.Euler(0, -angle / 2f, 0) * forward;
        Vector3 rightEdgeDir = Quaternion.Euler(0, angle / 2f, 0) * forward;

        Gizmos.DrawRay(origin, leftEdgeDir * range);
        Gizmos.DrawRay(origin, rightEdgeDir * range);
    }
}
