using System.Collections;
using UnityEngine;

public class PlayerDodgeLogic : MonoBehaviour
{
    private Rigidbody playerRb;
    private PlayerMovementHandler playerMovementHandler;
    private PlayerRotationHandler playerRotationHandler;
    private PlayerActionCoordinator playerActionCoordinator;
    private PlayerStaminaManager playerStaminaManager;
    private PlayerLockOnLogic playerLockOnLogic;
    private HealthHandler healthHandler;
    private Camera mainCamera;
    [SerializeField] private float dodgeForce = 10f;
    [SerializeField] private float dodgeCoolDown = 0.5f;
    [SerializeField] private float dodgeDuration = 0.35f;
    [SerializeField] private float dodgeIframeTime = 0.2f;
    [SerializeField] private float staminaCost = 10f;
    [SerializeField] private float dodgeRotationSpeedMultiplier = 2.2f;
    private Coroutine dodgeCoolDownCoroutine;
    private bool canDodge = true;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        healthHandler = GetComponent<HealthHandler>();
        playerStaminaManager = GetComponent<PlayerStaminaManager>();
        playerRotationHandler = GetComponent<PlayerRotationHandler>();
        playerLockOnLogic = GetComponent<PlayerLockOnLogic>();
        mainCamera = Camera.main;
        playerMovementHandler = GetComponent<PlayerMovementHandler>();
        playerActionCoordinator = GetComponent<PlayerActionCoordinator>();
    }
    public void Dodge()
    {
        if (playerMovementHandler == null || playerRotationHandler == null || playerActionCoordinator == null) return;
        if (!canDodge ||!playerActionCoordinator.CanPerformAction(PlayerAbility.Dodge)) return;
        if(playerStaminaManager == null || !playerStaminaManager.UseStamina(staminaCost)) return;
        
        

        Vector2 moveDir = playerMovementHandler.GetMoveDirection();
        Vector3 dodgeDirection;
        if(moveDir.magnitude == 0)
        {
            dodgeDirection = -transform.forward; 
        }
        else
        {
            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 cameraRight = mainCamera.transform.right;
            cameraForward.y = 0;
            cameraRight.y = 0;
            dodgeDirection = (moveDir.y * cameraForward.normalized + moveDir.x * cameraRight.normalized).normalized;
        }
        playerRb.linearVelocity = new Vector3(0f, playerRb.linearVelocity.y, 0f);
        canDodge = false;
        if(dodgeCoolDownCoroutine != null) StopCoroutine(dodgeCoolDownCoroutine);
        dodgeCoolDownCoroutine = StartCoroutine(DodgeCoolDown(dodgeDirection, moveDir));
    }
    private IEnumerator DodgeCoolDown(Vector3 dodgeDirection, Vector2 moveDir)
    {
        canDodge = false;
        //playerMovementHandler.DisablePlayerMovement();
        //playerRotationHandler.StopPlayerRotation();
        playerActionCoordinator.BeginAction(PlayerActionType.Dodge, PlayerAbility.None);
        // Smooth turn toward dodge direction before launching
        if (moveDir.magnitude > 0)
        {
            //yield return StartCoroutine(playerRotationHandler.RotateTowards(dodgeDirection,dodgeRotationSpeedMultiplier));
            Vector3 flatDir = new Vector3(dodgeDirection.x, 0f, dodgeDirection.z);
            if (flatDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatDir);
                transform.rotation = targetRot;
                playerRb.rotation = targetRot;
            }
        }

        // Apply dodge force after turn completes
        playerRb.linearVelocity = new Vector3(0f, playerRb.linearVelocity.y, 0f);
        playerRb.AddForce(dodgeDirection * dodgeForce, ForceMode.Impulse);

        if (healthHandler != null) healthHandler.DisableTakingDamage();
        yield return new WaitForSeconds(dodgeIframeTime);
        if (healthHandler != null) healthHandler.EnableTakingDamage();
        yield return new WaitForSeconds(Mathf.Max(0f,dodgeDuration - dodgeIframeTime));
        //playerMovementHandler.EnablePlayerMovement();
        //playerRotationHandler.ResumePlayerRotation();
        playerActionCoordinator.EndAction(PlayerActionType.Dodge);
        yield return new WaitForSeconds(dodgeCoolDown);
        canDodge = true;
        dodgeCoolDownCoroutine = null;
    }
    
    private void OnDisable()
    {
        if (dodgeCoolDownCoroutine != null)
        {
            StopCoroutine(dodgeCoolDownCoroutine);
            dodgeCoolDownCoroutine = null;
        }
        canDodge = true;
        if (healthHandler != null) healthHandler.EnableTakingDamage();
        if (playerActionCoordinator != null) playerActionCoordinator.EndAction(PlayerActionType.Dodge);
    }
}
