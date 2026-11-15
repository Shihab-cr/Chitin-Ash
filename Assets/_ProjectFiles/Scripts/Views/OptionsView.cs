using System;
using UnityEngine;
using UnityEngine.UI;

public class OptionsView : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private Button backButton;

    // Event that the Controller listens to
    public event Action OnBackClicked;

    private void Awake()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}