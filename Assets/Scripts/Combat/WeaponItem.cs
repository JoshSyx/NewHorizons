using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Items/Weapon")]
public class WeaponItem : Item
{
    public WeaponSlot slot;
    public float baseDamage = 10f;
    public DamageType damageType = DamageType.Melee;

    public bool IsDashAbility = false;
    public bool IsMelee => damageType == DamageType.Melee;
    public bool IsRanged => damageType == DamageType.Ranged;

    [Header("Melee Settings")]
    [Tooltip("Melee attack range in units")]
    public float MeleeRange = 2f;
    [Tooltip("Arc angle of melee sweep in degrees")]
    public float MeleeAngle = 90f;

    [Header("Ranged Settings")]
    [Tooltip("Ranged attack range in units")]
    public float RangedDistance = 20f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;

    [Header("Projectile Behavior")]
    [Tooltip("Whether gravity affects the projectile")]
    public bool useGravity = true;

    [Tooltip("How long before the projectile despawns after sticking")]
    public float stickDuration = 5f;

    [Tooltip("Total time before projectile despawns regardless of collision")]
    public float maxLifetime = 20f;

    [Tooltip("Force applied on impact")]
    public float knockbackForce = 3f;

    [Header("Cooldown Settings")]
    [Tooltip("Cooldown duration in seconds before the weapon can be used again")]
    public float cooldownDuration = 1f;

    private void OnEnable()
    {
        itemType = ItemType.Weapon;
    }

    public DamageData GetDamageData(GameObject source)
    {
        return new DamageData(baseDamage, damageType, source);
    }
}
