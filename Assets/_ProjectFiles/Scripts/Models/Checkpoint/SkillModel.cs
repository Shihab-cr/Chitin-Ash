using System;
using System.Collections.Generic;

// One line of the requirements list shown in the Skills tab
public struct RequirementStatus
{
    public string label;
    public bool met;

    public RequirementStatus(string label, bool met)
    {
        this.label = label;
        this.met = met;
    }
}

/// <summary>
/// MVC Model (pure C#): skill logic - which tier each skill is at, requirement checks, and total stat bonuses.
/// Also remembers defeated elites and consumed enemies (they are skill requirements in the GDD).
/// Spending red aphids is done by ProgressionModel.
/// </summary>
public class SkillModel
{
    private readonly List<SkillDefinitionSO> skills;
    private readonly ProgressionSaveData data;

    public SkillModel(List<SkillDefinitionSO> skills, ProgressionSaveData data)
    {
        this.skills = skills ?? new List<SkillDefinitionSO>();
        this.data = data;
    }

    public IReadOnlyList<SkillDefinitionSO> AllSkills => skills;

    // ---------------- Tiers ----------------

    public int GetUnlockedTiers(SkillDefinitionSO skill)
    {
        SkillSaveEntry entry = FindEntry(skill);
        return entry == null ? 0 : entry.unlockedTiers;
    }

    public bool IsUnlocked(SkillDefinitionSO skill)
    {
        return GetUnlockedTiers(skill) > 0;
    }

    public bool IsFullyUpgraded(SkillDefinitionSO skill)
    {
        return GetUnlockedTiers(skill) >= skill.MaxTier;
    }

    // The tier the player can buy next (null if fully upgraded)
    public SkillTier GetNextTier(SkillDefinitionSO skill)
    {
        int unlocked = GetUnlockedTiers(skill);
        return unlocked < skill.MaxTier ? skill.tiers[unlocked] : null;
    }

    // ---------------- Requirements ----------------

    public List<RequirementStatus> GetRequirements(SkillDefinitionSO skill, int playerLevel, int redAphids)
    {
        List<RequirementStatus> result = new List<RequirementStatus>();
        SkillTier tier = GetNextTier(skill);
        if (tier == null) return result;

        if (tier.redAphidCost > 0)
        {
            result.Add(new RequirementStatus($"Red Aphids: {redAphids} / {tier.redAphidCost}", redAphids >= tier.redAphidCost));
        }

        if (tier.requiredPlayerLevel > 1)
        {
            result.Add(new RequirementStatus($"Player Level {tier.requiredPlayerLevel} (now {playerLevel})", playerLevel >= tier.requiredPlayerLevel));
        }

        if (!string.IsNullOrEmpty(tier.requiredEliteId))
        {
            result.Add(new RequirementStatus($"Defeat Elite: {tier.requiredEliteId}", IsEliteDefeated(tier.requiredEliteId)));
        }

        if (tier.requiredConsumedCount > 0)
        {
            string enemyName = string.IsNullOrEmpty(tier.requiredConsumedEnemyId) ? "any enemy" : tier.requiredConsumedEnemyId;
            int consumed = GetConsumedCount(tier.requiredConsumedEnemyId);
            result.Add(new RequirementStatus($"Consume {enemyName}: {Math.Min(consumed, tier.requiredConsumedCount)} / {tier.requiredConsumedCount}", consumed >= tier.requiredConsumedCount));
        }

        return result;
    }

    public bool CanUnlockNextTier(SkillDefinitionSO skill, int playerLevel, int redAphids)
    {
        if (GetNextTier(skill) == null) return false;

        foreach (RequirementStatus requirement in GetRequirements(skill, playerLevel, redAphids))
        {
            if (!requirement.met) return false;
        }
        return true;
    }

    // Moves the skill one tier up (requirements must be checked before). Returns false if maxed.
    public bool UnlockNextTier(SkillDefinitionSO skill)
    {
        if (GetNextTier(skill) == null) return false;

        SkillSaveEntry entry = FindEntry(skill);
        if (entry == null)
        {
            entry = new SkillSaveEntry { skillId = skill.skillId, unlockedTiers = 0 };
            data.skills.Add(entry);
        }

        entry.unlockedTiers++;
        return true;
    }

    // ---------------- Bonuses ----------------

    // Adds the bonuses of every unlocked tier of every skill
    public void AddSkillBonuses(ref PlayerStats stats)
    {
        foreach (SkillDefinitionSO skill in skills)
        {
            if (skill == null) continue;

            int unlocked = Math.Min(GetUnlockedTiers(skill), skill.MaxTier);
            for (int i = 0; i < unlocked; i++)
            {
                foreach (StatBonus bonus in skill.tiers[i].bonuses)
                {
                    stats.Add(bonus);
                }
            }
        }
    }

    // ---------------- Elites & consumed enemies ----------------

    public void RecordEliteDefeated(string eliteId)
    {
        if (string.IsNullOrEmpty(eliteId) || IsEliteDefeated(eliteId)) return;
        data.defeatedElites.Add(eliteId);
    }

    public bool IsEliteDefeated(string eliteId)
    {
        foreach (string id in data.defeatedElites)
        {
            if (SameId(id, eliteId)) return true;
        }
        return false;
    }

    public void RecordEnemyConsumed(string enemyId)
    {
        if (string.IsNullOrEmpty(enemyId)) enemyId = "Unknown";

        foreach (IdCountEntry entry in data.consumedEnemies)
        {
            if (SameId(entry.id, enemyId))
            {
                entry.count++;
                return;
            }
        }
        data.consumedEnemies.Add(new IdCountEntry { id = enemyId, count = 1 });
    }

    // Empty id = total of all consumed enemies
    public int GetConsumedCount(string enemyId)
    {
        int total = 0;
        foreach (IdCountEntry entry in data.consumedEnemies)
        {
            if (string.IsNullOrEmpty(enemyId) || SameId(entry.id, enemyId))
            {
                total += entry.count;
            }
        }
        return total;
    }

    // ---------------- Helpers ----------------

    private SkillSaveEntry FindEntry(SkillDefinitionSO skill)
    {
        if (skill == null) return null;

        foreach (SkillSaveEntry entry in data.skills)
        {
            if (entry.skillId == skill.skillId) return entry;
        }
        return null;
    }

    private static bool SameId(string a, string b)
    {
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
