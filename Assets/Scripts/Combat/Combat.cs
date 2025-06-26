using UnityEngine;
using System.Collections.Generic;

public class Combat : MonoBehaviour
{
    // Apply damage helper
    public void ApplyDamage(GameObject target, DamageData data, Vector3 knockbackDir = default, float knockbackForce = 0f)
    {
        if (!target.TryGetComponent(out Health targetHealth)) return;

        targetHealth.InflictDamage(data, knockbackDir, knockbackForce);
    }

    // Check melee hits based on angle and range, returns hit GameObjects
    protected List<GameObject> GetMeleeHits(Vector3 origin, Vector3 forward, float range, float angle, LayerMask layerMask, string targetTag)
    {
        List<GameObject> hitTargets = new List<GameObject>();
        Collider[] hits = Physics.OverlapSphere(origin, range, layerMask);

        foreach (var hit in hits)
        {
            GameObject rootObj = hit.transform.root.gameObject;
            if (!rootObj.CompareTag(targetTag)) continue;
            if (hitTargets.Contains(rootObj)) continue;

            Vector3 directionToTarget = (rootObj.transform.position - origin).normalized;
            float angleToTarget = Vector3.Angle(forward, directionToTarget);
            if (angleToTarget <= angle / 2f)
                hitTargets.Add(rootObj);
        }

        return hitTargets;
    }

    // Basic ranged attack spawn helper
    protected void PerformRangedAttack(WeaponItem weapon, Vector3 origin, Vector3 targetPosition)
    {
        if (weapon.projectilePrefab == null)
        {
            Debug.LogWarning("Projectile prefab not set on weapon: " + weapon.itemName);
            return;
        }

        Vector3 direction = (targetPosition - origin).normalized;
        Vector3 projectileVelocity = direction * weapon.projectileSpeed;

        // Rotate projectile to face direction and align model correctly (e.g., rotate -90 X for arrow)
        Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(-90f, 0f, 0f);
        GameObject projectile = Instantiate(weapon.projectilePrefab, origin, rotation);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = projectileVelocity;

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
            projScript.Initialize(
                weapon.GetDamageData(gameObject),
                gameObject,
                projectileVelocity,
                weapon
            );
    }
}
