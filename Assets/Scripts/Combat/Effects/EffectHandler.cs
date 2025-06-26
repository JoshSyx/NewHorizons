using System.Collections.Generic;
using UnityEngine;


#region EffectData
/// <summary>
/// Represents enemyData for an effect with its remaining duration.
/// </summary>
[System.Serializable]
public class EffectData
{
    /// <summary>
    /// The effect to apply.
    /// </summary>
    public Effect effect;

    /// <summary>
    /// The remaining duration of the effect in seconds.
    /// </summary>
    public float duration;

    /// <summary>
    /// Initializes a new Instance of the <see cref="EffectData"/> class.
    /// </summary>
    /// <param name="effect">The effect to apply.</param>
    /// <param name="duration">The duration of the effect in seconds.</param>
    public EffectData(Effect effect, float duration)
    {
        this.effect = effect;
        this.duration = duration;
    }
}
#endregion

#region EffectHandler
/// <summary>
/// Handles the application and expiration of effects on a GameObject.
/// </summary>
public class EffectHandler : MonoBehaviour
{
    /// <summary>
    /// The list of currently active effects.
    /// </summary>
    private readonly List<EffectData> activeEffects = new();

    /// <summary>
    /// Unity's update method, called once per frame.
    /// Decrements effect durations and removes expired effects.
    /// </summary>
    private void Update()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].duration -= Time.deltaTime;

            if (activeEffects[i].duration <= 0)
            {
                Debug.Log($"{gameObject.name} is no longer affected by {activeEffects[i].effect}");
                activeEffects.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Applies a new effect to the GameObject.
    /// </summary>
    /// <param name="effect">The effect to apply.</param>
    /// <param name="duration">The duration of the effect in seconds.</param>
    public void ApplyEffect(Effect effect, float duration)
    {
        activeEffects.Add(new EffectData(effect, duration));
        Debug.Log($"{gameObject.name} is now affected by {effect} for {duration} seconds");
    }

    /// <summary>
    /// Checks whether a specific effect is currently active.
    /// </summary>
    /// <param name="effect">The effect to check.</param>
    /// <returns>True if the effect is active; otherwise, false.</returns>
    public bool HasEffect(Effect effect)
    {
        return activeEffects.Exists(e => e.effect.Equals(effect));
    }
}
#endregion