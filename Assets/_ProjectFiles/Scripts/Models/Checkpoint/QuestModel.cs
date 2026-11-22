using System;
using System.Collections.Generic;

/// <summary>
/// MVC Model (pure C#): quest logic - tracks the progress of every objective and completes quests.
/// Gameplay never calls this directly -> it goes through ProgressionEvents -> ProgressionController -> ProgressionModel -> here.
/// </summary>
public class QuestModel
{
    private readonly List<QuestDefinitionSO> quests;
    private readonly ProgressionSaveData data;

    // Fired once when a quest becomes completed (ProgressionModel / ProgressionController give the rewards)
    public event Action<QuestDefinitionSO> OnQuestCompleted;

    public QuestModel(List<QuestDefinitionSO> quests, ProgressionSaveData data)
    {
        this.quests = quests ?? new List<QuestDefinitionSO>();
        this.data = data;
    }

    public IReadOnlyList<QuestDefinitionSO> AllQuests => quests;

    // A quest is visible when it has no prerequisite or the prerequisite is completed
    public bool IsAvailable(QuestDefinitionSO quest)
    {
        return quest != null && (quest.prerequisite == null || IsCompleted(quest.prerequisite));
    }

    public bool IsCompleted(QuestDefinitionSO quest)
    {
        QuestSaveEntry entry = FindEntry(quest);
        return entry != null && entry.completed;
    }

    public bool IsActive(QuestDefinitionSO quest)
    {
        return IsAvailable(quest) && !IsCompleted(quest);
    }

    public int GetObjectiveProgress(QuestDefinitionSO quest, int objectiveIndex)
    {
        QuestSaveEntry entry = FindEntry(quest);
        if (entry == null || objectiveIndex >= entry.objectiveProgress.Count) return 0;
        return Math.Min(entry.objectiveProgress[objectiveIndex], quest.objectives[objectiveIndex].requiredAmount);
    }

    // 0..1 progress of the whole quest (for a progress bar)
    public float GetQuestProgress01(QuestDefinitionSO quest)
    {
        if (IsCompleted(quest)) return 1f;
        if (quest.objectives.Count == 0) return 0f;

        float sum = 0f;
        for (int i = 0; i < quest.objectives.Count; i++)
        {
            sum += (float)GetObjectiveProgress(quest, i) / quest.objectives[i].requiredAmount;
        }
        return sum / quest.objectives.Count;
    }

    // Adds "amount" to every matching objective of every active quest (kills, consumes, pickups...)
    public void AddProgress(QuestObjectiveType type, string targetId, int amount)
    {
        UpdateObjectives(type, targetId, amount, false);
    }

    // Sets progress to at least "value" (used for things like "Reach Level 5")
    public void SetProgressAtLeast(QuestObjectiveType type, string targetId, int value)
    {
        UpdateObjectives(type, targetId, value, true);
    }

    private void UpdateObjectives(QuestObjectiveType type, string targetId, int amount, bool isAbsolute)
    {
        // Take the active quests BEFORE updating, so a quest unlocked by this same report
        // doesn't get progress from it too
        List<QuestDefinitionSO> activeQuests = new List<QuestDefinitionSO>();
        foreach (QuestDefinitionSO quest in quests)
        {
            if (IsActive(quest)) activeQuests.Add(quest);
        }

        List<QuestDefinitionSO> justCompleted = new List<QuestDefinitionSO>();

        foreach (QuestDefinitionSO quest in activeQuests)
        {
            QuestSaveEntry entry = GetOrCreateEntry(quest);
            bool changed = false;

            for (int i = 0; i < quest.objectives.Count; i++)
            {
                QuestObjective objective = quest.objectives[i];
                if (objective.type != type || !TargetMatches(objective.targetId, targetId)) continue;

                int oldValue = entry.objectiveProgress[i];
                int newValue = isAbsolute ? Math.Max(oldValue, amount) : oldValue + amount;
                entry.objectiveProgress[i] = Math.Min(newValue, objective.requiredAmount);
                changed |= entry.objectiveProgress[i] != oldValue;
            }

            if (changed && AreAllObjectivesDone(quest, entry))
            {
                entry.completed = true;
                justCompleted.Add(quest);
            }
        }

        foreach (QuestDefinitionSO quest in justCompleted)
        {
            OnQuestCompleted?.Invoke(quest);
        }
    }

    private bool AreAllObjectivesDone(QuestDefinitionSO quest, QuestSaveEntry entry)
    {
        for (int i = 0; i < quest.objectives.Count; i++)
        {
            if (entry.objectiveProgress[i] < quest.objectives[i].requiredAmount) return false;
        }
        return true;
    }

    // Objective with empty targetId accepts anything
    private static bool TargetMatches(string objectiveTarget, string reportedTarget)
    {
        return string.IsNullOrEmpty(objectiveTarget)
            || string.Equals(objectiveTarget, reportedTarget, StringComparison.OrdinalIgnoreCase);
    }

    private QuestSaveEntry FindEntry(QuestDefinitionSO quest)
    {
        if (quest == null) return null;

        foreach (QuestSaveEntry entry in data.quests)
        {
            if (entry.questId == quest.questId) return entry;
        }
        return null;
    }

    private QuestSaveEntry GetOrCreateEntry(QuestDefinitionSO quest)
    {
        QuestSaveEntry entry = FindEntry(quest);
        if (entry == null)
        {
            entry = new QuestSaveEntry { questId = quest.questId };
            data.quests.Add(entry);
        }

        if (entry.objectiveProgress == null) entry.objectiveProgress = new List<int>();

        // Designer may add objectives after a save was made -> keep the list the same size
        while (entry.objectiveProgress.Count < quest.objectives.Count)
        {
            entry.objectiveProgress.Add(0);
        }
        return entry;
    }
}
