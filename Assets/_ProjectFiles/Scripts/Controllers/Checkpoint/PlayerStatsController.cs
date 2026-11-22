using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Put on the Player. Pushes the final stats (level + skills) into the player's components.
/// Health goes straight into HealthHandler. Everything else is exposed as UnityEvents so the
/// stamina / combat / dodge owners can hook them in the Inspector without this script knowing them.
///
/// Example: drag PlayerStaminaManager into "On Max Stamina Increased By" and pick UpgradeStamina.
/// </summary>
public class PlayerStatsController : MonoBehaviour
{
    [SerializeField] private HealthHandler healthHandler;

    [Header("Final values (new total)")]
    public UnityEvent<float> OnMaxStaminaChanged;
    public UnityEvent<float> OnDamageChanged;
    public UnityEvent<float> OnStaminaRegenBonusChanged;
    public UnityEvent<float> OnBlockDamageReductionChanged;
    public UnityEvent<float> OnBlockStaminaCostBonusChanged;
    public UnityEvent<float> OnDodgeDistanceBonusChanged;

    [Header("Increase only (for scripts that ADD, like PlayerStaminaManager.UpgradeStamina)")]
    public UnityEvent<float> OnMaxStaminaIncreasedBy;

    private ProgressionModel model;
    private float lastAppliedStamina;

    // Start (not Awake) so the ProgressionModel singleton already exists (Section 26, Bug 1)
    private void Start()
    {
        if (healthHandler == null) healthHandler = GetComponentInChildren<HealthHandler>();

        model = ProgressionModel.Instance;
        if (model == null)
        {
            Debug.LogWarning("[PlayerStatsController] No ProgressionController in the scene (it creates ProgressionModel).", this);
            return;
        }

        // The stamina script starts with the level-1 value, so increases are measured from there
        lastAppliedStamina = model.BaseStats.maxStamina;

        model.OnStatsChanged += ApplyStats;
        ApplyStats(model.CurrentStats);
    }

    private void OnDestroy()
    {
        if (model != null) model.OnStatsChanged -= ApplyStats;
    }

    private void ApplyStats(PlayerStats stats)
    {
        if (healthHandler != null)
        {
            healthHandler.MaxHealth = stats.maxHealth;
        }

        OnMaxStaminaChanged?.Invoke(stats.maxStamina);
        if (stats.maxStamina > lastAppliedStamina)
        {
            OnMaxStaminaIncreasedBy?.Invoke(stats.maxStamina - lastAppliedStamina);
        }
        lastAppliedStamina = stats.maxStamina;

        OnDamageChanged?.Invoke(stats.damage);
        OnStaminaRegenBonusChanged?.Invoke(stats.staminaRegen);
        OnBlockDamageReductionChanged?.Invoke(stats.blockDamageReduction);
        OnBlockStaminaCostBonusChanged?.Invoke(stats.blockStaminaCost);
        OnDodgeDistanceBonusChanged?.Invoke(stats.dodgeDistance);
    }
}
