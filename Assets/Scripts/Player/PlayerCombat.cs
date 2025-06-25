using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : Combat
{
    [SerializeField] private PlayerInventory inventory;

    [Header("Melee Sweep Visual")]
    [SerializeField] private LineRenderer meleeSweepRenderer;
    [SerializeField] private float meleeVisualDuration = 0.5f;

    private Coroutine meleeVisualCoroutine;

    // Track the last used weapon item
    private WeaponItem lastUsedWeapon;
    public WeaponItem LastUsedWeapon => lastUsedWeapon;

    // Track cooldown timers per weapon instance
    private Dictionary<WeaponItem, float> weaponCooldownTimers = new Dictionary<WeaponItem, float>();

    private void Update()
    {
        // Update cooldown timers per weapon
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

        // Check if weapon is on cooldown
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
            GameObject enemy = DetectEnemyWithAimAssist(weapon.RangedDistance, 15f);
            if (enemy != null)
            {
                DamageData data = weapon.GetDamageData(gameObject);
                Vector3 knockbackDir = (enemy.transform.position - transform.position).normalized;
                float knockbackForce = 3f;

                Health targetHealth = enemy.GetComponent<Health>();
                if (targetHealth != null)
                    targetHealth.InflictDamage(data, knockbackDir, knockbackForce);

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
        float meleeRange = meleeWeapon.MeleeRange;
        float meleeAngle = meleeWeapon.MeleeAngle;

        ShowMeleeSweepVisual(meleeRange, meleeAngle);

        Vector3 origin = transform.position + Vector3.up * 1.0f;
        Vector3 forward = transform.forward;

        Collider[] hits = Physics.OverlapSphere(origin, meleeRange);
        int hitCount = 0;

        foreach (var hit in hits)
        {
            GameObject rootObj = hit.transform.root.gameObject;
            if (!rootObj.CompareTag("Enemy"))
                continue;

            Vector3 directionToTarget = (rootObj.transform.position - origin).normalized;
            float angleToTarget = Vector3.Angle(forward, directionToTarget);

            if (angleToTarget <= meleeAngle / 2f)
            {
                DamageData data = meleeWeapon.GetDamageData(gameObject);
                Vector3 knockbackDir = (rootObj.transform.position - transform.position).normalized;
                float knockbackForce = 5f;

                Health targetHealth = rootObj.GetComponent<Health>();
                if (targetHealth != null)
                    targetHealth.InflictDamage(data, knockbackDir, knockbackForce);

                hitCount++;
            }
        }

        if (hitCount > 0)
            Debug.Log($"Melee attack hit {hitCount} enemies with {meleeWeapon.itemName}");
        else
            Debug.Log("Melee attack hit no enemies.");
    }

    private void ShowMeleeSweepVisual(float range, float angle)
    {
        if (meleeSweepRenderer == null)
            return;

        if (meleeVisualCoroutine != null)
            StopCoroutine(meleeVisualCoroutine);

        meleeVisualCoroutine = StartCoroutine(DrawMeleeSweepArc(range, angle));
    }

    private IEnumerator DrawMeleeSweepArc(float radius, float arcAngle)
    {
        int segments = 30;
        float halfAngle = arcAngle / 2f;

        Vector3 origin = transform.position + Vector3.up * 1.0f;
        Vector3 forward = transform.forward;

        meleeSweepRenderer.positionCount = segments + 2;
        meleeSweepRenderer.SetPosition(0, origin);

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, (float)i / segments);
            Quaternion rotation = Quaternion.AngleAxis(currentAngle, Vector3.up);
            Vector3 point = origin + rotation * forward * radius;
            meleeSweepRenderer.SetPosition(i + 1, point);
        }

        meleeSweepRenderer.enabled = true;

        yield return new WaitForSeconds(meleeVisualDuration);

        meleeSweepRenderer.enabled = false;
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
        return DetectEnemyWithAimAssist(10f, 15f);
    }
}
