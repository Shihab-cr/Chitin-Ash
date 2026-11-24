using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerSprintHandler : MonoBehaviour
{
    [SerializeField] private float sprintSpeedMultiplier = 1.5f;
    [SerializeField] private float staminaConsumptionRate = 1f;
    private PlayerMovementHandler playerMovementHandler;
    private PlayerStaminaManager playerStaminaManager;
    private PlayerActionCoordinator playerActionCoordinator;
    public UnityEvent OnSprintStart;
    public UnityEvent OnSprintEnd;
    private Coroutine playerSprintCoroutine;
    private bool isSprinting = false;
    private float appliedSprintSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovementHandler = GetComponent<PlayerMovementHandler>();
        playerActionCoordinator = GetComponent<PlayerActionCoordinator>();
        playerStaminaManager = GetComponent<PlayerStaminaManager>();
    }
    private void OnDisable()
    {
        StopSprint();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            StopSprint();
            return;
        }
        
        if (context.performed)
        {
            if (isSprinting || playerMovementHandler == null || playerActionCoordinator == null) return;
            if (!playerActionCoordinator.CanPerformAction(PlayerAbility.Sprint) || !playerActionCoordinator.CanPerformAction(PlayerAbility.Move)) return;
            if (playerStaminaManager == null || !playerStaminaManager.HasEnoughStamina(staminaConsumptionRate * Time.deltaTime)) return;
            StartSprint();
            
        }
         
    }
    private void StartSprint()
    {
        isSprinting = true;
        OnSprintStart?.Invoke();
        playerActionCoordinator.BeginAction(PlayerActionType.Sprint, PlayerAbility.All);
        if (playerSprintCoroutine != null) StopCoroutine(playerSprintCoroutine);
        
        playerSprintCoroutine = StartCoroutine(SprintCoroutine());
        ApplySprintSpeedBoost();
    }
    private void StopSprint()
    {
        if (!isSprinting) return;

        isSprinting = false;
        if (playerSprintCoroutine != null)
        {
            StopCoroutine(playerSprintCoroutine);
            playerSprintCoroutine = null;
        }
        if (playerActionCoordinator != null) playerActionCoordinator.EndAction(PlayerActionType.Sprint);

        RemoveSprintSpeedBoost();
        OnSprintEnd?.Invoke();
    }
    private void ApplySprintSpeedBoost()
    {
        if (playerMovementHandler == null) return;
        float currSpeed = playerMovementHandler.DefaultMoveSpeed;
        appliedSprintSpeed = currSpeed * sprintSpeedMultiplier;
        playerMovementHandler.ChangeMoveSpeedTo(appliedSprintSpeed);
    }
    private void RemoveSprintSpeedBoost()
    {
        if (playerMovementHandler == null) return;
        if (Mathf.Approximately(playerMovementHandler.MoveSpeed, appliedSprintSpeed))
        {
            playerMovementHandler.ResetMoveSpeed();

        }
    }
    private IEnumerator SprintCoroutine()
    {
        while (isSprinting)
        {
            if (playerActionCoordinator == null || !playerActionCoordinator.CanPerformAction(PlayerAbility.Sprint) || !playerActionCoordinator.CanPerformAction(PlayerAbility.Move))
            {
                StopSprint();
                yield break;
            }
            bool isMoving = playerMovementHandler != null && playerMovementHandler.GetMoveDirection().sqrMagnitude > 0.01f;
            if (isMoving)
            {
                float frameCost = staminaConsumptionRate * Time.deltaTime;
                if (playerStaminaManager != null && playerStaminaManager.HasEnoughStamina(frameCost))
                {
                    playerStaminaManager.UseStamina(frameCost);
                }
                else
                {
                    StopSprint();
                    yield break;
                }
            }
            
            yield return null;
        }
        
    }
}
