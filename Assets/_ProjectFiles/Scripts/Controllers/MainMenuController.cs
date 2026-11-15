using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MVC Controller: Bridges MainMenuView (UI) and GameProgressModel (Data).
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("MVC References")]
    [SerializeField] private MainMenuView mainMenuView;
    [SerializeField] private OptionsController optionsController;

    [Header("Scene Loading")]
    [Tooltip("Leave empty if using a single scene with UI panels. Type scene name if loading a separate level scene.")]
    [SerializeField] private string gameplaySceneName = ""; //DO NOT TOUCH ,if we didn't use other scenes plz delete it Mr reader or tell Leader Shihab to delete it // //only used if we decided later to make the Main menu an independent scene

    private GameProgressModel progressModel;

    private void Start()
    {
        progressModel = GameProgressModel.Instance;

        //all View button click events
        mainMenuView.OnNewGameClicked += HandleNewGameClicked;
        mainMenuView.OnConfirmOverwriteClicked += HandleConfirmNewGame;
        mainMenuView.OnCancelOverwriteClicked += () => mainMenuView.ShowOverwriteWarning(false);
        mainMenuView.OnLoadGameClicked += HandleLoadGameClicked;
        mainMenuView.OnOptionsClicked += HandleOptionsClicked;
        mainMenuView.OnCreditsClicked += () => mainMenuView.ShowCredits(true);
        mainMenuView.OnCloseCreditsClicked += () => mainMenuView.ShowCredits(false);
        mainMenuView.OnExitClicked += HandleExitClicked;

        // Initialize the Main Menu state
        OpenMainMenu();
    }

    public void OpenMainMenu()
    {
        // Freeze gameplay in the background
        Time.timeScale = 0f;

        mainMenuView.SetTitle("Chitin & Ash");
        mainMenuView.ShowMainMenu(true);
        mainMenuView.ShowOverwriteWarning(false);
        mainMenuView.ShowCredits(false);


        //Disable Load Game button if there are no saved files
        bool hasSave = progressModel.HasSavedFile();
        mainMenuView.SetLoadButtonInteractable(hasSave);
    }

    private void HandleNewGameClicked()
    {
        //Show warning if player already has data saved
        if (progressModel.HasSavedFile())
        {
            mainMenuView.ShowOverwriteWarning(true);
        }
        else
        {
            HandleConfirmNewGame();
        }
    }

    private void HandleConfirmNewGame()
    {
        //Deletes saved player data (not the settings)
        progressModel.StartNewGame();
        mainMenuView.ShowOverwriteWarning(false);
        EnterGameplay();
    }

    private void HandleLoadGameClicked()
    {
        if (!progressModel.HasSavedFile()) return;

        //Loads old player progress
        progressModel.LoadSavedGame();
        EnterGameplay();
    }

    private void EnterGameplay()
    {
        Time.timeScale = 1f; // Unfreeze time so gameplay can run

        if (!string.IsNullOrEmpty(gameplaySceneName))  //only used if we decided later to make the Main menu an independent scene //DO NOT TOUCH ,if we didn't use other scenes plz delete it Mr reader or tell Leader Shihab to delete it //
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            mainMenuView.ShowMainMenu(false);
        }
    }

    private void HandleOptionsClicked()
    {
        if (optionsController != null)
        {
            mainMenuView.ShowMainMenu(false);
            // Opens Options, and tells it to turn Main Menu back on when "Back" is clicked!
            optionsController.OpenOptions(() => mainMenuView.ShowMainMenu(true));
        }
    }

    private void HandleExitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnDestroy()
    {
        if (mainMenuView == null) return;

        mainMenuView.OnNewGameClicked -= HandleNewGameClicked;
        mainMenuView.OnConfirmOverwriteClicked -= HandleConfirmNewGame;
        mainMenuView.OnLoadGameClicked -= HandleLoadGameClicked;
        mainMenuView.OnOptionsClicked -= HandleOptionsClicked;
        mainMenuView.OnExitClicked -= HandleExitClicked;
    }
}