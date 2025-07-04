using UnityEngine;

public struct DamageData
{
    public float RawAmount;
    public float Multiplier;
    public float FinalAmount => RawAmount * Multiplier;

    public DamageType Type;
    public GameObject Source;

    public DamageData(float rawAmount, DamageType type, GameObject source = null, float multiplier = 1f)
    {
        RawAmount = rawAmount;
        Type = type;
        Source = source;
        Multiplier = multiplier * GameManager.Instance.damageMultiplier;
    }

    public DamageData WithMultiplier(float newMultiplier)
    {
        return new DamageData(RawAmount, Type, Source, newMultiplier);
    }
}
