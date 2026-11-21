using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// MVC Controller for the Skills tab:
///  - shows an icon for every skill
///  - pressing an icon shows its requirements + what it gives
///  - after unlocking, shows the requirements of the NEXT upgrade
/// Initialized by CheckpointMenuController.
/// </summary>
public class SkillsPanelController : MonoBehaviour
{
    [SerializeField] private SkillsPanelView view;

    [Header("Text formats (designer can change the wording)")]
    [SerializeField] private string redAphidsFormat = "Red Aphids: {0}";
    [SerializeField] private string lockedText = "Locked";
    [SerializeField] private string tierFormat = "Tier {0} / {1}";
    [SerializeField] private string fullyUpgradedText = "Fully Upgraded";
    [SerializeField] private string unlockLabel = "Unlock";
    [SerializeField] private string upgradeLabel = "Upgrade";
    [SerializeField] private string noRequirementsText = "No requirements";
    [SerializeField] private string noBonusesText = "-";
    [SerializeField] private string metColor = "#7CFC00";
    [SerializeField] private string notMetColor = "#FF5555";

    private ProgressionModel model;
    private int selectedIndex = -1;

    public void Initialize(ProgressionModel progressionModel)
    {
        model = progressionModel;
        view.OnSkillSlotClicked += HandleSlotClicked;
        view.OnUnlockClicked += HandleUnlockClicked;
        model.OnProgressionChanged += RefreshIfVisible;
    }

    private void OnDestroy()
    {
        if (view != null)
        {
            view.OnSkillSlotClicked -= HandleSlotClicked;
            view.OnUnlockClicked -= HandleUnlockClicked;
        }
        if (model != null) model.OnProgressionChanged -= RefreshIfVisible;
    }

    public void Refresh()
    {
        if (model == null || view == null) return;

        IReadOnlyList<SkillDefinitionSO> skills = model.Skills.AllSkills;
        IReadOnlyList<SkillSlotView> slots = view.GetSlots(skills.Count);

        view.SetRedAphids(string.Format(redAphidsFormat, model.CurrentRedAphids));

        for (int i = 0; i < skills.Count && i < slots.Count; i++)
        {
            SkillDefinitionSO skill = skills[i];
            if (skill == null) continue;

            slots[i].Setup(skill.icon, skill.displayName, model.Skills.GetUnlockedTiers(skill), skill.MaxTier, GetSlotState(skill));
        }

        view.SetSelectedSlot(selectedIndex);

        if (selectedIndex >= 0 && selectedIndex < skills.Count && skills[selectedIndex] != null)
        {
            view.ShowDetails(BuildDetails(skills[selectedIndex]));
        }
        else
        {
            view.HideDetails();
        }
    }

    public void ClearSelection()
    {
        selectedIndex = -1;
    }

    private void HandleSlotClicked(int index)
    {
        selectedIndex = index;
        Refresh();
    }

    private void HandleUnlockClicked()
    {
        IReadOnlyList<SkillDefinitionSO> skills = model.Skills.AllSkills;
        if (selectedIndex < 0 || selectedIndex >= skills.Count) return;

        model.TryUnlockSkill(skills[selectedIndex]); // model fires OnProgressionChanged -> Refresh
    }

    private void RefreshIfVisible()
    {
        if (view != null && view.gameObject.activeInHierarchy) Refresh();
    }

    private SkillSlotState GetSlotState(SkillDefinitionSO skill)
    {
        if (model.Skills.IsFullyUpgraded(skill)) return SkillSlotState.FullyUpgraded;
        if (model.CanUnlockSkill(skill)) return SkillSlotState.CanUnlock;
        if (model.Skills.IsUnlocked(skill)) return SkillSlotState.Unlocked;
        return SkillSlotState.Locked;
    }

    private SkillDetailsViewData BuildDetails(SkillDefinitionSO skill)
    {
        SkillModel system = model.Skills;
        int unlocked = system.GetUnlockedTiers(skill);
        bool maxed = system.IsFullyUpgraded(skill);
        SkillTier nextTier = system.GetNextTier(skill);

        SkillDetailsViewData data = new SkillDetailsViewData
        {
            icon = skill.icon,
            name = skill.displayName,
            description = skill.description,
            currentBonuses = BuildCurrentBonuses(skill, unlocked),
            showUnlockButton = !maxed,
            canUnlock = model.CanUnlockSkill(skill),
            unlockButtonLabel = unlocked == 0 ? unlockLabel : upgradeLabel
        };

        if (maxed)
        {
            data.tier = $"{fullyUpgradedText} ({skill.MaxTier}/{skill.MaxTier})";
            data.requirements = "";
            data.bonuses = "";
        }
        else
        {
            data.tier = unlocked == 0 ? lockedText : string.Format(tierFormat, unlocked, skill.MaxTier);
            data.requirements = BuildRequirements(model.GetSkillRequirements(skill));
            data.bonuses = BuildTierText(nextTier);
        }

        return data;
    }

    private string BuildRequirements(List<RequirementStatus> requirements)
    {
        if (requirements.Count == 0) return noRequirementsText;

        StringBuilder sb = new StringBuilder();
        foreach (RequirementStatus requirement in requirements)
        {
            string color = requirement.met ? metColor : notMetColor;
            sb.AppendLine($"<color={color}>{requirement.label}</color>");
        }
        return sb.ToString().TrimEnd();
    }

    private string BuildTierText(SkillTier tier)
    {
        if (tier == null) return noBonusesText;

        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrEmpty(tier.description)) sb.AppendLine(tier.description);
        foreach (StatBonus bonus in tier.bonuses)
        {
            sb.AppendLine(FormatBonus(bonus));
        }

        string text = sb.ToString().TrimEnd();
        return text.Length > 0 ? text : noBonusesText;
    }

    // Sum of every bonus the player already owns from this skill
    private string BuildCurrentBonuses(SkillDefinitionSO skill, int unlockedTiers)
    {
        PlayerStats total = new PlayerStats();
        bool any = false;
        for (int i = 0; i < unlockedTiers && i < skill.MaxTier; i++)
        {
            foreach (StatBonus bonus in skill.tiers[i].bonuses)
            {
                total.Add(bonus);
                any = true;
            }
        }
        if (!any) return noBonusesText;

        StringBuilder sb = new StringBuilder();
        AppendIfNotZero(sb, StatType.MaxHealth, total.maxHealth);
        AppendIfNotZero(sb, StatType.MaxStamina, total.maxStamina);
        AppendIfNotZero(sb, StatType.Damage, total.damage);
        AppendIfNotZero(sb, StatType.StaminaRegen, total.staminaRegen);
        AppendIfNotZero(sb, StatType.BlockDamageReduction, total.blockDamageReduction);
        AppendIfNotZero(sb, StatType.BlockStaminaCost, total.blockStaminaCost);
        AppendIfNotZero(sb, StatType.DodgeDistance, total.dodgeDistance);
        return sb.ToString().TrimEnd();
    }

    private static void AppendIfNotZero(StringBuilder sb, StatType stat, float amount)
    {
        if (!Mathf.Approximately(amount, 0f)) sb.AppendLine(FormatBonus(new StatBonus { stat = stat, amount = amount }));
    }

    private static string FormatBonus(StatBonus bonus)
    {
        string sign = bonus.amount >= 0 ? "+" : "";
        string value = bonus.stat == StatType.BlockDamageReduction
            ? (bonus.amount * 100f).ToString("0.#") + "%"
            : bonus.amount.ToString("0.#");
        return $"{sign}{value} {PlayerStats.GetDisplayName(bonus.stat)}";
    }
}
