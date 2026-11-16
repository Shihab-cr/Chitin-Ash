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
    private IDamageable playerHealthHandler;
    private bool canBlock = true;
    private bool isBlocking = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerStaminaManager = GetComponent<PlayerStaminaManager>();
        playerMovementHandler = GetComponent<PlayerMovementHandler>();
        playerHealthHandler = GetComponent<HealthHandler>();
    }
    public void OnBlockInput(InputAction.CallbackContext context)
    {
        if(!canBlock) return;

        if (context.started || context.performed) StartBlocking();
        else if (context.canceled) StopBlocking();
    }

    public void StartBlocking()
    {
        if(!canBlock || isBlocking) return;
        isBlocking = true;
        OnBlockStart?.Invoke();
        if (playerMovementHandler != null) playerMovementHandler.ChangeMoveSpeedTo(blockMoveSpeed);
    }
    public void StopBlocking()
    {
        isBlocking = false;
        OnBlockEnd?.Invoke();
        if (playerMovementHandler != null) playerMovementHandler.ResetMoveSpeed();
    }
    public void DisableBlocking()
    {
        canBlock = false;
        StopBlocking();
    }
    public void EnableBlocking()
    {
        canBlock = true;
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
            StopBlocking();
            OnGuardBreak?.Invoke();
            playerHealthHandler.TakeDmg(dmg);
        }
    }
}
