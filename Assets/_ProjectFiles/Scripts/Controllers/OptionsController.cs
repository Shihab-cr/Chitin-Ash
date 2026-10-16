using System;
using System.Collections;
using UnityEngine;

public class OptionsController : MonoBehaviour
{
    [SerializeField] private OptionsView optionsView;

    private SettingsModel settingsModel;
    private bool isRebinding = false;
    private Action onCloseCallback;

    private void Start()
    {
        settingsModel = new SettingsModel();
        settingsModel.Load();

        // Subscribe to View events
        optionsView.OnBgVolumeChanged += val => settingsModel.SetBgVolume(val);
        optionsView.OnActionVolumeChanged += val => settingsModel.SetActionVolume(val);
        optionsView.OnMouseSensitivityChanged += val => settingsModel.SetMouseSensitivity(val);
        optionsView.OnBrightnessChanged += val => settingsModel.SetBrightness(val);
        optionsView.OnPostProcessingChanged += isOn => settingsModel.SetPostProcessing(isOn);
        optionsView.OnStartRebind += HandleStartRebind;
        optionsView.OnBackClicked += HandleBackClicked;

        optionsView.Show(false);
    }

    // Called by MainMenuController or PauseMenuController when clicking "Options"
    public void OpenOptions(Action onClose)
    {
        onCloseCallback = onClose;
        settingsModel.Load();
        optionsView.SyncSettingsUI(settingsModel.Data);
        optionsView.Show(true);
    }

    private void HandleStartRebind(string actionId)
    {
        // Ignore clicks if we are already waiting for a key press
        if (isRebinding) return;
        StartCoroutine(RebindCoroutine(actionId));
    }

    private IEnumerator RebindCoroutine(string actionId)
    {
        isRebinding = true;
        optionsView.ShowWarningMessage("Press any key or mouse button (ESC to cancel)...");
        optionsView.SetKeyTextForAction(actionId, "...");

        // 1. Wait until the mouse click that pressed the UI button is released
        yield return new WaitUntil(() => !Input.anyKey);

        // 2. Wait for the player to press their desired new key/button
        yield return new WaitUntil(() => Input.anyKeyDown);

        KeyCode pressedKey = KeyCode.None;
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                pressedKey = keyCode;
                break;
            }
        }

        // 3. If player pressed ESC, cancel rebinding and restore previous key text
        if (pressedKey == KeyCode.Escape || pressedKey == KeyCode.None)
        {
            KeyCode oldKey = settingsModel.GetKeyForAction(actionId);
            optionsView.SetKeyTextForAction(actionId, oldKey.ToString());
            optionsView.ShowWarningMessage("");
        }
        else
        {
            // 4. Ask the Model to rebind (Model checks if another action already uses this key!)
            if (settingsModel.TryRebindKey(actionId, pressedKey, out string conflictingAction))
            {
                // Success! Update UI to show the player's new control
                optionsView.SetKeyTextForAction(actionId, pressedKey.ToString());
                optionsView.ShowWarningMessage("");
            }
            else
            {
                // Duplicate key blocked! Restore previous key text and show warning
                KeyCode oldKey = settingsModel.GetKeyForAction(actionId);
                optionsView.SetKeyTextForAction(actionId, oldKey.ToString());
                optionsView.ShowWarningMessage($"Cannot use '{pressedKey}'! Already bound to '{conflictingAction}'.");
            }
        }

        // Wait one frame so pressing ESC to cancel doesn't accidentally toggle the Pause Menu
        yield return null;
        isRebinding = false;
    }

    private void HandleBackClicked()
    {
        if (isRebinding) return;

        optionsView.Show(false);
        onCloseCallback?.Invoke();
    }
}