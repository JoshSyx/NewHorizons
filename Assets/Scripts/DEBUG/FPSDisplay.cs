using TMPro;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    private void Update()
    {
        float fps = 1f / Time.unscaledDeltaTime;
        fpsText.text = $"FPS: {Mathf.RoundToInt(fps)}";
    }
}
