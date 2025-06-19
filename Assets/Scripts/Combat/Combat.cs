using UnityEngine;

/// <summary>
/// Handles combat-related functionality.
/// </summary>
public class Combat : MonoBehaviour
{
    /// <summary>
    /// Applies damage and, optionally, a timed effect to a target GameObject 
    /// if it has a <see cref="Health"/> component.
    /// </summary>
    /// <param name="target">The GameObject to receive damage.</param>
    /// <param name="damage">The amount of damage to apply.</param>
    /// <param name="inflictEffect">The optional effect to apply to the target. Pass <c>null</c> to apply no effect.</param>
    /// <param name="effectTime">The duration in seconds for which the effect should last, if one is applied.</param>
    public void ManageHit(GameObject target, float damage, Effect? inflictEffect = null, float effectTime = 0f)
    {
        if (target.TryGetComponent(out Health targetHealth))
        {
            targetHealth.InflictDamage(damage);

            if (inflictEffect.HasValue && target.TryGetComponent(out EffectHandler handler))
            {
                handler.ApplyEffect(inflictEffect.Value, effectTime);
                Debug.Log($"{target.name} is affected by {inflictEffect.Value}");
            }
        }
    }
}
