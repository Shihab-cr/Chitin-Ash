using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Holds references to the Pause title and buttons, and broadcasts click events.
/// </summary>
public class PauseMenuView : MonoBehaviour
{
    [Header("Pause Menu UI Elements")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button exitButton;

    // Events for PauseMenuController to listen to
    public event Action OnResumeClicked;
    public event Action OnOptionsClicked;
    public event Action OnMainMenuClicked;
    public event Action OnExitClicked;

    private void Awake()
    {
        resumeButton.onClick.AddListener(() => OnResumeClicked?.Invoke());
        optionsButton.onClick.AddListener(() => OnOptionsClicked?.Invoke());
        mainMenuButton.onClick.AddListener(() => OnMainMenuClicked?.Invoke());
        exitButton.onClick.AddListener(() => OnExitClicked?.Invoke());
    }

    public void SetTitle(string title)
    {
        if (titleText != null)
            titleText.text = title;
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}