using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackLogic : MonoBehaviour
{
    [SerializeField] private LayerMask damageableLayermask;
    [Tooltip("How long (in seconds) an attack press is remembered before the current attack finishes.")]
    [SerializeField] private float attackBufferDuration = 0.3f;
    private readonly System.Collections.Generic.HashSet<IDamageable> hitTargets = new();
    private PlayerStaminaManager playerStaminaManager;
    private IAttackStrategy currAtckStrategy;
    private PlayerLightAttackLogic playerLightAtkLogic;
    private PlayerHeavyAttackLogic playerHeavyAtkLogic;
    private PlayerSpecialAttackLogic playerSpecialAtkLogic;
    private PlayerActionCoordinator playerActionCoordinator;
    private PlayerAnimatorControllerHandler playerAnimatorHandler;
    private PlayerAttackSO currAtkSO;

    private bool canAtk = true;
    public Transform damageTransform;
    private float damage;

    // Input Buffer State
    private IAttackStrategy bufferedStrategy;
    private float bufferExpireTime;

    private bool HasBufferedAttack => bufferedStrategy != null && Time.time <= bufferExpireTime;

    private void Start()
    {
        playerStaminaManager = GetComponent<PlayerStaminaManager>();
        playerActionCoordinator = GetComponent<PlayerActionCoordinator>();
        playerAnimatorHandler = GetComponent<PlayerAnimatorControllerHandler>();

        playerLightAtkLogic = GetComponent<PlayerLightAttackLogic>();
        playerHeavyAtkLogic = GetComponent<PlayerHeavyAttackLogic>();
        playerSpecialAtkLogic = GetComponent<PlayerSpecialAttackLogic>();
    }

    private void OnDisable()
    {
        ClearAttackBuffer();
        playerLightAtkLogic?.ResetCombo();

        if (!canAtk || currAtckStrategy != null)
        {
            canAtk = true;
            currAtckStrategy = null;
            if (playerActionCoordinator != null)
            {
                playerActionCoordinator.EndAction(PlayerActionType.Attack);
            }
        }
    }

    private void Update()
    {
        // Clean up attack state and buffer if interrupted mid-swing (e.g., Stun, Parry, or Death)
        if (!canAtk && playerActionCoordinator != null && !playerActionCoordinator.CanPerformAction(PlayerAbility.Attack))
        {
            ClearAttackBuffer();
            playerLightAtkLogic?.ResetCombo();
            canAtk = true;
            currAtckStrategy = null;
            return;
        }

        if (currAtckStrategy as Object == playerHeavyAtkLogic && playerHeavyAtkLogic != null && playerHeavyAtkLogic.IsCharging)
        {
            if (playerActionCoordinator != null && !playerActionCoordinator.CanPerformAction(PlayerAbility.Attack))
            {
                playerHeavyAtkLogic.CancelCharge();
                EndAtk();
            }
        }
    }

    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (!context.performed || playerLightAtkLogic == null) return;

        // If currently mid-attack, buffer the next combo input instead of dropping it
        if (!canAtk)
        {
            if (playerLightAtkLogic.HasStaminaForNextAttack())
            {
                BufferAttack(playerLightAtkLogic);
                playerLightAtkLogic.CancelComboResetTimer();
            }
            return;
        }

        if (playerActionCoordinator == null || !playerActionCoordinator.CanPerformAction(PlayerAbility.Attack)) return;
        SetAttackStrategy(playerLightAtkLogic);
        if (!currAtckStrategy.CanPerformAttack()) return;
        ExecuteAttackLogic();
    }

    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (playerHeavyAtkLogic == null) return;

        if (context.performed)
        {
            if (!canAtk || playerActionCoordinator == null || !playerActionCoordinator.CanPerformAction(PlayerAbility.Attack) || !playerHeavyAtkLogic.CanPerformAttack()) return;
            ClearAttackBuffer();
            SetAttackStrategy(playerHeavyAtkLogic);
            playerHeavyAtkLogic.ChargeHeavyAtk(context);
            LockPlayerActions();
        }
        else if (context.canceled)
        {
            if (!playerHeavyAtkLogic.IsCharging) return;

            if (playerActionCoordinator == null || !playerActionCoordinator.CanPerformAction(PlayerAbility.Attack))
            {
                playerHeavyAtkLogic.CancelCharge();
                EndAtk();
                return;
            }

            if (!playerHeavyAtkLogic.HasReachedMinCharge())
            {
                playerHeavyAtkLogic.CancelCharge();
                EndAtk();
                return;
            }

            playerHeavyAtkLogic.ChargeHeavyAtk(context);
            ExecuteAttackLogic();
        }
    }

    public void OnSpecialAttack0(InputAction.CallbackContext context) => HandleSpecialAttackInput(context, 0);
    public void OnSpecialAttack1(InputAction.CallbackContext context) => HandleSpecialAttackInput(context, 1);
    public void OnSpecialAttack2(InputAction.CallbackContext context) => HandleSpecialAttackInput(context, 2);
    public void OnSpecialAttack3(InputAction.CallbackContext context) => HandleSpecialAttackInput(context, 3);
    public void OnSpecialAttack4(InputAction.CallbackContext context) => HandleSpecialAttackInput(context, 4);

    private void HandleSpecialAttackInput(InputAction.CallbackContext context, int index)
    {
        if (!context.performed || playerSpecialAtkLogic == null) return;

        if (!canAtk)
        {
            playerSpecialAtkLogic.ChooseSpecialAtk(index);
            BufferAttack(playerSpecialAtkLogic);
            playerLightAtkLogic?.CancelComboResetTimer();
            return;
        }

        if (playerActionCoordinator == null || !playerActionCoordinator.CanPerformAction(PlayerAbility.Attack)) return;
        playerSpecialAtkLogic.ChooseSpecialAtk(index);
        SetAttackStrategy(playerSpecialAtkLogic);
        if (!currAtckStrategy.CanPerformAttack()) return;
        ExecuteAttackLogic();
    }

    private void BufferAttack(IAttackStrategy strategy)
    {
        if (strategy == null) return;
        bufferedStrategy = strategy;
        bufferExpireTime = Time.time + attackBufferDuration;
    }

    public void ClearAttackBuffer()
    {
        bufferedStrategy = null;
        bufferExpireTime = 0f;
    }

    public void SetAttackStrategy(IAttackStrategy attackStrategy)
    {
        currAtckStrategy = attackStrategy;
    }

    public void ExecuteAttackLogic()
    {
        if (currAtckStrategy == null) return;
        currAtkSO = currAtckStrategy.GetcurrAtkSO();
        if (currAtkSO == null) return;

        StartAtk();
        currAtckStrategy.PerformAtk();
    }

    // Called by Animation Event on the hit frame
    public void ApplyDamage()
    {
        if (currAtkSO == null || damageTransform == null || currAtckStrategy == null) return;

        hitTargets.Clear();
        Collider[] damageables = Physics.OverlapSphere(damageTransform.position, currAtkSO.damageRadius, damageableLayermask);

        foreach (Collider damageable in damageables)
        {
            IDamageable idmg = damageable.GetComponentInParent<IDamageable>();
            if (idmg != null && hitTargets.Add(idmg))
            {
                damage = currAtckStrategy.GetDamage();

                if (idmg is IAttackerAwareDamageable awareTarget)
                {
                    awareTarget.TakeDmg(damage, gameObject);
                }
                else
                {
                    idmg.TakeDmg(damage);
                }
            }
        }
    }

    private void LockPlayerActions()
    {
        canAtk = false;
        if (playerActionCoordinator != null)
        {
            playerActionCoordinator.BeginAction(PlayerActionType.Attack, PlayerAbility.Attack);
        }
    }

    public void StartAtk()
    {
        LockPlayerActions();
        if (playerStaminaManager != null)
        {
            playerStaminaManager.UseStamina(currAtckStrategy.GetStaminaCost());
        }
    }

    // Called by Animation Event at the end of the attack clip
    public void EndAtk()
    {
        canAtk = true;
        currAtckStrategy?.OnEndAtk();
        currAtckStrategy = null;

        if (!isActiveAndEnabled)
        {
            ClearAttackBuffer();
            if (playerActionCoordinator != null)
            {
                playerActionCoordinator.EndAction(PlayerActionType.Attack);
            }
            return;
        }

        // Immediately consume buffered input and chain into the next swing without returning to idle
        if (HasBufferedAttack)
        {
            IAttackStrategy nextStrategy = bufferedStrategy;
            ClearAttackBuffer();

            if (playerActionCoordinator != null && playerActionCoordinator.CanPerformAction(PlayerAbility.Attack))
            {
                SetAttackStrategy(nextStrategy);
                if (currAtckStrategy != null && currAtckStrategy.CanPerformAttack())
                {
                    ExecuteAttackLogic();
                    return;
                }
            }
        }

        // No valid buffered attack: unlock coordinator and crossfade back to idle
        ClearAttackBuffer();
        if (playerActionCoordinator != null)
        {
            playerActionCoordinator.EndAction(PlayerActionType.Attack);
        }

        if (playerAnimatorHandler != null && playerAnimatorHandler.IsPlayingState("Attack", 0))
        {
            playerAnimatorHandler.ForceReturnToIdle(0.1f);
        }
    }
}