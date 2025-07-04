using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth { get; private set; } = 100f;
    private float currentHealth;
    private bool isDead = false;

    [Header("Shield Settings")]
    private float currentShield = 0f;
    private float shieldReductionPercent = 0f;
    private float shieldTimer = 0f;

    [Header("Invincibility Settings")]
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private bool invincibilityOnCooldown = false;
    private float invincibilityCooldownRemaining = 0f;
    private float invincibilityCooldownDuration = 0f;

    [Header("User Feedback")]
    [SerializeField] private Material idle;
    [SerializeField] private Material getHit;
    [SerializeField] private Material die;

    private List<Renderer> _allRenderers = new List<Renderer>();
    private List<Material[]> _originalMaterials = new List<Material[]>();
    private Coroutine flashCoroutine;

    public float CurrentHealth => currentHealth;
    public float CurrentShield => currentShield;
    public bool IsAlive => currentHealth > 0f;

    private float shieldCooldownRemaining = 0f;
    private float shieldCooldownDuration = 0f;
    private bool shieldOnCooldown = false;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;

        _allRenderers.AddRange(GetComponentsInChildren<Renderer>());

        foreach (var renderer in _allRenderers)
        {
            // Store original materials as the "idle" state
            _originalMaterials.Add(renderer.materials.Clone() as Material[]);
        }
    }

    private void Update()
    {
        if (shieldTimer > 0f)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                currentShield = 0f;
                shieldReductionPercent = 0f;
                Debug.Log($"{gameObject.name}'s shield expired.");
            }
        }

        if (shieldOnCooldown)
        {
            shieldCooldownRemaining -= Time.deltaTime;
            shieldCooldownRemaining = Mathf.Max(0f, shieldCooldownRemaining);

            OverlayManager.Instance?.UpdateCooldownValue(shieldCooldownRemaining);

            if (shieldCooldownRemaining <= 0f)
            {
                shieldOnCooldown = false;
                OverlayManager.Instance?.ClearCooldownDisplay();
                Debug.Log($"{gameObject.name}'s shield cooldown ended.");
            }
        }

        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                Debug.Log($"{gameObject.name} is no longer invincible.");
            }
        }

        if (invincibilityOnCooldown)
        {
            invincibilityCooldownRemaining -= Time.deltaTime;
            invincibilityCooldownRemaining = Mathf.Max(0f, invincibilityCooldownRemaining);

            OverlayManager.Instance?.UpdateCooldownValue(invincibilityCooldownRemaining);

            if (invincibilityCooldownRemaining <= 0f)
            {
                invincibilityOnCooldown = false;
                OverlayManager.Instance?.ClearCooldownDisplay();
                Debug.Log($"{gameObject.name}'s invincibility cooldown ended.");
            }
        }
    }

    public void InflictDamage(DamageData damageData)
    {
        if (isDead) return;
        if (isInvincible)
        {
            Debug.Log($"{gameObject.name} is invincible and took no damage.");
            return;
        }

        float finalDamage = damageData.FinalAmount;

        if (currentShield > 0f)
        {
            float reducedDamage = finalDamage * (1f - shieldReductionPercent);
            float absorbed = finalDamage - reducedDamage;

            currentShield -= absorbed;

            if (currentShield <= 0f)
            {
                Debug.Log($"{gameObject.name}'s shield broke!");
                currentShield = 0f;
            }

            finalDamage = reducedDamage;
        }

        currentHealth = Mathf.Max(0f, currentHealth - finalDamage);

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashMaterial(getHit, 0.15f));

        if (currentHealth == 0f)
        {
            Kill();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, Mathf.Max(maxHealth * amount, currentHealth));
    }

    public void ApplyShield(AbilityItem ability)
    {
        if (ability == null || ability.shieldAmount <= 0f) return;

        if (shieldOnCooldown)
        {
            Debug.Log($"{gameObject.name} tried to activate shield, but it's on cooldown!");
            return;
        }

        currentShield = ability.shieldAmount;
        shieldReductionPercent = Mathf.Clamp01(ability.shieldDamageReductionPercent);
        shieldTimer = ability.shieldDuration;

        shieldCooldownDuration = ability.cooldownDuration;
        shieldCooldownRemaining = shieldCooldownDuration;
        shieldOnCooldown = true;

        OverlayManager.Instance?.SetCooldown(shieldCooldownDuration, shieldCooldownRemaining);

        Debug.Log($"{gameObject.name} gained a shield for {currentShield} damage (Duration: {shieldTimer}s, Reduction: {shieldReductionPercent * 100f}%)");
    }

    public void ApplyInvincibility(AbilityItem ability)
    {
        if (ability == null || ability.invincibilityDuration <= 0f) return;

        if (invincibilityOnCooldown)
        {
            Debug.Log($"{gameObject.name} tried to activate invincibility, but it's on cooldown!");
            return;
        }

        ActivateInvincibility(ability.invincibilityDuration, ability.cooldownDuration);
    }

    public void ActivateInvincibility(float duration, float cooldown = 0f)
    {
        if (duration <= 0f) return;

        isInvincible = true;
        invincibilityTimer = duration;

        if (cooldown > 0f)
        {
            invincibilityCooldownDuration = cooldown;
            invincibilityCooldownRemaining = cooldown;
            invincibilityOnCooldown = true;

            OverlayManager.Instance?.SetCooldown(invincibilityCooldownDuration, invincibilityCooldownRemaining);
        }

        Debug.Log($"{gameObject.name} is now invincible for {duration} seconds.");
    }

    public virtual void Kill()
    {
        if (isDead) return;
        isDead = true;

        SoulManager.Instance?.RegisterKill();

        StartCoroutine(HandleDeathVisuals());
    }

    private IEnumerator FlashMaterial(Material flashMaterial, float duration)
    {
        if (flashMaterial != null)
        {
            foreach (var renderer in _allRenderers)
            {
                Material[] flashMats = new Material[renderer.materials.Length];
                for (int i = 0; i < flashMats.Length; i++)
                    flashMats[i] = flashMaterial;

                renderer.materials = flashMats;
            }

            yield return new WaitForSeconds(duration);
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }

        // Restore original materials
        for (int i = 0; i < _allRenderers.Count; i++)
        {
            _allRenderers[i].materials = _originalMaterials[i];
        }
    }

    private IEnumerator HandleDeathVisuals()
    {
        if (die != null)
        {
            foreach (var renderer in _allRenderers)
            {
                Material[] dieMats = new Material[renderer.materials.Length];
                for (int i = 0; i < dieMats.Length; i++)
                    dieMats[i] = die;

                renderer.materials = dieMats;
            }

            yield return new WaitForSeconds(0.3f);
        }

        Destroy(gameObject);
    }

    public void SetInvincibleForDuration(float duration)
    {
        if (duration <= 0f) return;

        isInvincible = true;
        invincibilityTimer = duration;

        Debug.Log($"{gameObject.name} is now invincible for {duration} seconds.");
    }

    public void MultiplyMaxHealth(float multiplier)
    {
        maxHealth = Mathf.Min(1, maxHealth * multiplier);
        Heal(1);
    }
}
