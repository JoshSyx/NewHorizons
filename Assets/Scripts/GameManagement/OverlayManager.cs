using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class OverlayManager : MonoBehaviour
{
    public static OverlayManager Instance;

    [SerializeField] private CanvasGroup gamepadOverlay;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider shieldSlider;

    [Header("Slot UI Data")]
    [SerializeField] private List<SlotData> slotUIData;

    public bool showOverlay = false;
    private bool lastState = false;
    private Health playerHealth;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance._player != null)
        {
            playerHealth = GameManager.Instance._player.GetComponent<Health>();

            if (playerHealth != null)
            {
                if (healthSlider != null)
                {
                    healthSlider.maxValue = playerHealth.CurrentHealth;
                    healthSlider.value = playerHealth.CurrentHealth;
                }

                if (shieldSlider != null)
                {
                    shieldSlider.maxValue = 1f;
                    shieldSlider.value = 0f;
                }
            }
        }

        UpdateOverlayVisibility();
        lastState = showOverlay;

        // Show initial active slot (from PlayerInventory)
        ShowActiveSlot(PlayerInventory.Instance.GetLastSelectedSlot());
    }

    private void Update()
    {
        if (showOverlay != lastState)
        {
            UpdateOverlayVisibility();
            lastState = showOverlay;
        }

        if (showOverlay && playerHealth != null)
        {
            if (healthSlider != null)
                healthSlider.value = playerHealth.CurrentHealth;

            if (shieldSlider != null)
            {
                float normalizedShield = Mathf.Clamp01(playerHealth.CurrentShield / 100f);
                shieldSlider.value = normalizedShield;
            }
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

    public void ShowActiveSlot(Slot activeSlot)
    {
        foreach (var slotData in slotUIData)
        {
            if (slotData.iconImage == null) continue;

            bool isActive = slotData.slot == activeSlot;
            slotData.iconImage.sprite = isActive ? slotData.selectedSprite : slotData.normalSprite;
        }
    }
}
