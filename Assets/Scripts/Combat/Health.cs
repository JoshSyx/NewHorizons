using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float baseAmount = 100f;

    /// <summary>
    /// Applies raw damage to this entity's health.
    /// </summary>
    /// <param name="damage">Amount of damage to subtract.</param>
    public void InflictDamage(float damage)
    {
        baseAmount = Mathf.Max(0f, baseAmount - damage);
        if (baseAmount == 0f) Kill();
    }

    /// <summary>
    /// Called when health reaches 0. Override to add game-specific logic.
    /// </summary>
    public virtual void Kill()
    {
        Debug.Log($"{gameObject.name} is killed");
        Destroy(gameObject);
    }
}
