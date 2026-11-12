using System.Collections;
using UnityEngine;

public class PlayerDodgeLogic : MonoBehaviour
{
    private Rigidbody playerRb;
    private PlayerMovementHandler playerMovementHandler;
    private HealthHandler healthHandler;
    private Camera mainCamera;
    [SerializeField] private float dodgeForce = 10f;
    [SerializeField] private float dodgeCoolDown = 0.5f;
    [SerializeField] private float dodgeDuration = 0.35f;
    [SerializeField] private float dodgeIframeTime = 0.2f;
    
    private Coroutine dodgeCoolDownCoroutine;
    private bool canDodge = true;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        healthHandler = GetComponent<HealthHandler>();
        mainCamera = Camera.main;
        playerMovementHandler = GetComponent<PlayerMovementHandler>();
    }
    public void Dodge()
    {
        if (playerMovementHandler == null) return;
        if (!canDodge) return;
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
        playerRb.AddForce(dodgeDirection * dodgeForce, ForceMode.Impulse);
        canDodge = false;
        if(dodgeCoolDownCoroutine != null) StopCoroutine(dodgeCoolDownCoroutine);
        dodgeCoolDownCoroutine = StartCoroutine(DodgeCoolDown());
    }
    private IEnumerator DodgeCoolDown()
    {
        canDodge = false;
        playerMovementHandler.DisablePlayerMovement();
        if(healthHandler != null) healthHandler.DisableTakingDamage();
        yield return new WaitForSeconds(dodgeIframeTime);
        if(healthHandler != null) healthHandler.EnableTakingDamage();
        yield return new WaitForSeconds(dodgeDuration - dodgeIframeTime);
        playerMovementHandler.EnablePlayerMovement();
        yield return new WaitForSeconds(dodgeCoolDown);
        canDodge = true;
        dodgeCoolDownCoroutine = null;

    }
}
