using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MVC View for the Quests tab: a scroll list of QuestEntryViews.
/// Entries: assign an entry prefab (spawned automatically) OR place QuestEntryViews by hand inside the container.
/// </summary>
public class QuestsPanelView : MonoBehaviour
{
    [SerializeField] private Transform entryContainer;
    [Tooltip("Optional. Empty = use the QuestEntryViews already placed in the container.")]
    [SerializeField] private QuestEntryView entryPrefab;
    [Tooltip("Optional: shown when there are no quests ('No goals yet')")]
    [SerializeField] private GameObject emptyState;

    private readonly List<QuestEntryView> entries = new List<QuestEntryView>();

    // Makes sure there are exactly "count" visible entries and returns them in order
    public IReadOnlyList<QuestEntryView> GetEntries(int count)
    {
        if (entries.Count == 0 && entryContainer != null)
        {
            entries.AddRange(entryContainer.GetComponentsInChildren<QuestEntryView>(true));
        }

        while (entries.Count < count && entryPrefab != null && entryContainer != null)
        {
            entries.Add(Instantiate(entryPrefab, entryContainer));
        }

        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].gameObject.SetActive(i < count);
        }

        if (emptyState != null) emptyState.SetActive(count == 0);

        if (entries.Count < count)
        {
            Debug.LogWarning($"[QuestsPanelView] {count} quests but only {entries.Count} entries. Assign an entry prefab.", this);
        }

        return entries;
    }
}
