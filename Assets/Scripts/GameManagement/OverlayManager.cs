using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class CooldownUIData
{
    public Slot slot;
    public Slider cooldownSlider;
    public Image cooldownFillImage;
    public Color cooldownColor = Color.red;
}

public class OverlayManager : MonoBehaviour
{
    public static OverlayManager Instance;

    [SerializeField] private CanvasGroup gamepadOverlay;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider shieldSlider;

    [Header("Shield Bar Colors")]
    [SerializeField] private Image shieldSliderFillImage;
    [SerializeField] private Color shieldColor = Color.cyan;

    [Header("Slot UI Data")]
    [SerializeField] private List<SlotData> slotUIData;

    [Header("Cooldown UI Data Per Slot")]
    [SerializeField] private List<CooldownUIData> cooldownUIDataList;

    public bool showOverlay = true;
    private bool lastState = true;
    private Health playerHealth;

    // Track cooldown timers per slot
    private Dictionary<Slot, float> slotCurrentCooldowns = new Dictionary<Slot, float>();
    private Dictionary<Slot, float> slotMaxCooldowns = new Dictionary<Slot, float>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = playerHealth != null ? playerHealth.CurrentHealth : 100f;
            healthSlider.value = playerHealth != null ? playerHealth.CurrentHealth : 100f;
        }

        if (shieldSlider != null)
        {
            shieldSlider.maxValue = 1f;
            shieldSlider.value = playerHealth != null ? Mathf.Clamp01(playerHealth.CurrentShield / 100f) : 0f;
        }

        if (shieldSliderFillImage != null)
        {
            shieldSliderFillImage.color = shieldColor;
        }

        // Hide all cooldown sliders initially
        foreach (var cdData in cooldownUIDataList)
        {
            if (cdData.cooldownSlider != null)
                cdData.cooldownSlider.gameObject.SetActive(false);
        }

        UpdateOverlayVisibility();
        lastState = showOverlay;

        // Initial slot highlight
        if (PlayerInventory.Instance != null)
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
            {
                healthSlider.maxValue = playerHealth.maxHealth;
                healthSlider.value = playerHealth.CurrentHealth;   
            }

            if (shieldSlider != null)
                shieldSlider.value = Mathf.Clamp01(playerHealth.CurrentShield / 100f);
        }

        // Update cooldown timers per slot
        float deltaTime = Time.deltaTime;
        var slotsToClear = new List<Slot>();
        var keys = new List<Slot>(slotCurrentCooldowns.Keys);

        foreach (var slot in keys)
        {
            float current = slotCurrentCooldowns[slot] - deltaTime;

            if (current <= 0f)
            {
                current = 0f;
                slotsToClear.Add(slot);
            }

            slotCurrentCooldowns[slot] = current;
            UpdateCooldownUI(slot, current);
        }

        // Clear cooldowns finished
        foreach (var slot in slotsToClear)
        {
            SetCooldownUIActive(slot, false);
            slotCurrentCooldowns.Remove(slot);
            slotMaxCooldowns.Remove(slot);
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

    public void ToggleOverlay(bool show)
    {
        showOverlay = show;

        if (healthSlider != null) healthSlider.gameObject.SetActive(show);
        if (shieldSlider != null) shieldSlider.gameObject.SetActive(show);

        // Also toggle cooldown sliders visibility accordingly
        foreach (var cdData in cooldownUIDataList)
        {
            if (cdData.cooldownSlider != null)
                cdData.cooldownSlider.gameObject.SetActive(show && slotCurrentCooldowns.ContainsKey(cdData.slot));
        }
    }

    // Call this to start a cooldown timer on a specific slot
    public void StartCooldown(Slot slot, float maxDuration)
    {
        if (!cooldownUIDataList.Exists(x => x.slot == slot))
        {
            Debug.LogWarning($"No cooldown UI configured for slot {slot}");
            return;
        }

        slotMaxCooldowns[slot] = maxDuration;
        slotCurrentCooldowns[slot] = maxDuration;

        SetCooldownUIActive(slot, true);
        UpdateCooldownUI(slot, maxDuration);
    }

    // Update the cooldown value for the last used weapon's slot
    public void UpdateCooldownValue(float value)
    {
        if (PlayerInventory.Instance == null) return;

        var lastSlot = PlayerInventory.Instance.GetLastSelectedSlot();
        if (slotCurrentCooldowns.ContainsKey(lastSlot))
        {
            slotCurrentCooldowns[lastSlot] = value;
            UpdateCooldownUI(lastSlot, value);
        }
    }

    public void ClearCooldownDisplay()
    {
        if (PlayerInventory.Instance == null) return;

        var lastSlot = PlayerInventory.Instance.GetLastSelectedSlot();
        if (slotCurrentCooldowns.ContainsKey(lastSlot))
        {
            SetCooldownUIActive(lastSlot, false);
            slotCurrentCooldowns.Remove(lastSlot);
            slotMaxCooldowns.Remove(lastSlot);
        }
    }

    private void UpdateCooldownUI(Slot slot, float currentValue)
    {
        var cdData = cooldownUIDataList.Find(x => x.slot == slot);
        if (cdData == null || cdData.cooldownSlider == null) return;

        if (slotMaxCooldowns.TryGetValue(slot, out float maxVal))
        {
            cdData.cooldownSlider.value = currentValue / maxVal;
        }
        else
        {
            cdData.cooldownSlider.value = 0f;
        }
    }
    public void SetCooldown(float current, float max)
    {
        if (PlayerInventory.Instance == null) return;

        var slot = PlayerInventory.Instance.GetLastSelectedSlot();
        StartCooldown(slot, max);
        UpdateCooldownValue(current);
    }

    private void SetCooldownUIActive(Slot slot, bool active)
    {
        var cdData = cooldownUIDataList.Find(x => x.slot == slot);
        if (cdData == null || cdData.cooldownSlider == null) return;

        cdData.cooldownSlider.gameObject.SetActive(active);
    }

    public void ShowActiveSlot(Slot slot)
    {
        foreach (var data in slotUIData)
        {
            bool isActive = data.slot == slot;

            if (data.iconImage != null)
            {
                data.iconImage.sprite = isActive ? data.selectedSprite : data.normalSprite;
                data.iconImage.color = Color.white; // Optional: Reset color in case it was modified elsewhere
            }
        }
    }

}
