using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// MVC View: put on the root panel of the checkpoint menu.
/// Only holds UI references (PLACEHOLDERS for the designer), broadcasts button clicks and shows/hides panels.
/// Contains zero game/save logic.
/// </summary>
public class CheckpointMenuView : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private TMP_Text checkpointNameText;

    [Header("Save Buttons")]
    [SerializeField] private Button saveGameButton;
    [SerializeField] private Button saveAndQuitButton;
    [SerializeField] private Button saveAndMainMenuButton;

    [Header("Tab Buttons")]
    [SerializeField] private Button statsTabButton;
    [SerializeField] private Button skillsTabButton;
    [SerializeField] private Button questsTabButton;

    [Header("Tab Panels (only one is active at a time)")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private GameObject skillsPanel;
    [SerializeField] private GameObject questsPanel;

    [Header("Optional: highlight objects shown on the selected tab button")]
    [SerializeField] private GameObject statsTabHighlight;
    [SerializeField] private GameObject skillsTabHighlight;
    [SerializeField] private GameObject questsTabHighlight;

    [Header("Leave / Resume")]
    [SerializeField] private Button leaveButton;

    [Header("Feedback message (e.g. 'Game Saved')")]
    [SerializeField] private GameObject messageRoot;
    [SerializeField] private TMP_Text messageText;

    public event Action OnSaveGameClicked;
    public event Action OnSaveAndQuitClicked;
    public event Action OnSaveAndMainMenuClicked;
    public event Action<CheckpointTab> OnTabClicked;
    public event Action OnLeaveClicked;

    private void Awake()
    {
        AddListener(saveGameButton, () => OnSaveGameClicked?.Invoke());
        AddListener(saveAndQuitButton, () => OnSaveAndQuitClicked?.Invoke());
        AddListener(saveAndMainMenuButton, () => OnSaveAndMainMenuClicked?.Invoke());
        AddListener(statsTabButton, () => OnTabClicked?.Invoke(CheckpointTab.Stats));
        AddListener(skillsTabButton, () => OnTabClicked?.Invoke(CheckpointTab.Skills));
        AddListener(questsTabButton, () => OnTabClicked?.Invoke(CheckpointTab.Quests));
        AddListener(leaveButton, () => OnLeaveClicked?.Invoke());
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }

    public void SetCheckpointName(string checkpointName)
    {
        if (checkpointNameText != null) checkpointNameText.text = checkpointName;
    }

    public void ShowTab(CheckpointTab tab)
    {
        SetActive(statsPanel, tab == CheckpointTab.Stats);
        SetActive(skillsPanel, tab == CheckpointTab.Skills);
        SetActive(questsPanel, tab == CheckpointTab.Quests);

        SetActive(statsTabHighlight, tab == CheckpointTab.Stats);
        SetActive(skillsTabHighlight, tab == CheckpointTab.Skills);
        SetActive(questsTabHighlight, tab == CheckpointTab.Quests);
    }

    public void ShowMessage(string message)
    {
        if (messageText != null) messageText.text = message;
        SetActive(messageRoot != null ? messageRoot : (messageText != null ? messageText.gameObject : null), true);
    }

    public void HideMessage()
    {
        SetActive(messageRoot != null ? messageRoot : (messageText != null ? messageText.gameObject : null), false);
    }

    private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null) button.onClick.AddListener(action);
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null) target.SetActive(active);
    }
}
