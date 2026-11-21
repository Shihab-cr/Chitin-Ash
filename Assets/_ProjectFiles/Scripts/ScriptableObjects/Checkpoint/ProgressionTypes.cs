using System;

/// <summary>
/// Shared small types used by the Checkpoint / Progression systems (Stats, Skills, Quests).
/// Pure data, no Unity logic.
/// </summary>

// Every stat a level-up or a skill can change.
// Health / Stamina / Damage are shown in the Stats tab, the rest are used by skills (Stronger Block, Leap, ...).
public enum StatType
{
    MaxHealth,
    MaxStamina,
    Damage,
    StaminaRegen,
    BlockDamageReduction, // percent (0.1 = 10% less damage while blocking)
    BlockStaminaCost,     // negative value = block costs less stamina
    DodgeDistance
}

// One bonus given by a skill tier, e.g. { MaxHealth, +20 }
[Serializable]
public struct StatBonus
{
    public StatType stat;
    public float amount;
}

// The final numbers of the player (level stats + skill bonuses)
[Serializable]
public struct PlayerStats
{
    public float maxHealth;
    public float maxStamina;
    public float damage;
    public float staminaRegen;
    public float blockDamageReduction;
    public float blockStaminaCost;
    public float dodgeDistance;

    public void Add(StatBonus bonus)
    {
        switch (bonus.stat)
        {
            case StatType.MaxHealth: maxHealth += bonus.amount; break;
            case StatType.MaxStamina: maxStamina += bonus.amount; break;
            case StatType.Damage: damage += bonus.amount; break;
            case StatType.StaminaRegen: staminaRegen += bonus.amount; break;
            case StatType.BlockDamageReduction: blockDamageReduction += bonus.amount; break;
            case StatType.BlockStaminaCost: blockStaminaCost += bonus.amount; break;
            case StatType.DodgeDistance: dodgeDistance += bonus.amount; break;
        }
    }

    // Readable name used by the UI ("+20 Max Health")
    public static string GetDisplayName(StatType stat)
    {
        switch (stat)
        {
            case StatType.MaxHealth: return "Max Health";
            case StatType.MaxStamina: return "Max Stamina";
            case StatType.Damage: return "Damage";
            case StatType.StaminaRegen: return "Stamina Regen";
            case StatType.BlockDamageReduction: return "Block Damage Reduction";
            case StatType.BlockStaminaCost: return "Block Stamina Cost";
            case StatType.DodgeDistance: return "Dodge Distance";
            default: return stat.ToString();
        }
    }
}

// Tabs of the checkpoint menu
public enum CheckpointTab
{
    Stats,
    Skills,
    Quests
}
