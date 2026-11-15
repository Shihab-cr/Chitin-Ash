using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
//Basically the controller of Pause with its fns like returning to main menu without saving player progresds etc...
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("MVC References")]
    [SerializeField] private PauseMenuView pauseView;
    [SerializeField] private OptionsController optionsController;

    [Header("Single-Scene / Multi-Scene Navigation")]
    [Tooltip("Assign if Main Menu is in the same scene. Leave empty if using separate scenes.")]
    [SerializeField] private MainMenuController mainMenuController;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private string mainMenuSceneName = "";

    private bool isPaused = false;
    private bool inOptionsSubMenu = false;

    private void Start()
    {
        pauseView.SetTitle("PAUSED");
        pauseView.Show(false);

        pauseView.OnResumeClicked += ResumeGame;
        pauseView.OnOptionsClicked += OpenOptionsFromPause;
        pauseView.OnMainMenuClicked += GoToMainMenuWithoutSaving;
        pauseView.OnExitClicked += ExitGameWithoutSaving;
    }

    private void Update()
    {
        // Do not allow pausing if the player is currently on the Main Menu, Game Over screen, or inside Options
        if (inOptionsSubMenu) return;
        if (mainMenuPanel != null && mainMenuPanel.activeSelf) return;
        if (gameOverPanel != null && gameOverPanel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Rule: Stop time when menu is active
        pauseView.Show(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        inOptionsSubMenu = false;
        Time.timeScale = 1f; // Rule: Resume game -> unfreeze game
        pauseView.Show(false);
    }

    private void OpenOptionsFromPause()
    {
        if (optionsController == null) return;

        // Rule: Options -> show options menu
        inOptionsSubMenu = true;
        pauseView.Show(false);

        // When "Back" is clicked in Options, return to the Pause Menu!
        optionsController.OpenOptions(() =>
        {
            inOptionsSubMenu = false;
            pauseView.Show(true);
        });
    }

    private void GoToMainMenuWithoutSaving()
    {
        // Rule: go to Main Menu (button) -> NO saving player data
        isPaused = false;
        inOptionsSubMenu = false;
        pauseView.Show(false);

        if (mainMenuController != null)
        {
            // Single-scene mode: reopen the Main Menu panel
            mainMenuController.OpenMainMenu();
        }
        else if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            // Multi-scene mode: load the Main Menu scene
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    private void ExitGameWithoutSaving()
    {
        // Rule: exit game -> NO saving
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnDestroy()
    {
        if (pauseView == null) return;

        pauseView.OnResumeClicked -= ResumeGame;
        pauseView.OnOptionsClicked -= OpenOptionsFromPause;
        pauseView.OnMainMenuClicked -= GoToMainMenuWithoutSaving;
        pauseView.OnExitClicked -= ExitGameWithoutSaving;
    }
}