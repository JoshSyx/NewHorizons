#region Enums
/// <summary>
/// Specifies the type of an effect: Buff or Debuff.
/// </summary>
public enum EffectType
{
    /// <summary>
    /// A beneficial effect.
    /// </summary>
    Buff,

    /// <summary>
    /// A harmful effect.
    /// </summary>
    Debuff
}

/// <summary>
/// Enumeration of possible buff effects.
/// </summary>
public enum Buff
{
    Strength,
    Haste,
    Fortify,
    Regeneration,
    Focus,
    Shield
}

/// <summary>
/// Enumeration of possible debuff effects.
/// </summary>
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
#endregion

#region struct Effect
/// <summary>
/// Represents a single effect, which can be either a Buff or a Debuff.
/// </summary>
[System.Serializable]
public struct Effect
{
    /// <summary>
    /// The type of this effect (Buff or Debuff).
    /// </summary>
    public EffectType effectType;

    /// <summary>
    /// The specific buff, if this is a Buff effect.
    /// </summary>
    public Buff? buff;

    /// <summary>
    /// The specific debuff, if this is a Debuff effect.
    /// </summary>
    public Debuff? debuff;

    /// <summary>
    /// Initializes a new Buff effect.
    /// </summary>
    /// <param name="buff">The buff to assign.</param>
    public Effect(Buff buff)
    {
        this.buff = buff;
        this.debuff = null;
        this.effectType = EffectType.Buff;
    }

    /// <summary>
    /// Initializes a new Debuff effect.
    /// </summary>
    /// <param name="debuff">The debuff to assign.</param>
    public Effect(Debuff debuff)
    {
        this.buff = null;
        this.debuff = debuff;
        this.effectType = EffectType.Debuff;
    }

    /// <summary>
    /// Returns a string that represents the current effect.
    /// </summary>
    /// <returns>A string representation of the effect.</returns>
    public override string ToString()
    {
        return effectType == EffectType.Buff ? buff.ToString() : debuff.ToString();
    }

    /// <summary>
    /// Determines whether the specified effect is equal to the current effect.
    /// </summary>
    /// <param name="other">The effect to compare with the current effect.</param>
    /// <returns>True if the effects are equal; otherwise, false.</returns>
    public bool Equals(Effect other)
    {
        return effectType == other.effectType && buff == other.buff && debuff == other.debuff;
    }
}
#endregion