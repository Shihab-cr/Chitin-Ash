/// <summary>
/// MVC Model (pure C#, no MonoBehaviour): level rules -> easy to unit test.
/// Only knows the rules; spending the scraps is done by ProgressionController.
/// </summary>
public class PlayerLevelModel
{
    private readonly LevelProgressionSO config;
    private readonly ProgressionSaveData data;

    public PlayerLevelModel(LevelProgressionSO config, ProgressionSaveData data)
    {
        this.config = config;
        this.data = data;
    }

    public int Level => data.playerLevel;
    public int MaxLevel => config.maxLevel;
    public bool IsMaxLevel => config.IsMaxLevel(Level);

    // 0 when already at max level
    public int NextLevelCost => IsMaxLevel ? 0 : config.GetLevelUpCost(Level);

    public bool CanLevelUp(int availableScraps)
    {
        return !IsMaxLevel && availableScraps >= NextLevelCost;
    }

    public PlayerStats GetLevelStats(int level)
    {
        return config.GetStatsForLevel(level);
    }

    // Raises the level by one. Returns false if already maxed.
    public bool ApplyLevelUp()
    {
        if (IsMaxLevel) return false;

        data.playerLevel++;
        return true;
    }
}
