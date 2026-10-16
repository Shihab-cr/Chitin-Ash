using System.IO;
using UnityEngine;


/// <summary>
/// what this file does : taking (SettingsSaveData and PlayerAndWorldSaveData) from SaveDataStructures file, convert them into JSON text, and save or load them from two separate files on the computer's hard drive
/// </summary>
public static class SaveSystem
{
    // Two separate file paths so deleting player progress doNOT delete player settings!
    private static string ProgressFilePath => Path.Combine(Application.persistentDataPath, "chitin_progress.json");
    private static string SettingsFilePath => Path.Combine(Application.persistentDataPath, "chitin_settings.json");

    
    // 1. PLAYER PROGRESS SAVING (New Game / Load Game / Death)

    // Checks if a save file exists (used to enable/disable the "Load Game" button & show overwrite warning)
    public static bool HasProgressSave()
    {
        return File.Exists(ProgressFilePath);
    }

    // Writes the player's progress to disk as formatted JSON
    public static void SaveProgress(PlayerSaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(ProgressFilePath, json);
    }

    // Reads the player's progress from disk (or returns fresh data if no file exists)
    public static PlayerSaveData LoadProgress()
    {
        if (!HasProgressSave())
        {
            return new PlayerSaveData();
        }

        string json = File.ReadAllText(ProgressFilePath);
        return JsonUtility.FromJson<PlayerSaveData>(json);
    }

    // Deletes ONLY the progress file when starting a New Game (leaves settings untouched!)
    public static void DeleteProgressSave()
    {
        if (HasProgressSave())
        {
            File.Delete(ProgressFilePath);
        }
    }




    //,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,;),,,,,,,,,,,,,,,,,,//

    // 2. SETTINGS & CONTROLS SAVING (Options Menu)
 

    public static void SaveSettings(SettingsSaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SettingsFilePath, json);
    }

    public static SettingsSaveData LoadSettings()
    {
        // If the player opens the game for the very first time, create default GDD settings
        if (!File.Exists(SettingsFilePath))
        {
            SettingsSaveData defaultSettings = SettingsSaveData.CreateDefault();
            SaveSettings(defaultSettings);
            return defaultSettings;
        }

        string json = File.ReadAllText(SettingsFilePath);
        SettingsSaveData loaded = JsonUtility.FromJson<SettingsSaveData>(json);

        // Safety fallback in case the file was empty or corrupted
        if (loaded == null || loaded.keybinds == null || loaded.keybinds.Count == 0)
        {
            loaded = SettingsSaveData.CreateDefault();
        }

        return loaded;
    }
}