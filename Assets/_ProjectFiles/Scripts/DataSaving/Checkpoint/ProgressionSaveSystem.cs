using System.IO;
using UnityEngine;

/// <summary>
/// Saves / loads ProgressionSaveData as JSON (same style as the team's SaveSystem.cs).
/// File: persistentDataPath/chitin_progression.json
/// </summary>
public static class ProgressionSaveSystem
{
    private static string FilePath => Path.Combine(Application.persistentDataPath, "chitin_progression.json");

    public static bool HasSave()
    {
        return File.Exists(FilePath);
    }

    public static void Save(ProgressionSaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FilePath, json);
    }

    public static ProgressionSaveData Load()
    {
        if (!HasSave())
        {
            return new ProgressionSaveData();
        }

        ProgressionSaveData data = JsonUtility.FromJson<ProgressionSaveData>(File.ReadAllText(FilePath));

        // Safety fallback in case the file was empty or corrupted
        if (data == null)
        {
            data = new ProgressionSaveData();
        }

        data.EnsureValid();
        return data;
    }

    // Call this on New Game (together with SaveSystem.DeleteProgressSave)
    public static void DeleteSave()
    {
        if (HasSave())
        {
            File.Delete(FilePath);
        }
    }
}
