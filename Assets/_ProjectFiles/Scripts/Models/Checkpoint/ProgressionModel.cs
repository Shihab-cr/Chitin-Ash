using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MVC Model: holds the live progression data in memory (level, skills, quests) during gameplay,
/// like GameProgressModel does for checkpoint / aphids / biomass.
/// Pure C# (no MonoBehaviour). Created by ProgressionController, read by every checkpoint controller.
///
/// It owns three smaller models:
///   PlayerLevelModel -> Stats tab   (level + scrap costs)
///   SkillModel       -> Skills tab  (tiers + requirements + bonuses)
///   QuestModel       -> Quests tab  (goals of the beetle)
/// </summary>
public class ProgressionModel
{
    // A single shared instance in memory so any controller can read the current progression
    public static ProgressionModel Instance { get; private set; }

    public PlayerLevelModel Level { get; private set; }
    public SkillModel Skills { get; private set; }
    public QuestModel Quests { get; private set; }

    // Anything changed (level, skill, quest, currency) -> controllers refresh their views
    public event Action OnProgressionChanged;
    // Final stats changed -> PlayerStatsController pushes them to the player
    public event Action<PlayerStats> OnStatsChanged;
    // A quest was completed (controller gives scrap rewards, HUD can show a message)
    public event Action<QuestDefinitionSO> OnQuestCompleted;
    public event Action OnGameSaved;

    private readonly LevelProgressionSO levelConfig;
    private readonly List<SkillDefinitionSO> skillConfigs;
    private readonly List<QuestDefinitionSO> questConfigs;
    private ProgressionSaveData data;

    // Called once by ProgressionController.Awake() (loads the save file)
    public static ProgressionModel Create(LevelProgressionSO levelConfig, List<SkillDefinitionSO> skills, List<QuestDefinitionSO> quests)
    {
        Instance = new ProgressionModel(levelConfig, skills, quests);
        return Instance;
    }

    private ProgressionModel(LevelProgressionSO levelConfig, List<SkillDefinitionSO> skills, List<QuestDefinitionSO> quests)
    {
        this.levelConfig = levelConfig;
        skillConfigs = skills ?? new List<SkillDefinitionSO>();
        questConfigs = quests ?? new List<QuestDefinitionSO>();

        data = ProgressionSaveSystem.Load();
        BuildModels();
        SyncStateObjectives();
    }

    // ---------------- Currencies (stored by the team's existing models) ----------------

    // Scraps are written by PlayerScrapesHandler through PlayerDataModel (PlayerPrefs) on every change
    public int CurrentScraps => PlayerDataModel.LoadScrapesAmount();

    // Red aphids live in the team's PlayerSaveData (GameProgressModel)
    public int CurrentRedAphids => GameProgressModel.Instance.CurrentData.redAphids;

    // ---------------- Stats ----------------

    // Level stats + all unlocked skill bonuses
    public PlayerStats GetStatsAtLevel(int level)
    {
        PlayerStats stats = Level.GetLevelStats(level);
        Skills.AddSkillBonuses(ref stats);
        return stats;
    }

    public PlayerStats CurrentStats => GetStatsAtLevel(Level.Level);

    // Stats with NO skills at level 1 (starting point of the player's scripts)
    public PlayerStats BaseStats => Level.GetLevelStats(1);

    // ---------------- Stats tab ----------------

    public bool CanLevelUp()
    {
        return Level.CanLevelUp(CurrentScraps);
    }

    // Called by ProgressionController AFTER it spent the scraps through PlayerScrapesHandler
    public void ApplyLevelUp()
    {
        if (!Level.ApplyLevelUp()) return;

        SyncStateObjectives();
        CommitPurchase();
        NotifyChanged();
    }

    // ---------------- Skills tab ----------------

    public bool CanUnlockSkill(SkillDefinitionSO skill)
    {
        return skill != null && Skills.CanUnlockNextTier(skill, Level.Level, CurrentRedAphids);
    }

    public List<RequirementStatus> GetSkillRequirements(SkillDefinitionSO skill)
    {
        return Skills.GetRequirements(skill, Level.Level, CurrentRedAphids);
    }

    public bool TryUnlockSkill(SkillDefinitionSO skill)
    {
        if (!CanUnlockSkill(skill)) return false;

        SkillTier tier = Skills.GetNextTier(skill);
        GameProgressModel.Instance.CurrentData.redAphids -= tier.redAphidCost;
        Skills.UnlockNextTier(skill);

        Quests.AddProgress(QuestObjectiveType.UnlockSkill, skill.skillId, 1);
        CommitPurchase();
        NotifyChanged();
        return true;
    }

    // ---------------- Gameplay records (sent here by ProgressionController) ----------------

    public void RecordEnemyDefeated(string enemyId)
    {
        Quests.AddProgress(QuestObjectiveType.DefeatEnemy, enemyId, 1);
        NotifyChanged();
    }

    public void RecordEnemyConsumed(string enemyId)
    {
        Skills.RecordEnemyConsumed(enemyId);
        Quests.AddProgress(QuestObjectiveType.ConsumeEnemy, enemyId, 1);
        NotifyChanged();
    }

    public void RecordEliteDefeated(string eliteId)
    {
        Skills.RecordEliteDefeated(eliteId);
        Quests.AddProgress(QuestObjectiveType.DefeatElite, eliteId, 1);
        NotifyChanged();
    }

    public void RecordRedAphidsCollected(int amount)
    {
        GameProgressModel.Instance.CurrentData.redAphids += amount;
        Quests.AddProgress(QuestObjectiveType.CollectRedAphids, "", amount);
        NotifyChanged();
    }

    public void RecordCheckpointRested(string checkpointId)
    {
        Quests.AddProgress(QuestObjectiveType.RestAtCheckpoint, checkpointId, 1);
        NotifyChanged();
    }

    // ---------------- Saving ----------------

    /// <summary>Saves EVERYTHING: team progress file, PlayerPrefs (health, position) and the progression file.</summary>
    public void SaveAll(float currentHealth)
    {
        GameProgressModel progress = GameProgressModel.Instance;
        progress.CurrentData.biomass = CurrentScraps; // keep the team's save file in sync with the real scraps

        PlayerDataModel.SaveHealth(currentHealth);

        // Player respawns at the last checkpoint
        Vector3 respawn = progress.CurrentData.lastCheckpointPosition;
        PlayerDataModel.SavePlayerPosition(respawn.x, respawn.y, respawn.z);
        PlayerPrefs.Save();

        progress.SaveCurrentProgress();
        ProgressionSaveSystem.Save(data);

        OnGameSaved?.Invoke();
    }

    /// <summary>New Game: wipes levels / skills / quests (settings and the team's file are handled by GameProgressModel).</summary>
    public void ResetForNewGame()
    {
        ProgressionSaveSystem.DeleteSave();
        data = new ProgressionSaveData();
        BuildModels();
        SyncStateObjectives();
        NotifyChanged();
    }

    // Scraps are written to PlayerPrefs the moment they're spent, so the purchase is written too
    // (otherwise quitting without saving loses the level but keeps the cost)
    private void CommitPurchase()
    {
        GameProgressModel.Instance.SaveCurrentProgress();
        ProgressionSaveSystem.Save(data);
    }

    // ---------------- Helpers ----------------

    private void BuildModels()
    {
        if (Quests != null) Quests.OnQuestCompleted -= HandleQuestCompleted;

        Level = new PlayerLevelModel(levelConfig, data);
        Skills = new SkillModel(skillConfigs, data);
        Quests = new QuestModel(questConfigs, data);
        Quests.OnQuestCompleted += HandleQuestCompleted;
    }

    private void HandleQuestCompleted(QuestDefinitionSO quest)
    {
        if (quest.rewardRedAphids > 0)
        {
            GameProgressModel.Instance.CurrentData.redAphids += quest.rewardRedAphids;
        }

        // A new quest may have been unlocked -> give it the level the player already has
        SyncStateObjectives();
        OnQuestCompleted?.Invoke(quest);
    }

    // Objectives that depend on the current state (not on events)
    private void SyncStateObjectives()
    {
        Quests.SetProgressAtLeast(QuestObjectiveType.ReachPlayerLevel, "", Level.Level);
    }

    public void NotifyChanged()
    {
        OnProgressionChanged?.Invoke();
        OnStatsChanged?.Invoke(CurrentStats);
    }
}
