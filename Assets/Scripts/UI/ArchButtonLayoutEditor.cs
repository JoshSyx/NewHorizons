using UnityEngine;

[ExecuteInEditMode]
public class ArchButtonLayoutEditor : MonoBehaviour
{
    public RectTransform container; // Parent container
    public RectTransform[] buttons;

    [Tooltip("Start and End angles of the arch (in degrees)")]
    public Vector2 angleRange = new Vector2(0f, 180f);

    public float maxRadius = 200f;
    public float minRadius = 50f;
    public float maxButtonSize = 100f;
    public float minButtonSize = 30f;

    private void OnValidate()
    {
        if (container == null || buttons == null || buttons.Length == 0)
            return;

        ArrangeButtonsInArch();
    }

    public void ArrangeButtonsInArch()
    {
        int count = buttons.Length;
        if (count == 0) return;

        float startAngle = angleRange.x;
        float endAngle = angleRange.y;

        float arc = Mathf.Abs(endAngle - startAngle);
        float angleStep = count > 1 ? arc / (count - 1) : 0;

        float buttonSize = buttons[0] != null ? buttons[0].rect.width : maxButtonSize;
        float angleStepRad = Mathf.Deg2Rad * angleStep;

        float neededRadius = buttonSize / (angleStepRad > 0 ? angleStepRad : 1);

        float radius = Mathf.Clamp(neededRadius, minRadius, maxRadius);

        float arcLengthPerButton = radius * angleStepRad;
        if (arcLengthPerButton < buttonSize)
        {
            buttonSize = Mathf.Clamp(arcLengthPerButton, minButtonSize, maxButtonSize);
            foreach (var btn in buttons)
            {
                if (btn == null) continue;
                btn.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, buttonSize);
                btn.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, buttonSize);
            }
        }

        for (int i = 0; i < count; i++)
        {
            if (buttons[i] == null) continue;
            float angle = startAngle + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            float x = radius * Mathf.Cos(rad);
            float y = radius * Mathf.Sin(rad);

            buttons[i].anchoredPosition = new Vector2(x, y);
        }
    }
}
