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
    [SerializeField] private LayerMask enemyLayerMask;

    private WeaponItem lastUsedWeapon;
    public WeaponItem LastUsedWeapon => lastUsedWeapon;

    private Dictionary<WeaponItem, float> weaponCooldownTimers = new();

    private WeaponItem currentShootingWeapon;
    private float shootingCooldownTimer = 0f;
    private bool isShooting = false;

    private void Update()
    {
        var keys = new List<WeaponItem>(weaponCooldownTimers.Keys);
        foreach (var weapon in keys)
        {
            weaponCooldownTimers[weapon] -= Time.deltaTime;
            if (weaponCooldownTimers[weapon] <= 0f)
                weaponCooldownTimers.Remove(weapon);
        }

        if (isShooting && currentShootingWeapon != null)
        {
            shootingCooldownTimer -= Time.deltaTime;
            if (shootingCooldownTimer <= 0f && !IsWeaponOnCooldown(currentShootingWeapon))
            {
                PerformRangedAttackWithWeapon(currentShootingWeapon);
                shootingCooldownTimer = currentShootingWeapon.cooldownDuration;
            }
        }
    }

    public void AttackWithSlot(Slot slot)
    {
        if (inventory == null)
        {
            Debug.LogWarning("PlayerInventory reference is missing.");
            return;
        }

        var equippedItem = inventory.GetEquippedItem(slot);
        if (equippedItem == null)
        {
            Debug.Log($"No item equipped in the {slot} slot.");
            return;
        }

        if (equippedItem is WeaponItem weapon)
        {
            if (IsWeaponOnCooldown(weapon))
            {
                Debug.Log($"Weapon {weapon.itemName} is on cooldown.");
                return;
            }

            lastUsedWeapon = weapon;

            if (weapon.IsMelee)
                PerformMeleeAttack(weapon, weapon.knockbackForce);
            else if (weapon.IsRanged)
                PerformRangedAttackWithWeapon(weapon);

            StartWeaponCooldown(weapon);
        }
        else if (equippedItem is AbilityItem ability)
        {
            switch (ability.abilityType)
            {
                case AbilityType.Melee:
                    PerformMeleeAttack(ability, ability.knockbackForce);
                    break;
                case AbilityType.Ranged:
                    PerformRangedAttackWithAbility(ability);
                    break;
                case AbilityType.Dash:
                    break;
                default:
                    Debug.Log($"Unhandled ability type: {ability.abilityType}");
                    break;
            }
        }
        else
        {
            Debug.LogWarning($"Equipped item in slot {slot} is not a WeaponItem or AbilityItem.");
        }
    }

    private void PerformMeleeAttack(WeaponItem meleeWeapon, float knockbackForce)
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
            ApplyDamage(enemyObj, data, knockbackDir, knockbackForce);
            hitCount++;
        }

        Debug.Log(hitCount > 0
            ? $"Melee attack hit {hitCount} enemies with {meleeWeapon.itemName}"
            : "Melee attack hit no enemies.");

        ShowMeleeSweepVisual(meleeRange, meleeAngle);
    }

    private void PerformMeleeAttack(AbilityItem meleeAbility, float knockbackForce)
    {
        Vector3 origin = transform.position + Vector3.up * 1.0f;
        Vector3 forward = transform.forward;
        float meleeRange = meleeAbility.MeleeRange;
        float meleeAngle = meleeAbility.MeleeAngle;

        List<GameObject> hitEnemies = GetMeleeHits(origin, forward, meleeRange, meleeAngle, enemyLayerMask, "Enemy");

        int hitCount = 0;
        foreach (var enemyObj in hitEnemies)
        {
            DamageData data = meleeAbility.GetDamageData(gameObject);
            Vector3 knockbackDir = (enemyObj.transform.position - transform.position).normalized;
            ApplyDamage(enemyObj, data, knockbackDir, knockbackForce);
            hitCount++;
        }

        Debug.Log(hitCount > 0
            ? $"Melee ability hit {hitCount} enemies with {meleeAbility.itemName}"
            : "Melee ability hit no enemies.");

        ShowMeleeSweepVisual(meleeRange, meleeAngle);
    }

    private void PerformRangedAttackWithWeapon(WeaponItem weapon)
    {
        if (weapon == null || !weapon.IsRanged) return;

        float spawnHeight = 1.5f;
        Vector3 spawnPosition = transform.position + Vector3.up * spawnHeight;
        Vector3 aimDirection = GetAimAssistDirection();
        Vector3 aimTarget = spawnPosition + aimDirection * 100f;

        GameObject target = DetectEnemyWithAimAssist(aimAssistMaxDistance, aimAssistAngle);
        if (target != null)
        {
            aimTarget = new Vector3(target.transform.position.x, spawnHeight, target.transform.position.z);
        }

        base.PerformRangedAttack(weapon, spawnPosition, aimTarget);

        lastUsedWeapon = weapon;
        StartWeaponCooldown(weapon);
    }

    private void PerformRangedAttackWithAbility(AbilityItem ability)
    {
        if (ability == null || ability.projectilePrefab == null) return;

        float spawnHeight = 1.5f;
        Vector3 spawnPosition = transform.position + Vector3.up * spawnHeight;
        Vector3 aimDirection = GetAimAssistDirection();
        Vector3 aimTarget = spawnPosition + aimDirection * 100f;

        GameObject target = DetectEnemyWithAimAssist(aimAssistMaxDistance, aimAssistAngle);
        if (target != null)
        {
            aimTarget = new Vector3(target.transform.position.x, spawnHeight, target.transform.position.z);
        }

        base.PerformRangedAttack(ability, spawnPosition, aimTarget);
    }

    private bool IsWeaponOnCooldown(WeaponItem weapon)
    {
        return weaponCooldownTimers.TryGetValue(weapon, out float cooldown) && cooldown > 0f;
    }

    private void StartWeaponCooldown(WeaponItem weapon)
    {
        weaponCooldownTimers[weapon] = weapon.cooldownDuration;
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

    private GameObject DetectEnemyWithAimAssist(float maxDistance = 10f, float aimAssistAngle = 15f)
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;
        Vector3 forward = GetAimAssistDirection(); // ← This is the updated dynamic direction
        Collider[] hitColliders = Physics.OverlapSphere(rayOrigin, maxDistance);

        GameObject bestTarget = null;
        float closestAngle = aimAssistAngle;

        foreach (var collider in hitColliders)
        {
            GameObject rootObj = collider.transform.root.gameObject;
            if (!rootObj.CompareTag("Enemy")) continue;

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

    private Vector3 GetAimAssistDirection()
    {
        Vector2 aimInput = UserInput.Instance.AimInput;
        Vector2 moveInput = UserInput.Instance.MoveInput;

        Vector3 inputDirection = Vector3.zero;

        if (aimInput.sqrMagnitude > 0.01f)
            inputDirection = new Vector3(aimInput.x, 0f, aimInput.y);
        else if (moveInput.sqrMagnitude > 0.01f)
            inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        else
            inputDirection = transform.forward;

        return inputDirection.normalized;
    }

    public GameObject GetAimAssistTarget()
    {
        return DetectEnemyWithAimAssist(aimAssistMaxDistance, aimAssistAngle);
    }

    public void StartShootingWithWeapon(WeaponItem weapon)
    {
        if (weapon == null || !weapon.IsRanged) return;

        if (currentShootingWeapon != weapon)
        {
            currentShootingWeapon = weapon;
            shootingCooldownTimer = 0f;
            isShooting = true;
        }
    }

    public void StopShooting()
    {
        isShooting = false;
        currentShootingWeapon = null;
    }
}
