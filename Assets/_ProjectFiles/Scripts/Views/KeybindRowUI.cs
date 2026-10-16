using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
//Helper svcript for for a single row ([Action Text] + [Key Button]) instead of doing it manually 12 times(or more if we added new features)in optionsview
/// </summary>
public class KeybindRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text actionNameText;
    [SerializeField] private Button rebindButton;
    [SerializeField] private TMP_Text keyNameText;

    private string actionId;

    // Event fired when the player clicks this row's button to change the key
    public event Action<string> OnRebindRequested;

    private void Awake()
    {
        rebindButton.onClick.AddListener(() => OnRebindRequested?.Invoke(actionId));
    }

    // Called by OptionsView when spawning or refreshing the list of 12 GDD controls
    public void Setup(string id, string displayName, KeyCode currentKey)
    {
        actionId = id;
        if (actionNameText != null)
            actionNameText.text = displayName;

        UpdateKeyText(currentKey.ToString());
    }

    // Updates the text on the button (e.g., changing "Q" to "..." while waiting, or to the new key)
    public void UpdateKeyText(string text)
    {
        if (keyNameText != null)
            keyNameText.text = text;
    }
}