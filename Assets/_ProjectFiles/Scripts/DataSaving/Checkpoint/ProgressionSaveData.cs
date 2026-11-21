using System;
using System.Collections.Generic;

/// <summary>
/// Everything the Checkpoint systems need to remember (level, skills, quests, elites, consumed enemies).
/// Saved in its own JSON file next to the team's chitin_progress.json so we don't touch PlayerSaveData.
/// (JsonUtility can't save Dictionaries, so we use Lists like SaveDataStructures.cs does)
/// </summary>
[Serializable]
public class IdCountEntry
{
    public string id;
    public int count;
}

[Serializable]
public class SkillSaveEntry
{
    public string skillId;
    public int unlockedTiers; // 0 = locked, 1 = unlocked, 2+ = upgraded
}

[Serializable]
public class QuestSaveEntry
{
    public string questId;
    public bool completed;
    public List<int> objectiveProgress = new List<int>();
}

[Serializable]
public class ProgressionSaveData
{
    public int playerLevel = 1;
    public List<SkillSaveEntry> skills = new List<SkillSaveEntry>();
    public List<QuestSaveEntry> quests = new List<QuestSaveEntry>();
    public List<string> defeatedElites = new List<string>();
    public List<IdCountEntry> consumedEnemies = new List<IdCountEntry>();

    // Old / broken files can come back with null lists -> fix them
    public void EnsureValid()
    {
        if (playerLevel < 1) playerLevel = 1;
        if (skills == null) skills = new List<SkillSaveEntry>();
        if (quests == null) quests = new List<QuestSaveEntry>();
        if (defeatedElites == null) defeatedElites = new List<string>();
        if (consumedEnemies == null) consumedEnemies = new List<IdCountEntry>();
    }
}
