using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerParryLogic : MonoBehaviour, IDamageable
{
    private static readonly int CanExitParryHash = Animator.StringToHash("CanExitParry");

    [SerializeField] private float parryStamCost = 15f;
    [SerializeField] private Animator animator;

    private PlayerActionCoordinator playerActionCoordinator;
    private PlayerStaminaManager playerStaminaManager;
    private IDamageable nextDamageable;

    public UnityEvent OnParryStart;
    public UnityEvent OnParrySuccess;
    public UnityEvent OnParryEnd;

    private bool isParryWindowActive;
    private bool isParryBusy;
    private bool hasCoordinatorLock;
    private int activeParryStates;

    private void Awake()
    {
        playerActionCoordinator = GetComponent<PlayerActionCoordinator>();
        playerStaminaManager = GetComponent<PlayerStaminaManager>();
        if (animator == null) animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();

        PlayerBlockLogic blockLogic = GetComponent<PlayerBlockLogic>();
        nextDamageable = blockLogic == null ? GetComponent<HealthHandler>() : blockLogic;
    }

    private void OnDisable()
    {
        activeParryStates = 0;
        EndParry();
    }

    public void OnParryInput(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        StartParry();
    }

    private void StartParry()
    {
        if (isParryBusy || playerActionCoordinator == null) return;
        if (!playerActionCoordinator.CanPerformAction(PlayerAbility.Parry)) return;
        if (playerStaminaManager == null || !playerStaminaManager.HasEnoughStamina(parryStamCost)) return;

        playerStaminaManager.UseStamina(parryStamCost);

        isParryBusy = true;
        isParryWindowActive = false;
        hasCoordinatorLock = true;

        if (animator != null) animator.SetBool(CanExitParryHash, false);
        playerActionCoordinator.BeginAction(PlayerActionType.Parry, PlayerAbility.None);

        // Subscribe your Animator Trigger ("Parry") to OnParryStart in the Inspector
        OnParryStart?.Invoke();
    }

    // Call from an Animation Event on the exact frame the deflection window opens
    public void OpenParryWindow()
    {
        if (!isParryBusy) return;
        isParryWindowActive = true;
    }

    // Call from an Animation Event on the exact frame the deflection window closes
    public void CloseParryWindow()
    {
        isParryWindowActive = false;
    }

    public void TakeDmg(float dmg)
    {
        if (isParryWindowActive)
        {
            isParryWindowActive = false;

            // Unlock gameplay actions & allow Animator transitions to interrupt the recovery frames
            isParryBusy = false;
            if (animator != null) animator.SetBool(CanExitParryHash, true);
            ReleaseCoordinatorLock();

            OnParrySuccess?.Invoke();
            return;
        }

        if (nextDamageable as Object != null)
        {
            nextDamageable.TakeDmg(dmg);
        }
    }

    // Called by ParryStateBehaviour when entering the Parry Animator state
    public void NotifyParryStateEntered()
    {
        activeParryStates++;
    }

    // Called by ParryStateBehaviour when exiting the Parry Animator state (natural end OR early interrupt)
    public void NotifyParryStateExited()
    {
        activeParryStates = Mathf.Max(0, activeParryStates - 1);
        if (activeParryStates == 0)
        {
            EndParry();
        }
    }

    public void EndParry()
    {
        isParryWindowActive = false;

        if (animator != null) animator.SetBool(CanExitParryHash, false);
        ReleaseCoordinatorLock();

        if (!isParryBusy && activeParryStates == 0)
        {
            OnParryEnd?.Invoke();
            return;
        }

        isParryBusy = false;
        OnParryEnd?.Invoke();
    }

    private void ReleaseCoordinatorLock()
    {
        if (!hasCoordinatorLock) return;
        hasCoordinatorLock = false;
        if (playerActionCoordinator != null)
        {
            playerActionCoordinator.EndAction(PlayerActionType.Parry);
        }
    }
}