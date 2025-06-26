using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Items/Weapon")]
public class WeaponItem : Item, IEquippableSlotItem, IRangedItem
{
    public Slot slot;
    public GameObject weaponWorldModelPrefab;

    public float baseDamage = 10f;
    public DamageType damageType = DamageType.Melee;

    public bool IsMelee => damageType == DamageType.Melee;
    public bool IsRanged => damageType == DamageType.Ranged;

    public float MeleeRange = 2f;
    public float MeleeAngle = 90f;

    public float RangedDistance = 20f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;

    public bool useGravity = true;
    public float stickDuration = 5f;
    public float maxLifetime = 20f;
    public float knockbackForce = 3f;

    public float cooldownDuration = 1f;

    private void OnEnable()
    {
        itemType = ItemType.Weapon;
    }

    public DamageData GetDamageData(GameObject source)
    {
        return new DamageData(baseDamage, damageType, source);
    }

    // IEquippableSlotItem implementation
    Slot IEquippableSlotItem.Slot => slot;
    string IEquippableSlotItem.ItemName => itemName;
    GameObject IEquippableSlotItem.WorldModelPrefab => weaponWorldModelPrefab;

    // IRangedItem implementation
    GameObject IRangedItem.projectilePrefab => projectilePrefab;
    float IRangedItem.projectileSpeed => projectileSpeed;

    bool IRangedItem.useGravity => useGravity;
    float IRangedItem.stickDuration => stickDuration;
    float IRangedItem.maxLifetime => maxLifetime;
    float IRangedItem.knockbackForce => knockbackForce;

    DamageData IRangedItem.GetDamageData(GameObject source) => GetDamageData(source);
}
