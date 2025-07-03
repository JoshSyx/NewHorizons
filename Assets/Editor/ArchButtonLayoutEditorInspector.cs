using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArchButtonLayoutEditor))]
public class ArchButtonLayoutEditorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        ArchButtonLayoutEditor script = (ArchButtonLayoutEditor)target;

        DrawDefaultInspector();

        EditorGUILayout.LabelField("Angle Range");

        float min = script.angleRange.x;
        float max = script.angleRange.y;

        EditorGUILayout.MinMaxSlider(ref min, ref max, 0f, 360f);

        // Clamp values so min ≤ max
        min = Mathf.Clamp(min, 0f, max);
        max = Mathf.Clamp(max, min, 360f);

        if (min != script.angleRange.x || max != script.angleRange.y)
        {
            Undo.RecordObject(script, "Change Angle Range");
            script.angleRange = new Vector2(min, max);
            EditorUtility.SetDirty(script);
            script.ArrangeButtonsInArch();
        }
    }
}
