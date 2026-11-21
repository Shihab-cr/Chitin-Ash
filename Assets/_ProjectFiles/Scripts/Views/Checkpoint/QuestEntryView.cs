using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One quest row in the Quests tab. Make a prefab with this script. All references are PLACEHOLDERS.
/// </summary>
public class QuestEntryView : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [Tooltip("Multi-line list: 'Defeat Black Ants 3/5'")]
    [SerializeField] private TMP_Text objectivesText;
    [Tooltip("Optional: overall progress bar (0..1). Set the Slider to non-interactable.")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private GameObject completedMarker;

    public void Setup(string title, string description, string objectives, float progress01, bool completed)
    {
        if (titleText != null) titleText.text = title;
        if (descriptionText != null) descriptionText.text = description;
        if (objectivesText != null) objectivesText.text = objectives;

        if (progressBar != null)
        {
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
            progressBar.value = progress01;
        }

        if (completedMarker != null) completedMarker.SetActive(completed);
    }
}
