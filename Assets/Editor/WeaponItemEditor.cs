using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WeaponItem))]
public class WeaponItemEditor : Editor
{
    private SerializedProperty weaponSlotProp;
    private SerializedProperty damageTypeProp;

    // Melee props
    private SerializedProperty meleeRangeProp;
    private SerializedProperty meleeAngleProp;

    // Ranged props
    private SerializedProperty rangedDistanceProp;
    private SerializedProperty projectilePrefabProp;
    private SerializedProperty projectileSpeedProp;

    // Projectile behavior props
    private SerializedProperty useGravityProp;
    private SerializedProperty stickDurationProp;
    private SerializedProperty maxLifetimeProp;
    private SerializedProperty knockbackForceProp;

    // Cooldown prop
    private SerializedProperty cooldownDurationProp;

    private void OnEnable()
    {
        weaponSlotProp = serializedObject.FindProperty("slot");
        damageTypeProp = serializedObject.FindProperty("damageType");

        meleeRangeProp = serializedObject.FindProperty("MeleeRange");
        meleeAngleProp = serializedObject.FindProperty("MeleeAngle");

        rangedDistanceProp = serializedObject.FindProperty("RangedDistance");
        projectilePrefabProp = serializedObject.FindProperty("projectilePrefab");
        projectileSpeedProp = serializedObject.FindProperty("projectileSpeed");

        useGravityProp = serializedObject.FindProperty("useGravity");
        stickDurationProp = serializedObject.FindProperty("stickDuration");
        maxLifetimeProp = serializedObject.FindProperty("maxLifetime");
        knockbackForceProp = serializedObject.FindProperty("knockbackForce");

        cooldownDurationProp = serializedObject.FindProperty("cooldownDuration");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Always show Slot first
        EditorGUILayout.PropertyField(weaponSlotProp);
        EditorGUILayout.PropertyField(damageTypeProp);

        DamageType damageType = (DamageType)damageTypeProp.enumValueIndex;

        EditorGUILayout.Space();

        switch (damageType)
        {
            case DamageType.Melee:
                EditorGUILayout.LabelField("Melee Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(meleeRangeProp);
                EditorGUILayout.PropertyField(meleeAngleProp);
                break;

            case DamageType.Ranged:
                EditorGUILayout.LabelField("Ranged Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(rangedDistanceProp);
                EditorGUILayout.PropertyField(projectilePrefabProp);
                EditorGUILayout.PropertyField(projectileSpeedProp);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Projectile Behavior", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(useGravityProp);
                EditorGUILayout.PropertyField(stickDurationProp);
                EditorGUILayout.PropertyField(maxLifetimeProp);
                EditorGUILayout.PropertyField(knockbackForceProp);
                break;

            default:
                EditorGUILayout.HelpBox("No specific settings for this damage type.", MessageType.Info);
                break;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Cooldown Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(cooldownDurationProp);

        serializedObject.ApplyModifiedProperties();
    }
}
