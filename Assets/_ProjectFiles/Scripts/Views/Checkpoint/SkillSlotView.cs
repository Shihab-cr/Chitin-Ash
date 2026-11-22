using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum SkillSlotState
{
    Locked,       // requirements not met
    CanUnlock,    // requirements met, not bought yet (or next upgrade affordable)
    Unlocked,     // owned, more upgrades exist
    FullyUpgraded // owned, all tiers bought
}

/// <summary>
/// One skill icon in the Skills tab grid. Make a prefab with this script (or place them by hand
/// inside the slot container). All references are PLACEHOLDERS, empty ones are ignored.
/// </summary>
public class SkillSlotView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [Tooltip("e.g. '1/3'")]
    [SerializeField] private TMP_Text tierText;

    [Header("State objects (turned on/off by state)")]
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private GameObject canUnlockIndicator;
    [SerializeField] private GameObject fullyUpgradedIndicator;
    [SerializeField] private GameObject selectedHighlight;

    public event Action<SkillSlotView> OnClicked;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(() => OnClicked?.Invoke(this));
    }

    public void Setup(Sprite icon, string skillName, int unlockedTiers, int maxTiers, SkillSlotState state)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }
        if (nameText != null) nameText.text = skillName;
        if (tierText != null) tierText.text = $"{unlockedTiers}/{maxTiers}";

        if (lockedOverlay != null) lockedOverlay.SetActive(state == SkillSlotState.Locked);
        if (canUnlockIndicator != null) canUnlockIndicator.SetActive(state == SkillSlotState.CanUnlock);
        if (fullyUpgradedIndicator != null) fullyUpgradedIndicator.SetActive(state == SkillSlotState.FullyUpgraded);
    }

    public void SetSelected(bool selected)
    {
        if (selectedHighlight != null) selectedHighlight.SetActive(selected);
    }
}
