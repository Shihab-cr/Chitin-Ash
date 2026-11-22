using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Already formatted text for the skill details box (the controller builds it)
public struct SkillDetailsViewData
{
    public Sprite icon;
    public string name;
    public string description;
    public string tier;          // "Locked" / "Tier 1 / 3" / "Fully Upgraded"
    public string requirements;  // multi-line, colored met / not met
    public string bonuses;       // what the next tier gives
    public string currentBonuses;// what the player already has from this skill
    public string unlockButtonLabel;
    public bool showUnlockButton;
    public bool canUnlock;
}

/// <summary>
/// MVC View for the Skills tab: a grid of skill icons + a details box for the selected skill.
/// Slots: either assign a slot prefab (spawned automatically) OR place SkillSlotViews by hand inside the container.
/// </summary>
public class SkillsPanelView : MonoBehaviour
{
    [Header("Currency")]
    [SerializeField] private TMP_Text redAphidsText;

    [Header("Skill Grid")]
    [SerializeField] private Transform slotContainer;
    [Tooltip("Optional. Empty = use the SkillSlotViews already placed in the container.")]
    [SerializeField] private SkillSlotView slotPrefab;

    [Header("Details Box (shown when an icon is pressed)")]
    [SerializeField] private GameObject detailsRoot;
    [SerializeField] private Image detailIcon;
    [SerializeField] private TMP_Text detailNameText;
    [SerializeField] private TMP_Text detailDescriptionText;
    [SerializeField] private TMP_Text detailTierText;
    [SerializeField] private TMP_Text requirementsText;
    [SerializeField] private TMP_Text nextBonusesText;
    [SerializeField] private TMP_Text currentBonusesText;
    [SerializeField] private Button unlockButton;
    [SerializeField] private TMP_Text unlockButtonText;

    [Tooltip("Optional: shown when no skill is selected")]
    [SerializeField] private GameObject noSelectionHint;

    public event Action<int> OnSkillSlotClicked;
    public event Action OnUnlockClicked;

    private readonly List<SkillSlotView> slots = new List<SkillSlotView>();

    private void Awake()
    {
        if (unlockButton != null) unlockButton.onClick.AddListener(() => OnUnlockClicked?.Invoke());
    }

    // Makes sure there are exactly "count" slots and returns them in order
    public IReadOnlyList<SkillSlotView> GetSlots(int count)
    {
        if (slots.Count == 0 && slotContainer != null)
        {
            // Pick up hand-placed slots
            foreach (SkillSlotView existing in slotContainer.GetComponentsInChildren<SkillSlotView>(true))
            {
                Register(existing);
            }
        }

        while (slots.Count < count && slotPrefab != null && slotContainer != null)
        {
            Register(Instantiate(slotPrefab, slotContainer));
        }

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].gameObject.SetActive(i < count);
        }

        if (slots.Count < count)
        {
            Debug.LogWarning($"[SkillsPanelView] {count} skills but only {slots.Count} slots. Assign a slot prefab or add more slots.", this);
        }

        return slots;
    }

    public void SetRedAphids(string text)
    {
        if (redAphidsText != null) redAphidsText.text = text;
    }

    public void SetSelectedSlot(int index)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].SetSelected(i == index);
        }
    }

    public void ShowDetails(SkillDetailsViewData data)
    {
        if (detailsRoot != null) detailsRoot.SetActive(true);
        if (noSelectionHint != null) noSelectionHint.SetActive(false);

        if (detailIcon != null)
        {
            detailIcon.sprite = data.icon;
            detailIcon.enabled = data.icon != null;
        }
        SetText(detailNameText, data.name);
        SetText(detailDescriptionText, data.description);
        SetText(detailTierText, data.tier);
        SetText(requirementsText, data.requirements);
        SetText(nextBonusesText, data.bonuses);
        SetText(currentBonusesText, data.currentBonuses);
        SetText(unlockButtonText, data.unlockButtonLabel);

        if (unlockButton != null)
        {
            unlockButton.gameObject.SetActive(data.showUnlockButton);
            unlockButton.interactable = data.canUnlock;
        }
    }

    public void HideDetails()
    {
        if (detailsRoot != null) detailsRoot.SetActive(false);
        if (noSelectionHint != null) noSelectionHint.SetActive(true);
    }

    private void Register(SkillSlotView slot)
    {
        int index = slots.Count;
        slots.Add(slot);
        slot.OnClicked += _ => OnSkillSlotClicked?.Invoke(index);
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null) target.text = value;
    }
}
