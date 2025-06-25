using UnityEngine;
using UnityEngine.UI;

public class OverlayManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup gamepadOverlay;
    [SerializeField] private Slider healthSlider;  // Assign in Inspector

    [Tooltip("Toggle this to show/hide the gamepad overlay manually.")]
    public bool showOverlay = false;

    private bool lastState = false;
    private Health playerHealth;

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance._player != null)
        {
            playerHealth = GameManager.Instance._player.GetComponent<Health>();

            if (playerHealth != null && healthSlider != null)
            {
                healthSlider.maxValue = playerHealth.CurrentHealth;
                healthSlider.value = playerHealth.CurrentHealth;
            }
        }

        UpdateOverlayVisibility();
        lastState = showOverlay;
    }

    private void Update()
    {
        if (showOverlay != lastState)
        {
            UpdateOverlayVisibility();
            lastState = showOverlay;
        }

        if (showOverlay && playerHealth != null && healthSlider != null)
        {
            healthSlider.value = playerHealth.CurrentHealth;
        }
    }

    private void UpdateOverlayVisibility()
    {
        if (gamepadOverlay != null)
        {
            gamepadOverlay.alpha = showOverlay ? 1f : 0f;
            gamepadOverlay.interactable = showOverlay;
            gamepadOverlay.blocksRaycasts = showOverlay;
        }
    }
}
