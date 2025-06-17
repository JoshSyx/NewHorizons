using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EffectData
{
    public Effect effect;
    public float duration;

    public EffectData(Effect effect, float duration)
    {
        this.effect = effect;
        this.duration = duration;
    }
}

public class EffectHandler : MonoBehaviour
{
    private readonly List<EffectData> activeEffects = new();

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

    public void ApplyEffect(Effect effect, float duration)
    {
        activeEffects.Add(new EffectData(effect, duration));
        Debug.Log($"{gameObject.name} is now affected by {effect} for {duration} seconds");
    }

    public bool HasEffect(Effect effect)
    {
        return activeEffects.Exists(e => e.effect.Equals(effect));
    }
}
