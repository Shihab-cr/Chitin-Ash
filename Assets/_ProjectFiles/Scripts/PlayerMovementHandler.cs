using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementHandler : MonoBehaviour
{
    [SerializeField] private float defaultMoveSpeed = 15f;
    //[SerializeField] private float turnSpeed = 40f;
    private float moveSpeed;
    [SerializeField] private Camera mainCamera;

    private Rigidbody playerRb;
    private bool canMove = true;
    //private Quaternion previousRotation;
    private Vector2 moveDir = Vector2.zero;
    [SerializeField] private InputActionReference moveRef;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveSpeed = defaultMoveSpeed;
        playerRb = GetComponent<Rigidbody>();
        //previousRotation = transform.rotation;
        if (mainCamera == null) mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
    
    private void FixedUpdate()
    {
        moveDir = moveRef.action.ReadValue<Vector2>();
        HandlePlayerMovement(moveDir);
    }
    public void HandlePlayerMovement(Vector2 moveDir)
    {
        if (!canMove) return;
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        Vector3 moveProjection = moveDir.y * cameraForward.normalized +moveDir.x * cameraRight.normalized;
        Vector3 movementVector = moveProjection*moveSpeed*Time.fixedDeltaTime;
        if (playerRb != null) playerRb.MovePosition(transform.position + movementVector);
        if(movementVector == Vector3.zero)
        {
            playerRb.linearVelocity = new Vector3(0f, playerRb.linearVelocity.y, 0f);
            playerRb.angularVelocity = Vector3.zero;
        }
    }
   
    public Vector2 GetMoveDirection()
    {
        return moveDir;
    }
    
    public void DisablePlayerMovement()
    {
        canMove = false;
    }
    public void EnablePlayerMovement()
    {
        canMove = true;
    }
    public void ChangeMoveSpeedTo(float newSpeed)
    {
               moveSpeed = newSpeed;
    }
    public void ResetMoveSpeed()
    {
        moveSpeed = defaultMoveSpeed;
    }
}


/*
 private void Update()
    {
        Vector2 lookAtDir = moveRef.action.ReadValue<Vector2>();
        HandlePlayerRotation(lookAtDir);
    }


 public void HandlePlayerRotation(Vector2 moveDir)
    {
        if (!canMove) return;
        
        Vector3 currentPos = transform.position;
        
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        Vector3 relativeDirection = moveDir.y * cameraForward.normalized + moveDir.x * cameraRight.normalized;
        
       
        //I left this code for understanding the rotation or when studying this code later on

        //Vector3 targetPos = transform.position+relativeDirection*moveSpeed* Time.deltaTime;
        //Vector3 targetDirection = targetPos - currentPos;
        
        Quaternion targetRotation = previousRotation;
        Quaternion currRotation = transform.localRotation;
        if (moveDir.magnitude != 0)
        {
            if (previousRotation != currRotation)
            {
                previousRotation = currRotation;
            }
            //targetRotation = Quaternion.LookRotation(targetDirection);
            targetRotation = Quaternion.LookRotation(relativeDirection);
        }

        transform.localRotation = Quaternion.Slerp(currRotation, targetRotation, turnSpeed*Time.deltaTime);
        
        
    }

 */
