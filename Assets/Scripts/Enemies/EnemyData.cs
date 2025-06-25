using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("General")]
    public float speed = 8f;
    public float followDistance = 5f;
    public float distanceTolerance = 0.5f;
    public bool keepDistance = true;

    [Header("Movement Smoothing")]
    public float directionSmoothness = 0.1f;

    [Header("Avoidance")]
    public float avoidanceRadius = 2f;
    public float avoidanceStrength = 15f;
    public LayerMask enemyLayer;

    [Header("Flight")]
    public bool isFlying = false;
    public float flyingHeight = 5f;
    public float verticalSmoothness = 0.2f;

    [Header("Combat")]
    public WeaponItem equippedWeapon;       // Reference to weapon asset
    public float attackCooldown = 1.5f;     // Seconds between attacks
}
