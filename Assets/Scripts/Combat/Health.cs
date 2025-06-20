using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float baseAmount = 100f;

    [Header("User feedback")]
    [SerializeField] private Material idle;
    [SerializeField] private Material getHit;
    [SerializeField] private Material die;

    private List<Renderer> _allRenderers = new List<Renderer>();
    private List<Material[]> _originalMaterials = new List<Material[]>();

    protected virtual void Awake()
    {
        // Find all renderers in this object and children
        _allRenderers.AddRange(GetComponentsInChildren<Renderer>());

        foreach (var renderer in _allRenderers)
        {
            // Save original materials so we can revert after getHit flash
            _originalMaterials.Add(renderer.materials);

            // Optionally assign idle material to all sub-meshes
            if (idle)
            {
                var idleMats = new Material[renderer.materials.Length];
                for (int i = 0; i < idleMats.Length; i++)
                    idleMats[i] = idle;

                renderer.materials = idleMats;
            }
        }
    }

    /// <summary>
    /// Applies raw damage to this entity's health.
    /// </summary>
    /// <param name="damage">Amount of damage to subtract.</param>
    public void InflictDamage(float damage)
    {
        baseAmount = Mathf.Max(0f, baseAmount - damage);
        StartCoroutine(FlashMaterial(getHit, 0.15f));

        if (baseAmount == 0f)
        {
            Kill();
        }
    }

    public virtual void Kill()
    {
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

            // Restore original materials
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
