using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// EDITOR TOOL: Tools > Chitin & Ash > Build Checkpoint Menu (Placeholder)
/// Builds a plain grey placeholder version of the checkpoint menu in the open scene,
/// with every View / Controller field already connected.
/// The designer can restyle it, or replace any piece and drag the new one into the same field.
/// </summary>
public static class CheckpointUIBuilder
{
    private static readonly Color PanelColor = new Color(0.08f, 0.08f, 0.1f, 0.92f);
    private static readonly Color BoxColor = new Color(1f, 1f, 1f, 0.06f);
    private static readonly Color ButtonColor = new Color(0.25f, 0.27f, 0.32f, 1f);
    private static readonly Color HighlightColor = new Color(0.95f, 0.75f, 0.2f, 0.35f);

    [MenuItem("Tools/Chitin & Ash/Build Checkpoint Menu (Placeholder)")]
    public static void Build()
    {
        if (Object.FindAnyObjectByType<CheckpointMenuController>(FindObjectsInactive.Include) != null)
        {
            EditorUtility.DisplayDialog("Checkpoint Menu", "This scene already has a CheckpointMenuController.", "OK");
            return;
        }

        EnsureEventSystem();

        // ---------- Canvas + controller ----------
        GameObject canvasGO = new GameObject("CheckpointCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGO, "Build Checkpoint Menu");
        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        CheckpointMenuController menuController = canvasGO.AddComponent<CheckpointMenuController>();

        // ---------- Menu root ----------
        RectTransform menu = Panel(canvasGO.transform, "CheckpointMenu", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, PanelColor);
        CheckpointMenuView menuView = menu.gameObject.AddComponent<CheckpointMenuView>();

        TMP_Text checkpointName = MakeText(menu, "CheckpointName", "Checkpoint", 48, TextAlignmentOptions.Center,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0, -110), new Vector2(0, -30));

        // Left column: save buttons
        RectTransform left = Rect(menu, "SaveButtons", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(60, 120), new Vector2(460, -160));
        VerticalLayoutGroup leftLayout = left.gameObject.AddComponent<VerticalLayoutGroup>();
        leftLayout.spacing = 20; leftLayout.childControlHeight = false; leftLayout.childForceExpandHeight = false;
        leftLayout.childControlWidth = true; leftLayout.childForceExpandWidth = true;
        Button saveButton = LayoutButton(left, "SaveGameButton", "Save Game");
        Button saveQuitButton = LayoutButton(left, "SaveAndQuitButton", "Save & Quit");
        Button saveMenuButton = LayoutButton(left, "SaveAndMainMenuButton", "Save & Main Menu");
        Button leaveButton = LayoutButton(left, "LeaveButton", "Leave");

        TMP_Text message = MakeText(menu, "Message", "Game Saved", 32, TextAlignmentOptions.Center,
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0, 30), new Vector2(0, 90));
        message.color = new Color(0.6f, 1f, 0.6f);

        // Tabs row
        RectTransform tabs = Rect(menu, "Tabs", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(520, -220), new Vector2(-60, -140));
        HorizontalLayoutGroup tabLayout = tabs.gameObject.AddComponent<HorizontalLayoutGroup>();
        tabLayout.spacing = 20; tabLayout.childControlWidth = true; tabLayout.childForceExpandWidth = true;
        tabLayout.childControlHeight = true; tabLayout.childForceExpandHeight = true;
        Button statsTab = TabButton(tabs, "StatsTabButton", "Stats", out GameObject statsHighlight);
        Button skillsTab = TabButton(tabs, "SkillsTabButton", "Skills", out GameObject skillsHighlight);
        Button questsTab = TabButton(tabs, "QuestsTabButton", "Quests", out GameObject questsHighlight);

        // Content area for the three panels
        Vector2 contentMin = new Vector2(520, 120);
        Vector2 contentMax = new Vector2(-60, -240);

        // ---------- Stats panel ----------
        RectTransform stats = Panel(menu, "StatsPanel", Vector2.zero, Vector2.one, contentMin, contentMax, BoxColor);
        StatsPanelView statsView = stats.gameObject.AddComponent<StatsPanelView>();
        StatsPanelController statsController = stats.gameObject.AddComponent<StatsPanelController>();
        RectTransform statsList = Rect(stats, "StatsList", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(40, 140), new Vector2(-40, -30));
        VerticalLayoutGroup statsLayout = statsList.gameObject.AddComponent<VerticalLayoutGroup>();
        statsLayout.spacing = 10; statsLayout.childControlHeight = true; statsLayout.childForceExpandHeight = false;
        statsLayout.childControlWidth = true; statsLayout.childForceExpandWidth = true;
        TMP_Text levelText = LayoutText(statsList, "LevelText", "Level 1", 44);
        TMP_Text scrapsText = LayoutText(statsList, "ScrapsText", "Scraps: 0", 32);
        TMP_Text costText = LayoutText(statsList, "LevelUpCostText", "Level Up: 100 Scraps", 32);
        TMP_Text healthText = LayoutText(statsList, "HealthText", "Health: 100", 32);
        TMP_Text staminaText = LayoutText(statsList, "StaminaText", "Stamina: 100", 32);
        TMP_Text damageText = LayoutText(statsList, "DamageText", "Damage: 10", 32);
        Button levelUpButton = MakeButton(stats, "LevelUpButton", "Level Up", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-180, 30), new Vector2(180, 110));
        TMP_Text maxLevel = MakeText(stats, "MaxLevelIndicator", "MAX LEVEL", 36, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-180, 30), new Vector2(180, 110));
        maxLevel.gameObject.SetActive(false);

        SetRefs(statsView,
            ("levelText", levelText), ("scrapsText", scrapsText), ("levelUpCostText", costText),
            ("healthText", healthText), ("staminaText", staminaText), ("damageText", damageText),
            ("levelUpButton", levelUpButton), ("maxLevelIndicator", maxLevel.gameObject));
        SetRefs(statsController, ("view", statsView));

        // ---------- Skills panel ----------
        RectTransform skills = Panel(menu, "SkillsPanel", Vector2.zero, Vector2.one, contentMin, contentMax, BoxColor);
        SkillsPanelView skillsView = skills.gameObject.AddComponent<SkillsPanelView>();
        SkillsPanelController skillsController = skills.gameObject.AddComponent<SkillsPanelController>();
        TMP_Text aphidsText = MakeText(skills, "RedAphidsText", "Red Aphids: 0", 32, TextAlignmentOptions.Left,
            new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(30, -70), new Vector2(0, -15));

        RectTransform slotContainer = Rect(skills, "SlotContainer", new Vector2(0f, 0f), new Vector2(0.45f, 1f), new Vector2(30, 30), new Vector2(-10, -90));
        GridLayoutGroup grid = slotContainer.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(150, 170); grid.spacing = new Vector2(16, 16);

        SkillSlotView slotTemplate = BuildSkillSlotTemplate(skills);

        RectTransform details = Panel(skills, "DetailsBox", new Vector2(0.45f, 0f), new Vector2(1f, 1f), new Vector2(10, 30), new Vector2(-30, -30), BoxColor);
        Image detailIcon = Img(details, "Icon", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20, -140), new Vector2(140, -20), new Color(1, 1, 1, 0.2f));
        TMP_Text detailName = MakeText(details, "Name", "Skill Name", 36, TextAlignmentOptions.Left, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(160, -70), new Vector2(-20, -20));
        TMP_Text detailTier = MakeText(details, "Tier", "Locked", 26, TextAlignmentOptions.Left, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(160, -120), new Vector2(-20, -75));
        TMP_Text detailDesc = MakeText(details, "Description", "Description", 24, TextAlignmentOptions.TopLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20, -230), new Vector2(-20, -150));
        TMP_Text requirements = MakeText(details, "Requirements", "Requirements", 24, TextAlignmentOptions.TopLeft, new Vector2(0f, 0f), new Vector2(0.5f, 1f), new Vector2(20, 120), new Vector2(-10, -240));
        TMP_Text nextBonuses = MakeText(details, "NextBonuses", "Gives", 24, TextAlignmentOptions.TopLeft, new Vector2(0.5f, 0.35f), new Vector2(1f, 1f), new Vector2(10, 0), new Vector2(-20, -240));
        TMP_Text currentBonuses = MakeText(details, "CurrentBonuses", "Owned", 24, TextAlignmentOptions.TopLeft, new Vector2(0.5f, 0f), new Vector2(1f, 0.35f), new Vector2(10, 120), new Vector2(-20, 0));
        Button unlockButton = MakeButton(details, "UnlockButton", "Unlock", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-150, 20), new Vector2(150, 95));
        TMP_Text noSelection = MakeText(skills, "NoSelectionHint", "Press a skill icon", 30, TextAlignmentOptions.Center,
            new Vector2(0.45f, 0f), new Vector2(1f, 1f), new Vector2(10, 30), new Vector2(-30, -30));

        SetRefs(skillsView,
            ("redAphidsText", aphidsText), ("slotContainer", slotContainer), ("slotPrefab", slotTemplate),
            ("detailsRoot", details.gameObject), ("detailIcon", detailIcon), ("detailNameText", detailName),
            ("detailDescriptionText", detailDesc), ("detailTierText", detailTier), ("requirementsText", requirements),
            ("nextBonusesText", nextBonuses), ("currentBonusesText", currentBonuses), ("unlockButton", unlockButton),
            ("unlockButtonText", unlockButton.GetComponentInChildren<TMP_Text>()), ("noSelectionHint", noSelection.gameObject));
        SetRefs(skillsController, ("view", skillsView));

        // ---------- Quests panel ----------
        RectTransform quests = Panel(menu, "QuestsPanel", Vector2.zero, Vector2.one, contentMin, contentMax, BoxColor);
        QuestsPanelView questsView = quests.gameObject.AddComponent<QuestsPanelView>();
        QuestsPanelController questsController = quests.gameObject.AddComponent<QuestsPanelController>();
        RectTransform entryContainer = Rect(quests, "EntryContainer", Vector2.zero, Vector2.one, new Vector2(30, 30), new Vector2(-30, -30));
        VerticalLayoutGroup questLayout = entryContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        questLayout.spacing = 16; questLayout.childControlHeight = false; questLayout.childForceExpandHeight = false;
        questLayout.childControlWidth = true; questLayout.childForceExpandWidth = true;
        QuestEntryView entryTemplate = BuildQuestEntryTemplate(quests);
        TMP_Text emptyState = MakeText(quests, "EmptyState", "No goals yet", 32, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        SetRefs(questsView, ("entryContainer", entryContainer), ("entryPrefab", entryTemplate), ("emptyState", emptyState.gameObject));
        SetRefs(questsController, ("view", questsView));

        // ---------- Connect the menu ----------
        SetRefs(menuView,
            ("checkpointNameText", checkpointName),
            ("saveGameButton", saveButton), ("saveAndQuitButton", saveQuitButton), ("saveAndMainMenuButton", saveMenuButton),
            ("statsTabButton", statsTab), ("skillsTabButton", skillsTab), ("questsTabButton", questsTab),
            ("statsPanel", stats.gameObject), ("skillsPanel", skills.gameObject), ("questsPanel", quests.gameObject),
            ("statsTabHighlight", statsHighlight), ("skillsTabHighlight", skillsHighlight), ("questsTabHighlight", questsHighlight),
            ("leaveButton", leaveButton), ("messageText", message));
        SetRefs(menuController,
            ("menuView", menuView), ("statsController", statsController),
            ("skillsController", skillsController), ("questsController", questsController));

        menu.gameObject.SetActive(false); // hidden until the player rests

        EditorSceneManager.MarkSceneDirty(canvasGO.scene);
        Selection.activeGameObject = canvasGO;
        Debug.Log("[CheckpointUIBuilder] Placeholder checkpoint menu built and connected. Save the scene (Ctrl+S).");
    }

    // ---------------- Templates (inactive, outside their containers) ----------------

    private static SkillSlotView BuildSkillSlotTemplate(Transform parent)
    {
        RectTransform slot = Panel(parent, "SkillSlotTemplate", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0, 0), new Vector2(150, 170), ButtonColor);
        Button button = slot.gameObject.AddComponent<Button>();
        button.targetGraphic = slot.GetComponent<Image>();
        Image icon = Img(slot, "Icon", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-50, -110), new Vector2(50, -10), new Color(1, 1, 1, 0.25f));
        TMP_Text nameText = MakeText(slot, "Name", "Skill", 20, TextAlignmentOptions.Center, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(4, 28), new Vector2(-4, 58));
        TMP_Text tierText = MakeText(slot, "Tier", "0/1", 20, TextAlignmentOptions.Center, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(4, 2), new Vector2(-4, 28));
        GameObject locked = Img(slot, "LockedOverlay", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.55f)).gameObject;
        locked.GetComponent<Image>().raycastTarget = false;
        GameObject canUnlock = Img(slot, "CanUnlockIndicator", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30, -30), new Vector2(-6, -6), new Color(0.3f, 1f, 0.3f)).gameObject;
        GameObject maxed = Img(slot, "FullyUpgradedIndicator", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30, -30), new Vector2(-6, -6), new Color(1f, 0.8f, 0.2f)).gameObject;
        GameObject selected = Img(slot, "SelectedHighlight", Vector2.zero, Vector2.one, new Vector2(-6, -6), new Vector2(6, 6), HighlightColor).gameObject;
        selected.GetComponent<Image>().raycastTarget = false;
        selected.transform.SetAsFirstSibling();

        SkillSlotView view = slot.gameObject.AddComponent<SkillSlotView>();
        SetRefs(view, ("button", button), ("iconImage", icon), ("nameText", nameText), ("tierText", tierText),
            ("lockedOverlay", locked), ("canUnlockIndicator", canUnlock), ("fullyUpgradedIndicator", maxed), ("selectedHighlight", selected));
        slot.gameObject.SetActive(false);
        return view;
    }

    private static QuestEntryView BuildQuestEntryTemplate(Transform parent)
    {
        RectTransform entry = Panel(parent, "QuestEntryTemplate", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0, -200), new Vector2(0, 0), ButtonColor);
        entry.sizeDelta = new Vector2(entry.sizeDelta.x, 200);
        TMP_Text title = MakeText(entry, "Title", "Quest", 32, TextAlignmentOptions.Left, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20, -55), new Vector2(-80, -10));
        TMP_Text description = MakeText(entry, "Description", "Description", 22, TextAlignmentOptions.Left, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20, -90), new Vector2(-20, -55));
        TMP_Text objectives = MakeText(entry, "Objectives", "- Objective 0/1", 22, TextAlignmentOptions.TopLeft, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(20, 35), new Vector2(-20, -95));
        Slider bar = BuildProgressBar(entry);
        GameObject done = MakeText(entry, "CompletedMarker", "DONE", 26, TextAlignmentOptions.Right, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-120, -55), new Vector2(-20, -10)).gameObject;
        done.GetComponent<TMP_Text>().color = new Color(0.5f, 1f, 0.5f);

        QuestEntryView view = entry.gameObject.AddComponent<QuestEntryView>();
        SetRefs(view, ("titleText", title), ("descriptionText", description), ("objectivesText", objectives),
            ("progressBar", bar), ("completedMarker", done));
        entry.gameObject.SetActive(false);
        return view;
    }

    private static Slider BuildProgressBar(Transform parent)
    {
        RectTransform root = Rect(parent, "ProgressBar", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(20, 10), new Vector2(-20, 26));
        Img(root, "Background", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.5f));
        RectTransform fillArea = Rect(root, "Fill Area", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image fill = Img(fillArea, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.4f, 0.85f, 0.4f));
        Slider slider = root.gameObject.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
        return slider;
    }

    // ---------------- Small UI helpers ----------------

    private static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>(FindObjectsInactive.Include) != null) return;

        GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
    }

    private static RectTransform Rect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        return rt;
    }

    private static RectTransform Panel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        RectTransform rt = Rect(parent, name, anchorMin, anchorMax, offsetMin, offsetMax);
        rt.gameObject.AddComponent<Image>().color = color;
        return rt;
    }

    private static Image Img(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        RectTransform rt = Rect(parent, name, anchorMin, anchorMax, offsetMin, offsetMax);
        Image image = rt.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static TMP_Text MakeText(Transform parent, string name, string text, float size, TextAlignmentOptions align,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        RectTransform rt = Rect(parent, name, anchorMin, anchorMax, offsetMin, offsetMax);
        TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static TMP_Text LayoutText(Transform parent, string name, string text, float size)
    {
        TMP_Text tmp = MakeText(parent, name, text, size, TextAlignmentOptions.Left, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        LayoutElement le = tmp.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = size * 1.4f;
        return tmp;
    }

    private static Button MakeButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        RectTransform rt = Panel(parent, name, anchorMin, anchorMax, offsetMin, offsetMax, ButtonColor);
        Button button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = rt.GetComponent<Image>();
        MakeText(rt, "Label", label, 30, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return button;
    }

    private static Button LayoutButton(Transform parent, string name, string label)
    {
        Button button = MakeButton(parent, name, label, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        RectTransform rt = (RectTransform)button.transform;
        rt.sizeDelta = new Vector2(0, 80);
        button.gameObject.AddComponent<LayoutElement>().preferredHeight = 80;
        return button;
    }

    private static Button TabButton(Transform parent, string name, string label, out GameObject highlight)
    {
        Button button = MakeButton(parent, name, label, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        highlight = Img(button.transform, "SelectedHighlight", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0, 0), new Vector2(0, 8), new Color(0.95f, 0.75f, 0.2f)).gameObject;
        highlight.GetComponent<Image>().raycastTarget = false;
        highlight.SetActive(false);
        return button;
    }

    // Assigns private [SerializeField] fields the same way dragging in the Inspector does
    private static void SetRefs(Object target, params (string field, Object value)[] refs)
    {
        SerializedObject so = new SerializedObject(target);
        foreach ((string field, Object value) in refs)
        {
            SerializedProperty prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogWarning($"[CheckpointUIBuilder] Field '{field}' not found on {target.GetType().Name}");
                continue;
            }
            prop.objectReferenceValue = value;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
