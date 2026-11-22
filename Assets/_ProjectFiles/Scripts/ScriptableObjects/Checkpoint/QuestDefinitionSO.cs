using System;
using System.Collections.Generic;
using UnityEngine;

public enum QuestObjectiveType
{
    DefeatEnemy,      // targetId = enemy id (BlackAnt, PharaohAnt...) or empty for any
    ConsumeEnemy,     // targetId = enemy id or empty for any (Press E to consume)
    DefeatElite,      // targetId = elite id (BullAnt, DesertLocust, Mantis...)
    CollectRedAphids, // targetId ignored
    ReachPlayerLevel, // requiredAmount = level to reach
    UnlockSkill,      // targetId = skillId or empty for any skill
    RestAtCheckpoint  // targetId = checkpointId or empty for any
}

[Serializable]
public class QuestObjective
{
    [Tooltip("Text shown in the Quests tab, e.g. 'Defeat Black Ants'")]
    public string description = "New objective";
    public QuestObjectiveType type;
    [Tooltip("Enemy / elite / skill / checkpoint id. Leave empty to accept any.")]
    public string targetId = "";
    [Min(1)] public int requiredAmount = 1;
}

/// <summary>
/// Designer data for one goal of the beetle.
/// Create it from: Assets > Create > Chitin & Ash > Progression > Quest
/// </summary>
[CreateAssetMenu(fileName = "Quest_", menuName = "Chitin & Ash/Progression/Quest")]
public class QuestDefinitionSO : ScriptableObject
{
    [Tooltip("Unique id used in the save file. Do NOT change it after release.")]
    public string questId;
    public string title = "New Quest";
    [TextArea(2, 5)] public string description;

    [Tooltip("Optional: this quest only appears after this quest is completed")]
    public QuestDefinitionSO prerequisite;

    public List<QuestObjective> objectives = new List<QuestObjective>();

    [Header("Rewards (optional)")]
    [Min(0)] public int rewardScraps = 0;
    [Min(0)] public int rewardRedAphids = 0;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(questId))
        {
            questId = name;
        }
    }
}
