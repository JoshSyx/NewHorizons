using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    [Header("User Feedback")]
    [SerializeField] private Material idle;
    [SerializeField] private Material getHit;
    [SerializeField] private Material die;

    private List<Renderer> _allRenderers = new List<Renderer>();
    private List<Material[]> _originalMaterials = new List<Material[]>();
    private Coroutine flashCoroutine;

    public float CurrentHealth => currentHealth;
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

    public void InflictDamage(DamageData damageData)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - damageData.FinalAmount);

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashMaterial(getHit, 0.15f));

        if (currentHealth == 0f)
        {
            Kill();
        }
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
