using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Designer data: how much each level costs in Scraps and how much Health / Stamina / Damage each level gives.
/// Create it from: Assets > Create > Chitin & Ash > Progression > Level Progression
/// </summary>
[CreateAssetMenu(fileName = "LevelProgression", menuName = "Chitin & Ash/Progression/Level Progression")]
public class LevelProgressionSO : ScriptableObject
{
    [Header("Level Cap")]
    [Min(1)] public int maxLevel = 20;

    [Header("Base Stats (at Level 1)")]
    public float baseHealth = 100f;
    public float baseStamina = 100f;
    public float baseDamage = 10f;

    [Header("Stats Gained Every Level")]
    public float healthPerLevel = 10f;
    public float staminaPerLevel = 5f;
    public float damagePerLevel = 2f;

    [Header("Scrap Cost (formula)")]
    [Tooltip("Cost to go from level 1 to level 2")]
    [Min(0)] public int baseCost = 100;
    [Tooltip("Every next level costs this much more (1.25 = +25% each level)")]
    [Min(1f)] public float costMultiplier = 1.25f;

    [Header("Scrap Cost (optional exact list)")]
    [Tooltip("Optional. Element 0 = cost of level 1 -> 2, element 1 = level 2 -> 3 ... Leave empty to use the formula above.")]
    public List<int> customCosts = new List<int>();

    public bool IsMaxLevel(int level)
    {
        return level >= maxLevel;
    }

    // Cost in scraps to go from currentLevel to currentLevel + 1
    public int GetLevelUpCost(int currentLevel)
    {
        int index = Mathf.Max(currentLevel - 1, 0);

        if (customCosts != null && index < customCosts.Count)
        {
            return customCosts[index];
        }

        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, index));
    }

    // Stats coming ONLY from the level (skills are added on top by the SkillModel)
    public PlayerStats GetStatsForLevel(int level)
    {
        int levelsGained = Mathf.Clamp(level, 1, maxLevel) - 1;

        PlayerStats stats = new PlayerStats();
        stats.maxHealth = baseHealth + healthPerLevel * levelsGained;
        stats.maxStamina = baseStamina + staminaPerLevel * levelsGained;
        stats.damage = baseDamage + damagePerLevel * levelsGained;
        return stats;
    }
}
