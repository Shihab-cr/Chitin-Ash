using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerBlockLogic : MonoBehaviour, IDamageable
{
    public UnityEvent OnBlockStart;
    public UnityEvent OnBlockEnd;
    public UnityEvent OnBlockSuccess;
    public UnityEvent OnGuardBreak;
    [SerializeField] private float blockStaminaCost = 25f;
    [SerializeField] private float blockMoveSpeed = 5f;
    private PlayerStaminaManager playerStaminaManager;
    private PlayerMovementHandler playerMovementHandler;
    private PlayerActionCoordinator playerActionCoordinator;
    private IDamageable playerHealthHandler;
    //private bool canBlock = true;
    private bool isBlocking = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerStaminaManager = GetComponent<PlayerStaminaManager>();
        playerMovementHandler = GetComponent<PlayerMovementHandler>();
        playerHealthHandler = GetComponent<HealthHandler>();
        playerActionCoordinator = GetComponent<PlayerActionCoordinator>();
    }
    void Update()
    {
        if(isBlocking && playerActionCoordinator !=null && !playerActionCoordinator.CanPerformAction(PlayerAbility.Block))
        {
            StopBlocking();
        }
    }
    public void OnBlockInput(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            StopBlocking();
            return;
        }
        //if(!canBlock) return;
        if (playerActionCoordinator == null || !playerActionCoordinator.CanPerformAction(PlayerAbility.Block)) return;

        if (context.started || context.performed) StartBlocking();
        
    }

    public void StartBlocking()
    {
        if(!playerActionCoordinator.CanPerformAction(PlayerAbility.Block) || isBlocking) return;
        isBlocking = true;
        OnBlockStart?.Invoke();
        playerActionCoordinator.BeginAction(PlayerActionType.Block,PlayerAbility.Block | PlayerAbility.Move | PlayerAbility.Rotate | PlayerAbility.Dodge);
        if (playerMovementHandler != null) playerMovementHandler.ChangeMoveSpeedTo(blockMoveSpeed);
    }
    public void StopBlocking()
    {
        if (!isBlocking) return;

        isBlocking = false;
        OnBlockEnd?.Invoke();
        playerActionCoordinator.EndAction(PlayerActionType.Block);
        if (playerMovementHandler != null) playerMovementHandler.ResetMoveSpeed();
    }
   
    public void TakeDmg(float dmg)
    {
        if (!isBlocking)
        {
            playerHealthHandler.TakeDmg(dmg);
            return;
        }

        if (playerStaminaManager != null && playerStaminaManager.HasEnoughStamina(blockStaminaCost))
        {
            playerStaminaManager.UseStamina(blockStaminaCost);
            OnBlockSuccess?.Invoke();
        }
        else
        {
            if (playerStaminaManager != null && playerStaminaManager.CurrentStamina > 0f)
            {
                playerStaminaManager.UseStamina(playerStaminaManager.CurrentStamina);
            }
            StopBlocking();
            OnGuardBreak?.Invoke();
            playerHealthHandler.TakeDmg(dmg);
        }
    }
    private void OnDisable()
    {
        StopBlocking();
    }
}
