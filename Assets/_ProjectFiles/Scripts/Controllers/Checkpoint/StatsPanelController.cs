using UnityEngine;

/// <summary>
/// MVC Controller for the Stats tab: shows level / scraps / stats and spends scraps to level up.
/// Initialized by CheckpointMenuController.
/// </summary>
public class StatsPanelController : MonoBehaviour
{
    [SerializeField] private StatsPanelView view;

    [Header("Text formats (designer can change the wording)")]
    [SerializeField] private string levelFormat = "Level {0}";
    [SerializeField] private string scrapsFormat = "Scraps: {0}";
    [SerializeField] private string costFormat = "Level Up: {0} Scraps";
    [SerializeField] private string maxLevelText = "MAX LEVEL";
    [Tooltip("{0} = current value, {1} = value after level up")]
    [SerializeField] private string statWithPreviewFormat = "{0}  ->  {1}";
    [SerializeField] private string healthLabel = "Health: ";
    [SerializeField] private string staminaLabel = "Stamina: ";
    [SerializeField] private string damageLabel = "Damage: ";

    private ProgressionModel model;

    public void Initialize(ProgressionModel progressionModel)
    {
        model = progressionModel;
        view.OnLevelUpClicked += HandleLevelUpClicked;
        model.OnProgressionChanged += RefreshIfVisible;
    }

    private void OnDestroy()
    {
        if (view != null) view.OnLevelUpClicked -= HandleLevelUpClicked;
        if (model != null) model.OnProgressionChanged -= RefreshIfVisible;
    }

    public void Refresh()
    {
        if (model == null || view == null) return;

        PlayerLevelModel level = model.Level;
        PlayerStats current = model.CurrentStats;
        bool isMax = level.IsMaxLevel;
        PlayerStats next = isMax ? current : model.GetStatsAtLevel(level.Level + 1);

        StatsViewData data = new StatsViewData
        {
            level = string.Format(levelFormat, level.Level),
            scraps = string.Format(scrapsFormat, model.CurrentScraps),
            levelUpCost = isMax ? maxLevelText : string.Format(costFormat, level.NextLevelCost),
            health = healthLabel + FormatStat(current.maxHealth, next.maxHealth, isMax),
            stamina = staminaLabel + FormatStat(current.maxStamina, next.maxStamina, isMax),
            damage = damageLabel + FormatStat(current.damage, next.damage, isMax),
            canLevelUp = model.CanLevelUp(),
            isMaxLevel = isMax
        };

        view.Render(data);
    }

    private void HandleLevelUpClicked()
    {
        // Spending scraps needs the player's PlayerScrapesHandler -> ProgressionController does it
        if (ProgressionController.Instance != null) ProgressionController.Instance.TryLevelUp(); // model fires OnProgressionChanged -> Refresh
    }

    private void RefreshIfVisible()
    {
        if (view != null && view.gameObject.activeInHierarchy) Refresh();
    }

    private string FormatStat(float current, float next, bool isMax)
    {
        if (isMax || Mathf.Approximately(current, next)) return current.ToString("0.#");
        return string.Format(statWithPreviewFormat, current.ToString("0.#"), next.ToString("0.#"));
    }
}
