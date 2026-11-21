using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// MVC Controller for the Quests tab: shows the goals of the beetle and their progress.
/// Active quests first, completed quests after them. Initialized by CheckpointMenuController.
/// </summary>
public class QuestsPanelController : MonoBehaviour
{
    [SerializeField] private QuestsPanelView view;

    [Header("Display")]
    [SerializeField] private bool showCompletedQuests = true;
    [Tooltip("{0} = objective text, {1} = current, {2} = required")]
    [SerializeField] private string objectiveFormat = "- {0}  {1}/{2}";
    [SerializeField] private string doneColor = "#7CFC00";

    private ProgressionModel model;

    public void Initialize(ProgressionModel progressionModel)
    {
        model = progressionModel;
        model.OnProgressionChanged += RefreshIfVisible;
    }

    private void OnDestroy()
    {
        if (model != null) model.OnProgressionChanged -= RefreshIfVisible;
    }

    public void Refresh()
    {
        if (model == null || view == null) return;

        QuestModel system = model.Quests;
        List<QuestDefinitionSO> toShow = new List<QuestDefinitionSO>();

        foreach (QuestDefinitionSO quest in system.AllQuests)
        {
            if (system.IsActive(quest)) toShow.Add(quest);
        }
        if (showCompletedQuests)
        {
            foreach (QuestDefinitionSO quest in system.AllQuests)
            {
                if (quest != null && system.IsCompleted(quest)) toShow.Add(quest);
            }
        }

        IReadOnlyList<QuestEntryView> entries = view.GetEntries(toShow.Count);
        for (int i = 0; i < toShow.Count && i < entries.Count; i++)
        {
            QuestDefinitionSO quest = toShow[i];
            entries[i].Setup(quest.title, quest.description, BuildObjectives(system, quest),
                system.GetQuestProgress01(quest), system.IsCompleted(quest));
        }
    }

    private void RefreshIfVisible()
    {
        if (view != null && view.gameObject.activeInHierarchy) Refresh();
    }

    private string BuildObjectives(QuestModel system, QuestDefinitionSO quest)
    {
        bool questDone = system.IsCompleted(quest);
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < quest.objectives.Count; i++)
        {
            QuestObjective objective = quest.objectives[i];
            int current = questDone ? objective.requiredAmount : system.GetObjectiveProgress(quest, i);
            string line = string.Format(objectiveFormat, objective.description, current, objective.requiredAmount);

            if (current >= objective.requiredAmount)
            {
                line = $"<color={doneColor}>{line}</color>";
            }
            sb.AppendLine(line);
        }
        return sb.ToString().TrimEnd();
    }
}
