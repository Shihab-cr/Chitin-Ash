using UnityEngine;
/// <summary>
/// MiddleMan betw our saving files system and the Controller, also holds the active player progress in memory(ram) during gameplay
/// </summary>
public class GameProgressModel
{
    // A single shared instance in memory so any controller can access current player progress
    public static GameProgressModel Instance { get; private set; } = new GameProgressModel();

    // Holds the live player data (Checkpoint position, Biomass, Aphids)
    public PlayerSaveData CurrentData { get; private set; } = new PlayerSaveData();

    //Checks if a saved game exists on disk 
    public bool HasSavedFile()
    {
        return SaveSystem.HasProgressSave();
    }

    //Deletes saved player data and resets memory for a New Game (....doNOT delete saved settings )
    public void StartNewGame()
    {
        SaveSystem.DeleteProgressSave();
        CurrentData = new PlayerSaveData();
        SaveCurrentProgress(); //to register new game on disk
    }

    //Loads saved progress from disk into memory
    public void LoadSavedGame()
    {
        CurrentData = SaveSystem.LoadProgress();
    }

    //Saves current memory to disk (called after death on Game Over & at Checkpoints)
    public void SaveCurrentProgress()
    {
        SaveSystem.SaveProgress(CurrentData);
    }

    //Updates the nearest checkpoint position
    public void SetCheckpoint(string checkpointId, Vector3 position)
    {
        CurrentData.lastCheckpointId = checkpointId;
        CurrentData.lastCheckpointPosition = position;
        SaveCurrentProgress();
    }
}