//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(AbilityItem))]
//public class AbilityItemEditor : Editor
//{
//    // Base class (Item) properties
//    private SerializedProperty itemNameProp;
//    private SerializedProperty iconProp;
//    private SerializedProperty worldModelPrefabProp;

//    // AbilityItem properties
//    private SerializedProperty abilitySlotProp;
//    private SerializedProperty abilityTypeProp;

//    private SerializedProperty damageTypeProp;
//    private SerializedProperty baseDamageProp;

//    // Dash props
//    private SerializedProperty dashSpeedProp;
//    private SerializedProperty dashDurationProp;
//    private SerializedProperty dashCooldownProp;

//    // Melee props
//    private SerializedProperty meleeRangeProp;
//    private SerializedProperty meleeAngleProp;

//    // Ranged props
//    private SerializedProperty rangedDistanceProp;
//    private SerializedProperty projectilePrefabProp;
//    private SerializedProperty projectileSpeedProp;

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

//        // AbilityItem properties
//        abilitySlotProp = serializedObject.FindProperty("slot");
//        abilityTypeProp = serializedObject.FindProperty("abilityType");

//        damageTypeProp = serializedObject.FindProperty("damageType");
//        baseDamageProp = serializedObject.FindProperty("baseDamage");

//        dashSpeedProp = serializedObject.FindProperty("dashSpeed");
//        dashDurationProp = serializedObject.FindProperty("dashDuration");
//        dashCooldownProp = serializedObject.FindProperty("dashCooldown");

//        meleeRangeProp = serializedObject.FindProperty("MeleeRange");
//        meleeAngleProp = serializedObject.FindProperty("MeleeAngle");

//        rangedDistanceProp = serializedObject.FindProperty("RangedDistance");
//        projectilePrefabProp = serializedObject.FindProperty("projectilePrefab");
//        projectileSpeedProp = serializedObject.FindProperty("projectileSpeed");

//        useGravityProp = serializedObject.FindProperty("useGravity");
//        stickDurationProp = serializedObject.FindProperty("stickDuration");
//        maxLifetimeProp = serializedObject.FindProperty("maxLifetime");
//        knockbackForceProp = serializedObject.FindProperty("knockbackForce");

//        cooldownDurationProp = serializedObject.FindProperty("cooldownDuration");
//    }

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();

//        // Draw base Item properties
//        EditorGUILayout.LabelField("Base Item Settings", EditorStyles.boldLabel);
//        EditorGUILayout.PropertyField(itemNameProp);
//        EditorGUILayout.PropertyField(iconProp);
//        EditorGUILayout.PropertyField(worldModelPrefabProp);

//        EditorGUILayout.Space();

//        // Draw AbilityItem base fields
//        EditorGUILayout.LabelField("Ability Settings", EditorStyles.boldLabel);
//        EditorGUILayout.PropertyField(abilitySlotProp);
//        EditorGUILayout.PropertyField(abilityTypeProp);
//        EditorGUILayout.PropertyField(damageTypeProp);
//        EditorGUILayout.PropertyField(baseDamageProp);

//        EditorGUILayout.Space();

//        // Draw AbilityType-specific settings
//        AbilityType abilityType = (AbilityType)abilityTypeProp.enumValueIndex;

//        switch (abilityType)
//        {
//            case AbilityType.Dash:
//                EditorGUILayout.LabelField("Dash Settings", EditorStyles.boldLabel);
//                EditorGUILayout.PropertyField(dashSpeedProp);
//                EditorGUILayout.PropertyField(dashDurationProp);
//                EditorGUILayout.PropertyField(dashCooldownProp);
//                break;

//            case AbilityType.Melee:
//                EditorGUILayout.LabelField("Melee Settings", EditorStyles.boldLabel);
//                EditorGUILayout.PropertyField(meleeRangeProp);
//                EditorGUILayout.PropertyField(meleeAngleProp);
//                break;

//            case AbilityType.Ranged:
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

//            default:
//                EditorGUILayout.HelpBox("No specific settings for this ability type.", MessageType.Info);
//                break;
//        }

//        EditorGUILayout.Space();
//        EditorGUILayout.LabelField("Cooldown", EditorStyles.boldLabel);
//        EditorGUILayout.PropertyField(cooldownDurationProp);

//        serializedObject.ApplyModifiedProperties();
//    }
//}
