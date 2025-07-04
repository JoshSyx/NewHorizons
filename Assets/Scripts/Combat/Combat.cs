using UnityEngine;
using System.Collections.Generic;

public class Combat : MonoBehaviour
{
    // Apply damage and knockback here
    public void ApplyDamage(GameObject target, DamageData data, Vector3 knockbackDir = default, float knockbackForce = 0f)
    {
        if (!target.TryGetComponent(out Health targetHealth)) return;
        Debug.Log(data.Multiplier);
        // Apply damage
        targetHealth.InflictDamage(data);

        // Apply knockback if needed
        if (knockbackDir != Vector3.zero && knockbackForce > 0f)
        {
            if (target.TryGetComponent(out Rigidbody rb))
            {
                rb.AddForce(knockbackDir.normalized * knockbackForce, ForceMode.Impulse);
            }
        }
    }

    // Melee hit detection
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
            hitTargets.Add(rootObj);
        }

        return hitTargets;
    }

    // Unified ranged attack spawn for any IRangedItem (WeaponItem or AbilityItem)
    protected void PerformRangedAttack(IRangedItem rangedItem, Vector3 origin, Vector3 targetPosition)
    {
        if (rangedItem.projectilePrefab == null)
        {
            Debug.LogWarning("Projectile prefab not set on ranged item.");
            return;
        }

        Vector3 direction = (targetPosition - origin).normalized;
        Vector3 projectileVelocity = direction * rangedItem.projectileSpeed;
        Quaternion rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(-90f, 0f, 0f);

        GameObject projectile = Instantiate(rangedItem.projectilePrefab, origin, rotation);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = projectileVelocity;

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.Initialize(
                rangedItem.GetDamageData(gameObject),
                gameObject,
                projectileVelocity,
                rangedItem
            );
        }
    }

    public void PerformExplosionAttack(Vector3 center, float radius, float damage, DamageType damageType, GameObject source, float knockbackForce = 0f)
    {
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);
        foreach (Collider hit in hitColliders)
        {
            GameObject target = hit.transform.root.gameObject;
            if (target == gameObject) continue;
            if (target.TryGetComponent(out Health health))
            {
                DamageData data = new DamageData(damage, damageType, source);
                health.InflictDamage(data);
            }

            if (hit.TryGetComponent(out Rigidbody rb))
            {
                Vector3 knockbackDir = (hit.transform.position - center).normalized;
                rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
            }
        }

        // Optional: instantiate explosion VFX here
    }

}
