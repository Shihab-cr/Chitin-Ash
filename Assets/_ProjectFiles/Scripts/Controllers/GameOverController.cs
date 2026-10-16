using System;
using UnityEngine;

/// <summary>
/// MVC Controller: Manages GameOverView, saves player data after death,and respans in nearest checkpoint. >>>
/// </summary>
public class GameOverController : MonoBehaviour
{
    [Header("MVC References")]
    [SerializeField] private GameOverView gameOverView;

    [Header("Player Respawn Reference")]
    [Tooltip("Drag the Player GameObject here so Restart can move them to the last checkpoint.")] 
    [SerializeField] private Transform playerTransform;

    [Header("Testing")]
    [Tooltip("Press this key in Play Mode to test triggering the Game Over menu.")]
    [SerializeField] private KeyCode debugKillPlayerKey = KeyCode.K;

    // Event that the Player Health Handler or Enemy Spawner can listen to on respawn
    public event Action OnPlayerRespawned; //action=no input ....event =others can listen to but not make it themselves

    private void Start()
    {
        gameOverView.SetTitle("GAME OVER");
        gameOverView.Show(false);

        gameOverView.OnRestartClicked += HandleRestartClicked;
    }

    private void Update()
    {
        // Debug shortcut so you can test Game Over anytime by pressing K
        if (debugKillPlayerKey != KeyCode.None && Input.GetKeyDown(debugKillPlayerKey))
        {
            if (Time.timeScale > 0f) // Only trigger if we are actively in gameplay
            {
                TriggerGameOver();
            }
        }
    }

    /// <summary>
    /// Call this method from the Player Health Handler when HP reaches 0.
    /// </summary>
    public void TriggerGameOver()
    {
        // Rule: Player's data is saved after death
        GameProgressModel.Instance.SaveCurrentProgress();

        Time.timeScale = 0f;
        gameOverView.Show(true);
    }

    private void HandleRestartClicked()
    {
        // Ensure player's data is saved
        GameProgressModel.Instance.SaveCurrentProgress();

        // Rule: Spawn beside the nearest checkpoint
        if (playerTransform != null)
        {
            Vector3 checkpointPos = GameProgressModel.Instance.CurrentData.lastCheckpointPosition;

            // Briefly disable CharacterController if present so Unity allows teleporting the position
            CharacterController cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerTransform.position = checkpointPos;

            if (cc != null) cc.enabled = true;
        }

        Time.timeScale = 1f;
        gameOverView.Show(false);

        // Notify other systems (like Health Handler) to refill HP
        OnPlayerRespawned?.Invoke();
    }

    private void OnDestroy()
    {
        if (gameOverView != null)
            gameOverView.OnRestartClicked -= HandleRestartClicked;
    }
}