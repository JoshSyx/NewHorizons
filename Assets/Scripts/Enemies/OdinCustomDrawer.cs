using System;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using System.Collections.Generic;

[Serializable]
public class PrefabPoint
{
    public GameObject prefab;

    [Range(0f, 1f)]
    public float position;
}
public class PrefabRangeBarAttribute : Attribute { }

public class PrefabRangeBarDrawer : OdinAttributeDrawer<PrefabRangeBarAttribute, List<PrefabPoint>>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        var rect = EditorGUILayout.GetControlRect(false, 40);

        // Draw background bar
        EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 1));
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height / 2 - 1, rect.width, 2), Color.gray);

        // Draw prefab points
        for (int i = 0; i < ValueEntry.SmartValue.Count; i++)
        {
            var point = ValueEntry.SmartValue[i];
            float xPos = Mathf.Lerp(rect.x, rect.xMax, point.position);

            var iconRect = new Rect(xPos - 8, rect.y + rect.height / 2 - 8, 16, 16);
            GUI.Box(iconRect, GUIContent.none);

            // Drag handle
            EditorGUI.BeginChangeCheck();
            float newPercent = GUI.HorizontalSlider(
                new Rect(xPos - 25, rect.y + rect.height - 15, 50, 16),
                point.position, 0f, 1f
            );
            if (EditorGUI.EndChangeCheck())
            {
                point.position = Mathf.Clamp01(newPercent);
                GUI.changed = true;
            }

            // Optional: prefab name
            GUI.Label(new Rect(xPos - 50, rect.y, 100, 16), point.prefab ? point.prefab.name : "None", EditorStyles.miniLabel);
        }

        // Draw the default list below (optional)
        this.CallNextDrawer(label);
    }
}