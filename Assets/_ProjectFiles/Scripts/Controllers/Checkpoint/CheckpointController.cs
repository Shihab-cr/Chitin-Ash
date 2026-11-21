using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// MVC Controller of a rest point in the world. When the Player is inside the trigger and presses Interact:
///   1. heals the player (checkpoint healing)
///   2. sets this as the respawn checkpoint (team's GameProgressModel)
///   3. fires OnRested (hook enemy reset / VFX / sound here)
///   4. opens the checkpoint menu
///
/// Setup: any Collider on this object (it is forced to "Is Trigger"), Player must have the "Player" tag.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CheckpointController : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Unique id saved in the save file (e.g. CP1_HollowLog)")]
    [SerializeField] private string checkpointId;
    [Tooltip("Shown at the top of the checkpoint menu")]
    [SerializeField] private string displayName = "Checkpoint";
    [Tooltip("Where the player respawns. Uses this object's position if empty.")]
    [SerializeField] private Transform respawnPoint;

    [Header("Menu (found automatically if empty)")]
    [SerializeField] private CheckpointMenuController checkpointMenu;

    [Header("Input")]
    [Tooltip("Optional: the Interact action from the Input Actions asset")]
    [SerializeField] private InputActionReference interactAction;
    [Tooltip("Used when no Interact action is assigned")]
    [SerializeField] private Key fallbackKey = Key.E;

    [Header("PLACEHOLDER: 'Press E to rest' prompt object (shown while the player is in range)")]
    [SerializeField] private GameObject interactPrompt;

    [Header("Rules")]
    [SerializeField] private bool healOnRest = true;

    [Header("Hooks (enemy reset, VFX, sound...)")]
    public UnityEvent OnRested;

    public string CheckpointId => checkpointId;
    public string DisplayName => displayName;

    private HealthHandler playerHealthInRange;
    private bool playerInRange;

    private void Awake()
    {
        if (checkpointMenu == null) checkpointMenu = FindAnyObjectByType<CheckpointMenuController>(FindObjectsInactive.Include);
        SetPromptVisible(false);
    }

    private void OnEnable()
    {
        if (interactAction != null && !interactAction.action.enabled)
        {
            interactAction.action.Enable();
        }
    }

    private void Update()
    {
        if (!playerInRange || CheckpointMenuController.IsOpen) return;
        if (Time.timeScale == 0f) return; // paused / main menu / game over

        if (WasInteractPressed())
        {
            Rest();
        }
    }

    public void Rest()
    {
        if (healOnRest && playerHealthInRange != null)
        {
            playerHealthInRange.ResetHealth();
        }

        Vector3 respawn = respawnPoint != null ? respawnPoint.position : transform.position;
        GameProgressModel.Instance.SetCheckpoint(checkpointId, respawn);

        ProgressionEvents.ReportCheckpointRested(checkpointId);
        OnRested?.Invoke();

        SetPromptVisible(false);

        if (checkpointMenu != null)
        {
            checkpointMenu.Open(this);
        }
        else
        {
            Debug.LogWarning("[CheckpointController] No CheckpointMenuController in the scene.", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;

        playerInRange = true;
        playerHealthInRange = other.GetComponentInParent<HealthHandler>();
        SetPromptVisible(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;

        playerInRange = false;
        playerHealthInRange = null;
        SetPromptVisible(false);
    }

    // Hide the prompt again after the menu closes while still standing here
    public void OnMenuClosed()
    {
        SetPromptVisible(playerInRange);
    }

    private bool WasInteractPressed()
    {
        if (interactAction != null)
        {
            return interactAction.action.WasPressedThisFrame();
        }
        return Keyboard.current != null && Keyboard.current[fallbackKey].wasPressedThisFrame;
    }

    private static bool IsPlayer(Collider other)
    {
        if (other.CompareTag("Player")) return true;
        return other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player");
    }

    private void SetPromptVisible(bool visible)
    {
        if (interactPrompt != null) interactPrompt.SetActive(visible);
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(checkpointId)) checkpointId = gameObject.name;

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }
}
