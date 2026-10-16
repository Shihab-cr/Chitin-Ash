using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// making SettingsSaveData and PlayerAndWorldSaveData containers
/// </summary>


//1.single control binding //all linked 
[System.Serializable] //if this is not written, Unity will skip the struct (and probably show an error/warning like what happened with me)
public struct KeybindEntry
{
    public string actionId;     //Internal ID used by code
    public string displayName;  //Text shown to the player on the UI
    public KeyCode key;         //The actual keyboard/mouse button
}

//2.Container for all Options/Settings (Seperated logic to not be wiped by a New Game)
[Serializable]
public class SettingsSaveData
{
    public float bgVolume = 1.0f;
    public float actionVolume = 1.0f;
    public float mouseSensitivity = 1.0f;
    public float brightness = 1.0f;
    public bool postProcessing = true;

    // Using a List ((for anyone reading, it was a dictionary then realised Unity JSON cant read dictionaries!))
    public List<KeybindEntry> keybinds = new List<KeybindEntry>();

    // Helper function that loads all default controls (I got them from the gdd)
    public static SettingsSaveData CreateDefault()
    {
        SettingsSaveData data = new SettingsSaveData();

        data.keybinds.Add(new KeybindEntry { actionId = "MoveForward", displayName = "Move Forward", key = KeyCode.W });
        data.keybinds.Add(new KeybindEntry { actionId = "MoveBackward", displayName = "Move Backward", key = KeyCode.S });
        data.keybinds.Add(new KeybindEntry { actionId = "MoveLeft", displayName = "Move Left", key = KeyCode.A });
        data.keybinds.Add(new KeybindEntry { actionId = "MoveRight", displayName = "Move Right", key = KeyCode.D });
        data.keybinds.Add(new KeybindEntry { actionId = "LightAttack", displayName = "Light Attack", key = KeyCode.Mouse0 });
        data.keybinds.Add(new KeybindEntry { actionId = "HeavyAttack", displayName = "Heavy Attack", key = KeyCode.Mouse1 });
        data.keybinds.Add(new KeybindEntry { actionId = "Dodge", displayName = "Dodge / Leap", key = KeyCode.Space });
        data.keybinds.Add(new KeybindEntry { actionId = "Parry", displayName = "Parry", key = KeyCode.Q });
        data.keybinds.Add(new KeybindEntry { actionId = "Block", displayName = "Block (Hold)", key = KeyCode.E });
        data.keybinds.Add(new KeybindEntry { actionId = "Sprint", displayName = "Sprint (Hold)", key = KeyCode.LeftShift });
        data.keybinds.Add(new KeybindEntry { actionId = "Interact", displayName = "Interact / Consume", key = KeyCode.F });
        data.keybinds.Add(new KeybindEntry { actionId = "LockOn", displayName = "Lock-On Toggle", key = KeyCode.Z });

        return data;
    }
}

//3.Container for Player Progress (Deleted when starting a New Game)
[Serializable]
public class PlayerSaveData
{
    public string lastCheckpointId = "CP1_HollowLog"; //initialized with first checkpoint name from gdd till changed during game
    public Vector3 lastCheckpointPosition = Vector3.zero;
    public int biomass = 0;
    public int redAphids = 0;
}