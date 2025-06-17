public enum EffectType
{
    Buff,
    Debuff
}

public enum Buff
{
    Strength,
    Haste,
    Fortify,
    Regeneration,
    Focus,
    MagicShield
}

public enum Debuff
{
    Weakness,
    Burn,
    Freeze,
    Poison,
    Bleed,
    Stun,
    Shock,
    Blind,
    Cripple,
    Curse,
    Corrosion,
    Slow,
    Silence,
    Fear,
    Confuse,
    Drain
}

[System.Serializable]
public struct Effect
{
    public EffectType effectType;
    public Buff? buff;
    public Debuff? debuff;

    public Effect(Buff buff)
    {
        this.buff = buff;
        this.debuff = null;
        this.effectType = EffectType.Buff;
    }

    public Effect(Debuff debuff)
    {
        this.buff = null;
        this.debuff = debuff;
        this.effectType = EffectType.Debuff;
    }

    public override string ToString()
    {
        return effectType == EffectType.Buff ? buff.ToString() : debuff.ToString();
    }

    public bool Equals(Effect other)
    {
        return effectType == other.effectType && buff == other.buff && debuff == other.debuff;
    }
}
