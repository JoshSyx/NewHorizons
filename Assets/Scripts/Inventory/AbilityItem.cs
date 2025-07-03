using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "Items/Ability")]
public class AbilityItem : Item, IEquippableSlotItem, IRangedItem
{
    public Slot slot;
    public GameObject abilityWorldModelPrefab;

    public AbilityType abilityType = AbilityType.None;
    public bool IsContinuousFire = false; // NEW flag to control shooting mode

    public DamageType damageType = DamageType.Effect; // Default for abilities
    public float baseDamage = 0f;

    // Dash settings
    public float dashSpeed = 25f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    // Melee settings
    public float MeleeRange = 2f;
    public float MeleeAngle = 90f;

    // Ranged settings
    public float RangedDistance = 20f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;

    // Projectile behavior
    public bool useGravity = true;
    public float stickDuration = 5f;
    public float maxLifetime = 20f;
    public float knockbackForce = 3f;

    // Shield settings
    public float shieldAmount = 0f;
    public float shieldDuration = 5f;
    [Range(0f, 1f)] public float shieldDamageReductionPercent = 0.75f;

    // Cooldown for all abilities
    public float cooldownDuration = 1f;

    private void OnEnable()
    {
        itemType = ItemType.Ability;
    }

    public DamageData GetDamageData(GameObject source)
    {
        return new DamageData(baseDamage, damageType, source);
    }

    // IEquippableSlotItem implementation
    Slot IEquippableSlotItem.Slot => slot;
    string IEquippableSlotItem.ItemName => itemName;
    GameObject IEquippableSlotItem.WorldModelPrefab => abilityWorldModelPrefab;

    // IRangedItem implementation
    GameObject IRangedItem.projectilePrefab => projectilePrefab;
    float IRangedItem.projectileSpeed => projectileSpeed;

    bool IRangedItem.useGravity => useGravity;
    float IRangedItem.stickDuration => stickDuration;
    float IRangedItem.maxLifetime => maxLifetime;
    float IRangedItem.knockbackForce => knockbackForce;

    DamageData IRangedItem.GetDamageData(GameObject source) => GetDamageData(source);
}
