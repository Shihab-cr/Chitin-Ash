using UnityEngine;
/// <summary>
/// holds values for 4 sliders / updates them to SaveSystem file when changed / makes sure no key is assigned to multiple skills 
/// </summary>
public class SettingsModel
{
    public SettingsSaveData Data { get; private set; }

    // Loads settings from disk when the game starts
    public void Load()
    {
        Data = SaveSystem.LoadSettings();
    }

    // Saves settings to disk whenever the player changes an option
    public void Save()
    {
        SaveSystem.SaveSettings(Data);
    }

    //setters of sliders and checkboxs
    public void SetBgVolume(float val) { Data.bgVolume = val; Save(); }
    public void SetActionVolume(float val) { Data.actionVolume = val; Save(); }
    public void SetMouseSensitivity(float val) { Data.mouseSensitivity = val; Save(); }
    public void SetBrightness(float val) { Data.brightness = val; Save(); }
    public void SetPostProcessing(bool enabled) { Data.postProcessing = enabled; Save(); }

    // Returns the current KeyCode for a specific action 
    public KeyCode GetKeyForAction(string actionId)
    {
        for (int i = 0; i < Data.keybinds.Count; i++)
        {
            if (Data.keybinds[i].actionId == actionId)
                return Data.keybinds[i].key;
        }
        return KeyCode.None;
    }

    //Ke rebinding (NO DUPLICATE KEYS ALLOWED)
    public bool TryRebindKey(string actionId, KeyCode newKey, out string conflictingActionName)
    {
        conflictingActionName = null;

        // 1. Loop through all controls to see if ANOTHER action already uses 'newKey'
        for (int i = 0; i < Data.keybinds.Count; i++)
        {
            if (Data.keybinds[i].actionId != actionId && Data.keybinds[i].key == newKey)
            {
                // Conflict found! Tell the Controller which action already uses this key
                conflictingActionName = Data.keybinds[i].displayName;
                return false;
            }
        }

        //Assign the new key to this action and save to disk
        for (int i = 0; i < Data.keybinds.Count; i++)
        {
            if (Data.keybinds[i].actionId == actionId)
            {
                KeybindEntry updated = Data.keybinds[i];
                updated.key = newKey;
                Data.keybinds[i] = updated;
                Save();
                return true;
            }
        }

        return false;
    }
}