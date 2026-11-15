using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
///Simple script. Attached to the Game Over UI Panel, displays the Game Over title and broadcasts when the Restart button is clicked.
/// </summary>
public class GameOverView : MonoBehaviour
{
    [Header("Game Over UI Elements")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button restartButton;

    // Event for GameOverController to listen to
    public event Action OnRestartClicked;

    private void Awake()
    {
        restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
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