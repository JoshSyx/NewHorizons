using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("General")]
    public float speed = 8f;
    public bool keepDistance = true;
    public float returnSpeedMultiplier = 1.5f;

    [Header("Vision Settings")]
    public float visionDistance = 15f;
    [Range(0, 180)]
    public float visionAngle = 90f;

    [Header("Movement Smoothing")]
    public float directionSmoothness = 0.1f;

    [Header("Flight")]
    public bool isFlying = false;
    public float retreatSpeed = 10f;
    public bool retreatAfterHit = true;

    [Tooltip("Target height above start position to hover at")]
    public float flightHeight = 3f;  // <-- add this

    [Header("Dive Attack (Flying Only)")]
    public float patrolHeight = 10f;
    public float diveSpeed = 15f;
    public float ascendSpeed = 10f;
    public float attackRange = 2f;
    public float hoverAfterAttackTime = 1.5f;

    [Header("Combat")]
    public WeaponItem equippedWeapon;
    public float attackCooldown = 1.5f;

    [Header("Rotation")]
    public float rotationSpeed = 10f;

    [Header("Distance Control")]
    public float minDistance = 3f;
    public float maxDistance = 7f;
    public float adjustDistanceAmount = 1f;

    [Header("AI")]
    public float maxWanderDistance = 20f;
    public float wanderRadius = 5f;
    public float wanderInterval = 3f;
}
