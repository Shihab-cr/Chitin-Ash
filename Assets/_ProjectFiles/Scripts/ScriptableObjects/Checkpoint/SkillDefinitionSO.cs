using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One upgrade step of a skill. Tier 1 = unlocking the skill, Tier 2+ = upgrades.
/// Bonuses of all unlocked tiers are added together.
/// </summary>
[Serializable]
public class SkillTier
{
    public string tierName = "Tier 1";
    [TextArea(2, 4)] public string description;

    [Header("Requirements")]
    [Tooltip("Red Aphids spent to unlock this tier")]
    [Min(0)] public int redAphidCost = 1;
    [Tooltip("Player level needed (Stats tab level)")]
    [Min(1)] public int requiredPlayerLevel = 1;
    [Tooltip("Elite that must be defeated first (example: BullAnt, DesertLocust). Leave empty for none.")]
    public string requiredEliteId = "";
    [Tooltip("Enemy type that must be consumed (example: BlackAnt). Leave empty with a count > 0 to mean ANY enemy.")]
    public string requiredConsumedEnemyId = "";
    [Min(0)] public int requiredConsumedCount = 0;

    [Header("Bonuses given by this tier")]
    public List<StatBonus> bonuses = new List<StatBonus>();
}

/// <summary>
/// Designer data for one passive skill (e.g. Stronger Block, Leap/Dodge, Stamina Regen).
/// Create it from: Assets > Create > Chitin & Ash > Progression > Skill
/// </summary>
[CreateAssetMenu(fileName = "Skill_", menuName = "Chitin & Ash/Progression/Skill")]
public class SkillDefinitionSO : ScriptableObject
{
    [Tooltip("Unique id used in the save file. Do NOT change it after release.")]
    public string skillId;
    public string displayName = "New Skill";
    [Tooltip("PLACEHOLDER: designer drops the skill icon here")]
    public Sprite icon;
    [TextArea(2, 5)] public string description;

    [Tooltip("Element 0 = unlock, element 1 = first upgrade, ...")]
    public List<SkillTier> tiers = new List<SkillTier>();

    public int MaxTier => tiers.Count;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(skillId))
        {
            skillId = name;
        }
    }
}
