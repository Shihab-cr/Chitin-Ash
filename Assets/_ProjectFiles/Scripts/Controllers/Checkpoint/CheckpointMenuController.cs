using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// MVC Controller of the menu that opens when the player rests at a Checkpoint.
///   Save Game            -> saves all progress
///   Save & Quit          -> saves, then quits the game
///   Save & Main Menu     -> saves, then returns to the main menu
///   Stats / Skills / Quests tabs
///
/// Put this on an object that is ALWAYS active (e.g. the Canvas), not on the panel it hides
/// (same rule as PauseMenuController).
/// </summary>
public class CheckpointMenuController : MonoBehaviour
{
    // True while the menu is open (other scripts can check it, e.g. PauseMenuController)
    public static bool IsOpen { get; private set; }

    // Use this in PauseMenuController: also true on the frame the menu was closed with Escape,
    // so the same Escape press doesn't open the pause menu
    public static bool BlocksPauseMenu => IsOpen || Time.frameCount == closedOnFrame;
    private static int closedOnFrame = -1;

    [Header("MVC References")]
    [SerializeField] private CheckpointMenuView menuView;
    [SerializeField] private StatsPanelController statsController;
    [SerializeField] private SkillsPanelController skillsController;
    [SerializeField] private QuestsPanelController questsController;

    [Header("Single-Scene / Multi-Scene Main Menu (same as PauseMenuController)")]
    [Tooltip("Assign if the Main Menu is in the same scene. Leave empty if using separate scenes.")]
    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private string mainMenuSceneName = "";

    [Header("Behaviour")]
    [SerializeField] private CheckpointTab defaultTab = CheckpointTab.Stats;
    [SerializeField] private bool closeWithEscape = true;
    [SerializeField] private bool showCursorWhileOpen = true;
    [SerializeField] private string savedMessage = "Game Saved";
    [SerializeField] private float messageDuration = 2f;

    [Header("Hooks (disable player input, play sounds...)")]
    public UnityEvent OnMenuOpened;
    public UnityEvent OnMenuClosed;

    private ProgressionModel model;
    private CheckpointController currentCheckpoint;
    private Coroutine messageRoutine;
    private CursorLockMode previousLockMode;
    private bool previousCursorVisible;

    private void Start()
    {
        IsOpen = false;
        model = ProgressionModel.Instance;

        if (model == null)
        {
            Debug.LogError("[CheckpointMenuController] No ProgressionController in the scene (it creates ProgressionModel).", this);
        }
        else
        {
            if (statsController != null) statsController.Initialize(model);
            if (skillsController != null) skillsController.Initialize(model);
            if (questsController != null) questsController.Initialize(model);
        }

        menuView.OnSaveGameClicked += HandleSaveGame;
        menuView.OnSaveAndQuitClicked += HandleSaveAndQuit;
        menuView.OnSaveAndMainMenuClicked += HandleSaveAndMainMenu;
        menuView.OnTabClicked += ShowTab;
        menuView.OnLeaveClicked += Close;

        menuView.HideMessage();
        menuView.Show(false);
    }

    private void Update()
    {
        if (!IsOpen || !closeWithEscape) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    // ---------------- Open / Close ----------------

    public void Open(CheckpointController checkpoint)
    {
        if (IsOpen) return;

        currentCheckpoint = checkpoint;
        IsOpen = true;
        Time.timeScale = 0f; // Section 12: freeze gameplay while a menu is open

        if (showCursorWhileOpen)
        {
            previousLockMode = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        menuView.SetCheckpointName(checkpoint != null ? checkpoint.DisplayName : "");
        menuView.HideMessage();
        menuView.Show(true);

        if (skillsController != null) skillsController.ClearSelection();
        ShowTab(defaultTab);

        OnMenuOpened?.Invoke();
    }

    public void Close()
    {
        if (!IsOpen) return;

        IsOpen = false;
        closedOnFrame = Time.frameCount;
        Time.timeScale = 1f; // Section 12: ALWAYS restore time

        if (showCursorWhileOpen)
        {
            Cursor.lockState = previousLockMode;
            Cursor.visible = previousCursorVisible;
        }

        StopMessage();
        menuView.Show(false);

        if (currentCheckpoint != null) currentCheckpoint.OnMenuClosed();
        currentCheckpoint = null;

        OnMenuClosed?.Invoke();
    }

    // ---------------- Tabs ----------------

    public void ShowTab(CheckpointTab tab)
    {
        menuView.ShowTab(tab);

        switch (tab)
        {
            case CheckpointTab.Stats: if (statsController != null) statsController.Refresh(); break;
            case CheckpointTab.Skills: if (skillsController != null) skillsController.Refresh(); break;
            case CheckpointTab.Quests: if (questsController != null) questsController.Refresh(); break;
        }
    }

    // ---------------- Save buttons ----------------

    private void HandleSaveGame()
    {
        if (!SaveAll()) return;

        StopMessage();
        messageRoutine = StartCoroutine(ShowMessageRoutine(savedMessage));
    }

    private void HandleSaveAndQuit()
    {
        SaveAll();

        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void HandleSaveAndMainMenu()
    {
        SaveAll();
        Close();

        if (mainMenuController != null)
        {
            // Single-scene mode: reopen the Main Menu panel (it freezes time itself)
            mainMenuController.OpenMainMenu();
        }
        else if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            // Multi-scene mode
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("[CheckpointMenuController] Assign a MainMenuController or a main menu scene name.", this);
        }
    }

    private bool SaveAll()
    {
        if (model == null) return false;

        if (ProgressionController.Instance == null) return false;
        ProgressionController.Instance.SaveAll();
        return true;
    }

    // ---------------- Feedback message ----------------

    // Realtime because Time.timeScale is 0 while the menu is open
    private IEnumerator ShowMessageRoutine(string message)
    {
        menuView.ShowMessage(message);
        yield return new WaitForSecondsRealtime(messageDuration);
        menuView.HideMessage();
        messageRoutine = null;
    }

    private void StopMessage()
    {
        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
            messageRoutine = null;
        }
        menuView.HideMessage();
    }

    private void OnDestroy()
    {
        if (IsOpen)
        {
            IsOpen = false;
            Time.timeScale = 1f;
        }

        if (menuView == null) return;
        menuView.OnSaveGameClicked -= HandleSaveGame;
        menuView.OnSaveAndQuitClicked -= HandleSaveAndQuit;
        menuView.OnSaveAndMainMenuClicked -= HandleSaveAndMainMenu;
        menuView.OnTabClicked -= ShowTab;
        menuView.OnLeaveClicked -= Close;
    }
}
