using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary> 
/// MVC View: Attached to the Main Menu UI Panel.
/// Holds references to Unity UI components, broadcasts button click events, and toggles panels when instructed by the Controller. 
/// Contains zero game/save logic.
/// </summary>
public class MainMenuView : MonoBehaviour
{
    [Header("Main Menu UI Elements")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button exitButton;

    [Header("Overwrite Save Warning Sub-Panel")]
    [SerializeField] private GameObject overwriteWarningPanel;
    [SerializeField] private Button confirmOverwriteButton;
    [SerializeField] private Button cancelOverwriteButton;

    [Header("Credits Sub-Panel")]
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private Button closeCreditsButton;

    // Events that the Controller will listen to
    public event Action OnNewGameClicked;
    public event Action OnLoadGameClicked;
    public event Action OnOptionsClicked;
    public event Action OnCreditsClicked;
    public event Action OnExitClicked;
    public event Action OnConfirmOverwriteClicked;
    public event Action OnCancelOverwriteClicked;
    public event Action OnCloseCreditsClicked;

    private void Awake()
    {
        // Whenever a Unity UI Button is clicked, fire the matching C# event
        newGameButton.onClick.AddListener(() => OnNewGameClicked?.Invoke());
        loadGameButton.onClick.AddListener(() => OnLoadGameClicked?.Invoke());
        optionsButton.onClick.AddListener(() => OnOptionsClicked?.Invoke());
        creditsButton.onClick.AddListener(() => OnCreditsClicked?.Invoke());
        exitButton.onClick.AddListener(() => OnExitClicked?.Invoke());

        confirmOverwriteButton.onClick.AddListener(() => OnConfirmOverwriteClicked?.Invoke());
        cancelOverwriteButton.onClick.AddListener(() => OnCancelOverwriteClicked?.Invoke());
        closeCreditsButton.onClick.AddListener(() => OnCloseCreditsClicked?.Invoke());
    }

    //visual update functions (called by MainMenuController)

    public void SetTitle(string title)
    {
        if (titleText != null)
            titleText.text = title;
    }

    public void SetLoadButtonInteractable(bool isInteractable)
    {
        loadGameButton.interactable = isInteractable;
    }

    public void ShowOverwriteWarning(bool show)
    {
        overwriteWarningPanel.SetActive(show);
    }

    public void ShowCredits(bool show)
    {
        creditsPanel.SetActive(show);
    }

    public void ShowMainMenu(bool show)
    {
        gameObject.SetActive(show);
    }
}