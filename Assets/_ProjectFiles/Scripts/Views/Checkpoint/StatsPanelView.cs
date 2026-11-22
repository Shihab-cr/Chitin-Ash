using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Already formatted text for the Stats tab (the controller builds it)
public struct StatsViewData
{
    public string level;
    public string scraps;
    public string levelUpCost;
    public string health;
    public string stamina;
    public string damage;
    public bool canLevelUp;
    public bool isMaxLevel;
}

/// <summary>
/// MVC View for the Stats tab. Zero logic, only PLACEHOLDER references for the designer.
/// </summary>
public class StatsPanelView : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text scrapsText;
    [SerializeField] private TMP_Text levelUpCostText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text staminaText;
    [SerializeField] private TMP_Text damageText;

    [Header("Level Up")]
    [SerializeField] private Button levelUpButton;
    [Tooltip("Optional: shown instead of the button when the max level is reached")]
    [SerializeField] private GameObject maxLevelIndicator;

    public event Action OnLevelUpClicked;

    private void Awake()
    {
        if (levelUpButton != null) levelUpButton.onClick.AddListener(() => OnLevelUpClicked?.Invoke());
    }

    public void Render(StatsViewData data)
    {
        SetText(levelText, data.level);
        SetText(scrapsText, data.scraps);
        SetText(levelUpCostText, data.levelUpCost);
        SetText(healthText, data.health);
        SetText(staminaText, data.stamina);
        SetText(damageText, data.damage);

        if (levelUpButton != null)
        {
            levelUpButton.interactable = data.canLevelUp;
            levelUpButton.gameObject.SetActive(!data.isMaxLevel || maxLevelIndicator == null);
        }
        if (maxLevelIndicator != null) maxLevelIndicator.SetActive(data.isMaxLevel);
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null) target.text = value;
    }
}
