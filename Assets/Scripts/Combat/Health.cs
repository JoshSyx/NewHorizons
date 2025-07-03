using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    [Header("Shield Settings")]
    private float currentShield = 0f;
    private float shieldReductionPercent = 0f;
    private float shieldTimer = 0f;

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

    protected virtual void Awake()
    {
        currentHealth = maxHealth;

        _allRenderers.AddRange(GetComponentsInChildren<Renderer>());

        foreach (var renderer in _allRenderers)
        {
            _originalMaterials.Add(renderer.materials);

            if (idle != null)
            {
                Material[] idleMats = new Material[renderer.materials.Length];
                for (int i = 0; i < idleMats.Length; i++)
                    idleMats[i] = idle;

                renderer.materials = idleMats;
            }
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
                Debug.Log($"{gameObject.name}'s shield expired.");
            }
        }
    }

    public void InflictDamage(DamageData damageData)
    {
        if (isDead) return;

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
        if (isDead || amount <= 0f) return;

        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        Debug.Log($"{gameObject.name} healed for {currentHealth - previousHealth} points");

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashMaterial(idle, 0.2f)); // Reuses idle as healing visual
    }

    public void ApplyShield(AbilityItem ability)
    {
        if (ability == null || ability.shieldAmount <= 0f) return;

        currentShield = ability.shieldAmount;
        shieldReductionPercent = Mathf.Clamp01(ability.shieldDamageReductionPercent);
        shieldTimer = ability.shieldDuration;

        Debug.Log($"{gameObject.name} gained a shield for {currentShield} damage (Duration: {shieldTimer}s, Reduction: {shieldReductionPercent * 100f}%)");
    }

    public virtual void Kill()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"{gameObject.name} is killed");
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

            for (int i = 0; i < _allRenderers.Count; i++)
            {
                _allRenderers[i].materials = _originalMaterials[i];
            }
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
}
