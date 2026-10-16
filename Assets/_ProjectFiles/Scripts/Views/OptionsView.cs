using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// MVC View: Attached to the Options/Settings Panel, it manages sliders , keybinds etc...
/// </summary>
public class OptionsView : MonoBehaviour
{
    [Header("Audio, Mouse & Visual Sliders")]
    [SerializeField] private Slider bgVolumeSlider;
    [SerializeField] private Slider actionVolumeSlider;
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Toggle postProcessingToggle;

    [Header("Controls Rebinding UI")]
    [SerializeField] private Transform controlsListContainer;
    [SerializeField] private KeybindRowUI keybindRowPrefab;
    [SerializeField] private TMP_Text feedbackWarningText;

    [Header("Navigation")]
    [SerializeField] private Button backButton;

    // Keeps track of the spawned UI rows by their actionId (e.g., "Parry")
    private readonly Dictionary<string, KeybindRowUI> spawnedRows = new Dictionary<string, KeybindRowUI>();

    // Events for the OptionsController to listen to
    public event Action<float> OnBgVolumeChanged;
    public event Action<float> OnActionVolumeChanged;
    public event Action<float> OnMouseSensitivityChanged;
    public event Action<float> OnBrightnessChanged;
    public event Action<bool> OnPostProcessingChanged;
    public event Action<string> OnStartRebind;
    public event Action OnBackClicked;

    private void Awake()
    {
        bgVolumeSlider.onValueChanged.AddListener(val => OnBgVolumeChanged?.Invoke(val));
        actionVolumeSlider.onValueChanged.AddListener(val => OnActionVolumeChanged?.Invoke(val));
        mouseSensitivitySlider.onValueChanged.AddListener(val => OnMouseSensitivityChanged?.Invoke(val));
        brightnessSlider.onValueChanged.AddListener(val => OnBrightnessChanged?.Invoke(val));
        postProcessingToggle.onValueChanged.AddListener(isOn => OnPostProcessingChanged?.Invoke(isOn));
        backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
    }

    // Called by OptionsController when opening the Options menu to match UI to saved data
    public void SyncSettingsUI(SettingsSaveData data)
    {
        // SetValueWithoutNotify updates the slider position without triggering a redundant save event
        bgVolumeSlider.SetValueWithoutNotify(data.bgVolume);
        actionVolumeSlider.SetValueWithoutNotify(data.actionVolume);
        mouseSensitivitySlider.SetValueWithoutNotify(data.mouseSensitivity);
        brightnessSlider.SetValueWithoutNotify(data.brightness);
        postProcessingToggle.SetIsOnWithoutNotify(data.postProcessing);

        BuildOrUpdateKeybindRows(data.keybinds);
        ShowWarningMessage("");
    }

    private void BuildOrUpdateKeybindRows(List<KeybindEntry> keybinds)
    {
        for (int i = 0; i < keybinds.Count; i++)
        {
            KeybindEntry entry = keybinds[i];

            if (!spawnedRows.TryGetValue(entry.actionId, out KeybindRowUI row))
            {
                row = Instantiate(keybindRowPrefab, controlsListContainer);
                row.OnRebindRequested += id => OnStartRebind?.Invoke(id);
                spawnedRows[entry.actionId] = row;
            }

            row.Setup(entry.actionId, entry.displayName, entry.key);
        }
    }

    // Updates a single button's text after the player presses a new key
    public void SetKeyTextForAction(string actionId, string keyText)
    {
        if (spawnedRows.TryGetValue(actionId, out KeybindRowUI row))
        {
            row.UpdateKeyText(keyText);
        }
    }

    // Shows or hides the warning text (e.g., when a duplicate key is pressed)
    public void ShowWarningMessage(string message)
    {
        if (feedbackWarningText != null)
        {
            feedbackWarningText.text = message;
            feedbackWarningText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}