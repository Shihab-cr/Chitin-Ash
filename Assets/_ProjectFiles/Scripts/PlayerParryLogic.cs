using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public interface IParryable
{
    void OnParried(GameObject parrier);
}

public interface IAttackerAwareDamageable
{
    void TakeDmg(float dmg, GameObject attacker);
}

public class PlayerParryLogic : MonoBehaviour, IDamageable, IAttackerAwareDamageable
{
    [SerializeField] private float parryStamCost = 15f;
    [Tooltip("Failsafe duration (in seconds) to unlock player actions if the Parry Animator state does not trigger OnStateExit.")]
    [SerializeField] private float maxParryLockDuration = 1.0f;

    [Header("Parry Counter Settings")]
    [SerializeField] private float parryCounterDamage = 25f;

    private PlayerActionCoordinator playerActionCoordinator;
    private PlayerStaminaManager playerStaminaManager;
    private PlayerAnimatorControllerHandler playerAnimatorHandler;
    private IDamageable nextDamageable;

    public UnityEvent OnParryStart;
    public UnityEvent OnParrySuccess;
    public UnityEvent OnParryEnd;
    public UnityEvent<GameObject> OnParriedAttacker;

    private Coroutine parryFailsafeCoroutine;
    private bool isParryWindowActive;
    private bool isParryBusy;
    private bool hasCoordinatorLock;
    private int activeParryStates;

    private void Awake()
    {
        playerActionCoordinator = GetComponent<PlayerActionCoordinator>();
        playerStaminaManager = GetComponent<PlayerStaminaManager>();
        playerAnimatorHandler = GetComponent<PlayerAnimatorControllerHandler>();

        PlayerBlockLogic blockLogic = GetComponent<PlayerBlockLogic>();
        nextDamageable = blockLogic == null ? GetComponent<HealthHandler>() : blockLogic;
    }

    private void OnDisable()
    {
        StopFailsafeCoroutine();
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
        if (!isActiveAndEnabled || isParryBusy || playerActionCoordinator == null) return;
        if (!playerActionCoordinator.CanPerformAction(PlayerAbility.Parry)) return;
        if (playerStaminaManager == null || !playerStaminaManager.HasEnoughStamina(parryStamCost)) return;

        playerStaminaManager.UseStamina(parryStamCost);

        isParryBusy = true;
        isParryWindowActive = false;
        hasCoordinatorLock = true;

        playerActionCoordinator.BeginAction(PlayerActionType.Parry, PlayerAbility.None);
        playerAnimatorHandler?.TriggerParry();

        StopFailsafeCoroutine();
        parryFailsafeCoroutine = StartCoroutine(ParryFailsafeCoroutine());

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

    // Standard IDamageable entry point
    public void TakeDmg(float dmg)
    {
        HandleIncomingDamage(dmg, null);
    }

    // Attacker-aware entry point (enables counter-damage and enemy stagger on parry)
    public void TakeDmg(float dmg, GameObject attacker)
    {
        HandleIncomingDamage(dmg, attacker);
    }

    private void HandleIncomingDamage(float dmg, GameObject attacker)
    {
        if (isParryWindowActive)
        {
            isParryWindowActive = false;
            isParryBusy = false;

            StopFailsafeCoroutine();
            ReleaseCoordinatorLock();

            ApplyParryCounter(attacker);
            OnParrySuccess?.Invoke();
            return;
        }

        if (nextDamageable as Object != null)
        {
            nextDamageable.TakeDmg(dmg);
        }
    }

    private void ApplyParryCounter(GameObject attacker)
    {
        if (attacker == null) return;

        // 1. Trigger enemy stagger / deflection state
        IParryable parryable = attacker.GetComponentInParent<IParryable>();
        parryable?.OnParried(gameObject);

        // 2. Deal counter damage back to the attacker
        if (parryCounterDamage > 0f)
        {
            IAttackerAwareDamageable awareAttacker = attacker.GetComponentInParent<IAttackerAwareDamageable>();
            if (awareAttacker != null && !ReferenceEquals(awareAttacker, this))
            {
                awareAttacker.TakeDmg(parryCounterDamage, gameObject);
            }
            else
            {
                IDamageable attackerDamageable = attacker.GetComponentInParent<IDamageable>();
                if (attackerDamageable != null && !ReferenceEquals(attackerDamageable, this))
                {
                    attackerDamageable.TakeDmg(parryCounterDamage);
                }
            }
        }

        OnParriedAttacker?.Invoke(attacker);
    }

    // Called by ParryStateBehaviour when entering the Parry Animator state
    public void NotifyParryStateEntered()
    {
        activeParryStates++;
    }

    // Called by ParryStateBehaviour when exiting the Parry Animator state
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
        StopFailsafeCoroutine();
        isParryWindowActive = false;

        ReleaseCoordinatorLock();

        if (!isParryBusy && activeParryStates == 0)
        {
            OnParryEnd?.Invoke();
            return;
        }

        isParryBusy = false;
        OnParryEnd?.Invoke();
    }

    private IEnumerator ParryFailsafeCoroutine()
    {
        yield return new WaitForSeconds(Mathf.Max(0.2f, maxParryLockDuration));

        parryFailsafeCoroutine = null;
        activeParryStates = 0;
        EndParry();

        if (playerAnimatorHandler != null && playerAnimatorHandler.IsPlayingState("Parry", 0))
        {
            playerAnimatorHandler.ForceReturnToIdle(0.1f);
        }
    }

    private void StopFailsafeCoroutine()
    {
        if (parryFailsafeCoroutine != null)
        {
            StopCoroutine(parryFailsafeCoroutine);
            parryFailsafeCoroutine = null;
        }
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