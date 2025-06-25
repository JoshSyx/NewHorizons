using UnityEngine;

[System.Serializable]
public class DamageProfile
{
    public float rawAmount = 10f;
    public float multiplier = 1f;

    public DamageData ToDamageData(GameObject source)
    {
        return new DamageData(rawAmount, DamageType.Melee, source, multiplier);
    }
}
