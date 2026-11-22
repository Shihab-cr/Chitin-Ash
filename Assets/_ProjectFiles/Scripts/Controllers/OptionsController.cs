using System;
using UnityEngine;

public class OptionsController : MonoBehaviour
{
    [SerializeField] private OptionsView optionsView;

    // Stores the action to reopen either MainMenuPanel or PauseMenuPanel
    private Action _onCloseCallback;

    private void Awake()
    {
        if (optionsView != null)
        {
            optionsView.OnBackClicked += HandleBackClicked;
        }
    }

    private void OnDestroy()
    {
        if (optionsView != null)
        {
            optionsView.OnBackClicked -= HandleBackClicked;
        }
    }

    // Called by MainMenuController or PauseMenuController when opening Options
    public void OpenOptions(Action onCloseCallback)
    {
        _onCloseCallback = onCloseCallback;
        optionsView.Show();
    }

    private void HandleBackClicked()
    {
        optionsView.Hide();
        _onCloseCallback?.Invoke();
    }
}