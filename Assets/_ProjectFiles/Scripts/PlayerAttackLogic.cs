using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.InputSystem.Interactions;

public class PlayerAttackLogic : MonoBehaviour
{
    [SerializeField] private LayerMask damageableLayermask;

    
    private PlayerMovementHandler playerMovementHandler;
    private PlayerRotationHandler playerRotationHandler;
    private PlayerStaminaManager playerStaminaManager;
    private PlayerBlockLogic playerBlockLogic;
    private PlayerDodgeLogic playerDodgeLogic;
    private IAttackStrategy currAtckStrategy;
    private PlayerLightAttackLogic playerLightAtkLogic;
    private PlayerHeavyAttackLogic playerHeavyAtkLogic;
    private PlayerSpecialAttackLogic playerSpecialAtkLogic;
    private PlayerAttackSO currAtkSO;
    private bool canAtk= true;
    public Transform damageTransform;
    private float damage;

    void Start()
    {
        playerMovementHandler = GetComponent<PlayerMovementHandler>();
        playerRotationHandler = GetComponent<PlayerRotationHandler>();
        playerStaminaManager = GetComponent<PlayerStaminaManager>();
        playerDodgeLogic = GetComponent<PlayerDodgeLogic>();
        playerBlockLogic = GetComponent<PlayerBlockLogic>();

        playerLightAtkLogic = GetComponent<PlayerLightAttackLogic>();
        playerHeavyAtkLogic = GetComponent<PlayerHeavyAttackLogic>();
        playerSpecialAtkLogic = GetComponent<PlayerSpecialAttackLogic>();
    }
   public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!canAtk) return;
            SetAttackStrategy(playerLightAtkLogic);
            if (!currAtckStrategy.CanPerformAttack()) return;
            ExecuteAttackLogic();
        }
        
    }
    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!canAtk || !playerHeavyAtkLogic.CanPerformAttack()) return;
            SetAttackStrategy(playerHeavyAtkLogic);
            playerHeavyAtkLogic.ChargeHeavyAtk(context);
            LockPlayerActions();
        }
        else if (context.canceled)
        {
            if (!playerHeavyAtkLogic.IsCharging) return;
            playerHeavyAtkLogic.ChargeHeavyAtk(context);
            ExecuteAttackLogic();
        }
        
    }
    public void OnSpecialAttack0(InputAction.CallbackContext context)
    {
        HandleSpecialAttackInput(context, 0);
    }
    public void OnSpecialAttack1(InputAction.CallbackContext context)
    {
        HandleSpecialAttackInput(context, 1);
    }
    public void OnSpecialAttack2(InputAction.CallbackContext context)
    {
        HandleSpecialAttackInput(context, 2);
    }
    public void OnSpecialAttack3(InputAction.CallbackContext context)
    {
        HandleSpecialAttackInput(context, 3);
    }
    public void OnSpecialAttack4(InputAction.CallbackContext context)
    {
        HandleSpecialAttackInput(context, 4);
    }
    private void HandleSpecialAttackInput(InputAction.CallbackContext context, int index)
    {
        if (context.performed)
        {
            if (!canAtk) return;
            playerSpecialAtkLogic.ChooseSpecialAtk(index);
            SetAttackStrategy(playerSpecialAtkLogic);
            if (!currAtckStrategy.CanPerformAttack()) return;
            ExecuteAttackLogic();
        }
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

    //This method will be called by the animation events
    public void ApplyDamage()
    {
        if (currAtkSO == null || damageTransform == null || currAtckStrategy==null) return;
        Collider[] damageables = Physics.OverlapSphere(damageTransform.position, currAtkSO.damageRadius, damageableLayermask);
        foreach(Collider damageable in damageables)
        {
            if (damageable.TryGetComponent<IDamageable>(out IDamageable idmg))
            {
                damage = currAtckStrategy.GetDamage();
                idmg.TakeDmg(damage);

            }
        }
    }

    private void LockPlayerActions()
    {
        canAtk = false;
        if (playerMovementHandler != null) playerMovementHandler.DisablePlayerMovement();
        if (playerRotationHandler != null) playerRotationHandler.StopPlayerRotation();
        
        if (playerDodgeLogic != null) playerDodgeLogic.DisableDodging();
        if (playerBlockLogic != null) playerBlockLogic.DisableBlocking();
    }
    //Called to stop the player from movement can be delegated to the animation clip
    public void StartAtk()
    {
        LockPlayerActions();
        if (playerStaminaManager != null) playerStaminaManager.UseStamina(currAtckStrategy.GetStaminaCost());

    }

    //Called to re-enable player movement, can be delegated to the animation clip
    public void EndAtk()
    {
        canAtk = true;
        if (playerMovementHandler != null) playerMovementHandler.EnablePlayerMovement();
        if (playerRotationHandler != null) playerRotationHandler.ResumePlayerRotation();
        if (playerDodgeLogic != null) playerDodgeLogic.EnableDodging();
        if (playerBlockLogic != null) playerBlockLogic.EnableBlocking();
        currAtckStrategy?.OnEndAtk();
        currAtckStrategy = null;
    }
    
    
    
    
}
