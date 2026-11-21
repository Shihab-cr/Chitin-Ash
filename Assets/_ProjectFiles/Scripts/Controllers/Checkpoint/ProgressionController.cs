using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// MVC Controller: bridges the scene and ProgressionModel.
///   - creates ProgressionModel from the designer's ScriptableObjects
///   - forwards gameplay reports (ProgressionEvents) into the model
///   - spends / gives scraps through the player's PlayerScrapesHandler
///   - saves with the player's current health
/// Scene-placed Singleton (Section 26). Put ONE in the gameplay scene.
/// </summary>
public class ProgressionController : MonoBehaviour
{
    public static ProgressionController Instance { get; private set; }

    [Header("Designer Data (ScriptableObjects)")]
    [SerializeField] private LevelProgressionSO levelProgression;
    [SerializeField] private List<SkillDefinitionSO> skills = new List<SkillDefinitionSO>();
    [SerializeField] private List<QuestDefinitionSO> quests = new List<QuestDefinitionSO>();
    public UnityEvent Onsucssesful_level_up;

    [Header("Player References (found by the 'Player' tag if left empty)")]
    [SerializeField] private PlayerScrapesHandler scrapesHandler;
    [SerializeField] private HealthHandler playerHealth;

    private ProgressionModel model;

    private void Awake()
    {
        // Singleton boilerplate: only one may exist.
        // Destroy only this component (not the GameObject) so a duplicate added by mistake
        // on another object (e.g. the Player) never deletes that object.
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"[ProgressionController] Duplicate found on '{name}', removing it. Keep only one in the scene.", this);
            Destroy(this);
            return;
        }
        Instance = this;

        if (levelProgression == null)
        {
            Debug.LogWarning("[ProgressionController] No LevelProgressionSO assigned, using default values.", this);
            levelProgression = ScriptableObject.CreateInstance<LevelProgressionSO>();
        }

        // Created in Awake so other scripts can use ProgressionModel.Instance in their Start()
        model = ProgressionModel.Create(levelProgression, skills, quests);
        model.OnQuestCompleted += GiveQuestScrapReward;
    }

    private void OnEnable()
    {
        if (Instance != this) return; // duplicate being removed

        ProgressionEvents.OnEnemyDefeated += HandleEnemyDefeated;
        ProgressionEvents.OnEnemyConsumed += HandleEnemyConsumed;
        ProgressionEvents.OnEliteDefeated += HandleEliteDefeated;
        ProgressionEvents.OnRedAphidsCollected += HandleRedAphidsCollected;
        ProgressionEvents.OnCheckpointRested += HandleCheckpointRested;
    }

    private void OnDisable()
    {
        if (Instance != this) return;

        ProgressionEvents.OnEnemyDefeated -= HandleEnemyDefeated;
        ProgressionEvents.OnEnemyConsumed -= HandleEnemyConsumed;
        ProgressionEvents.OnEliteDefeated -= HandleEliteDefeated;
        ProgressionEvents.OnRedAphidsCollected -= HandleRedAphidsCollected;
        ProgressionEvents.OnCheckpointRested -= HandleCheckpointRested;
    }

    private void Start()
    {
        if (Instance != this) return;

        FindPlayerReferences();
        model.NotifyChanged();
    }

    private void OnDestroy()
    {
        if (model != null) model.OnQuestCompleted -= GiveQuestScrapReward;
        if (Instance == this) Instance = null;
    }

    // ---------------- Actions used by the checkpoint menu controllers ----------------

    public bool TryLevelUp()
    {
        if (!model.CanLevelUp()) return false;

        if (scrapesHandler == null)
        {
            Debug.LogWarning("[ProgressionController] No PlayerScrapesHandler found, can't spend scraps.", this);
            return false;
        }

        if (!scrapesHandler.UseScrapes(model.Level.NextLevelCost)) return false;
        Onsucssesful_level_up?.Invoke();
        model.ApplyLevelUp();
        return true;
    }

    public void SaveAll()
    {
        float health = playerHealth != null ? playerHealth.CurrHealth : PlayerDataModel.LoadHealth();
        model.SaveAll(health);
    }

    // Call from the Main Menu "New Game"
    public void ResetForNewGame()
    {
        model.ResetForNewGame();
    }

    // ---------------- Gameplay reports -> model ----------------

    private void HandleEnemyDefeated(string enemyId) => model.RecordEnemyDefeated(enemyId);
    private void HandleEnemyConsumed(string enemyId) => model.RecordEnemyConsumed(enemyId);
    private void HandleEliteDefeated(string eliteId) => model.RecordEliteDefeated(eliteId);
    private void HandleRedAphidsCollected(int amount) => model.RecordRedAphidsCollected(amount);
    private void HandleCheckpointRested(string checkpointId) => model.RecordCheckpointRested(checkpointId);

    private void GiveQuestScrapReward(QuestDefinitionSO quest)
    {
        if (quest.rewardScraps > 0 && scrapesHandler != null)
        {
            scrapesHandler.AddScrapesToPlayer(quest.rewardScraps);
        }
    }

    private void FindPlayerReferences()
    {
        if (scrapesHandler != null && playerHealth != null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        if (scrapesHandler == null) scrapesHandler = player.GetComponentInChildren<PlayerScrapesHandler>();
        if (playerHealth == null) playerHealth = player.GetComponentInChildren<HealthHandler>();
    }
}
