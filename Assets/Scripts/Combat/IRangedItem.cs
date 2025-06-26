using UnityEngine;

public interface IRangedItem
{
    GameObject projectilePrefab { get; }
    float projectileSpeed { get; }
    bool useGravity { get; }
    float stickDuration { get; }
    float knockbackForce { get; }
    float maxLifetime { get; }
    DamageData GetDamageData(GameObject source);
}
