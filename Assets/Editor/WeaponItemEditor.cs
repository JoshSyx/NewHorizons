//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(WeaponItem))]
//public class WeaponItemEditor : Editor
//{
//    // Base class (Item) properties
//    private SerializedProperty itemNameProp;
//    private SerializedProperty iconProp;
//    private SerializedProperty worldModelPrefabProp;

//    // WeaponItem properties
//    private SerializedProperty weaponSlotProp;
//    private SerializedProperty damageTypeProp;
//    private SerializedProperty baseDamageProp;

//    // Melee props
//    private SerializedProperty meleeRangeProp;
//    private SerializedProperty meleeAngleProp;

//    // Ranged props
//    private SerializedProperty rangedDistanceProp;
//    private SerializedProperty projectilePrefabProp;
//    private SerializedProperty projectileSpeedProp;

//    // Exploding props
//    private SerializedProperty explosionRadiusProp;
//    private SerializedProperty explosionDamageProp;
//    private SerializedProperty fuseTimeProp;

//    // Projectile behavior props
//    private SerializedProperty useGravityProp;
//    private SerializedProperty stickDurationProp;
//    private SerializedProperty maxLifetimeProp;
//    private SerializedProperty knockbackForceProp;

//    // Cooldown
//    private SerializedProperty cooldownDurationProp;

//    private void OnEnable()
//    {
//        // Base class properties
//        itemNameProp = serializedObject.FindProperty("itemName");
//        iconProp = serializedObject.FindProperty("icon");
//        worldModelPrefabProp = serializedObject.FindProperty("worldModelPrefab");

//        // WeaponItem-specific properties
//        weaponSlotProp = serializedObject.FindProperty("slot");
//        damageTypeProp = serializedObject.FindProperty("damageType");
//        baseDamageProp = serializedObject.FindProperty("baseDamage");

//        meleeRangeProp = serializedObject.FindProperty("range");
//        meleeAngleProp = serializedObject.FindProperty("MeleeAngle");

//        rangedDistanceProp = serializedObject.FindProperty("RangedDistance");
//        projectilePrefabProp = serializedObject.FindProperty("projectilePrefab");
//        projectileSpeedProp = serializedObject.FindProperty("projectileSpeed");

//        // Exploding properties
//        explosionRadiusProp = serializedObject.FindProperty("explosionRadius");
//        explosionDamageProp = serializedObject.FindProperty("explosionDamage");
//        fuseTimeProp = serializedObject.FindProperty("fuseTime");

//        useGravityProp = serializedObject.FindProperty("useGravity");
//        stickDurationProp = serializedObject.FindProperty("stickDuration");
//        maxLifetimeProp = serializedObject.FindProperty("maxLifetime");
//        knockbackForceProp = serializedObject.FindProperty("knockbackForce");

//        cooldownDurationProp = serializedObject.FindProperty("cooldownDuration");
//    }

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();

//        // Draw base Item fields
//        EditorGUILayout.LabelField("Base Item Settings", EditorStyles.boldLabel);
//        EditorGUILayout.PropertyField(itemNameProp);
//        EditorGUILayout.PropertyField(iconProp);
//        EditorGUILayout.PropertyField(worldModelPrefabProp);

//        EditorGUILayout.Space();

//        // Weapon general settings
//        EditorGUILayout.LabelField("Weapon Settings", EditorStyles.boldLabel);
//        EditorGUILayout.PropertyField(weaponSlotProp);
//        EditorGUILayout.PropertyField(damageTypeProp);

//        // Show baseDamage for all types since it’s relevant
//        EditorGUILayout.PropertyField(baseDamageProp);

//        EditorGUILayout.Space();

//        DamageType damageType = (DamageType)damageTypeProp.enumValueIndex;

//        // Conditional fields for each damage type
//        switch (damageType)
//        {
//            case DamageType.Melee:
//                EditorGUILayout.LabelField("Melee Settings", EditorStyles.boldLabel);
//                EditorGUILayout.PropertyField(meleeRangeProp);
//                EditorGUILayout.PropertyField(meleeAngleProp);
//                break;

//            case DamageType.Ranged:
//                EditorGUILayout.LabelField("Ranged Settings", EditorStyles.boldLabel);
//                EditorGUILayout.PropertyField(rangedDistanceProp);
//                EditorGUILayout.PropertyField(projectilePrefabProp);
//                EditorGUILayout.PropertyField(projectileSpeedProp);

//                EditorGUILayout.Space();
//                EditorGUILayout.LabelField("Projectile Behavior", EditorStyles.boldLabel);
//                EditorGUILayout.PropertyField(useGravityProp);
//                EditorGUILayout.PropertyField(stickDurationProp);
//                EditorGUILayout.PropertyField(maxLifetimeProp);
//                EditorGUILayout.PropertyField(knockbackForceProp);
//                break;

//            case DamageType.Explosion:
//                EditorGUILayout.LabelField("Exploding Settings", EditorStyles.boldLabel);
//                EditorGUILayout.PropertyField(explosionRadiusProp);
//                EditorGUILayout.PropertyField(explosionDamageProp);
//                EditorGUILayout.PropertyField(fuseTimeProp);
//                break;

//            default:
//                EditorGUILayout.HelpBox($"No specific settings for damage type {damageType}.", MessageType.Info);
//                break;
//        }

//        EditorGUILayout.Space();

//        // Cooldown
//        EditorGUILayout.LabelField("Cooldown Settings", EditorStyles.boldLabel);
//        EditorGUILayout.PropertyField(cooldownDurationProp);

//        serializedObject.ApplyModifiedProperties();
//    }
//}
