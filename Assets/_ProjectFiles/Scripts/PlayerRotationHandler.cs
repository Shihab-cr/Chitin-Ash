using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRotationHandler : MonoBehaviour
{
    [SerializeField] private float turnSpeed = 40f;
    [SerializeField] private InputActionReference moveInputReference;
    private IRotationStrategy rotationStrategy;
    private IRotationStrategy previousRotationStrategy;
    private FreeLockRotationStrategy freeLockRotationStrategy;
    private FrozenLockedRotationStrategy frozenLockedRotationStrategy;
    private RotationContext rotationContext;
    [SerializeField] private Camera mainCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(mainCamera == null) mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        rotationContext = new RotationContext(Vector2.zero, mainCamera.transform, transform);
        
        freeLockRotationStrategy = new FreeLockRotationStrategy();
        frozenLockedRotationStrategy = new FrozenLockedRotationStrategy();
        previousRotationStrategy = freeLockRotationStrategy;
        rotationStrategy = freeLockRotationStrategy; // Default strategy
    }

    // Update is called once per frame
    void Update()
    {
        ExecuteRotationStrategy();
    }
    
    private void ExecuteRotationStrategy()
    {
        rotationContext.moveInput = moveInputReference.action.ReadValue<Vector2>();
        rotationContext.playerTransform = transform;
        rotationContext.cameraTransform = mainCamera.transform;
        Quaternion targetRotation = rotationStrategy.CalculateRotation(rotationContext);
        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed );

    }
    public void SetRotationStrategy(IRotationStrategy newStrategy)
    {
        if (rotationStrategy == frozenLockedRotationStrategy)
            previousRotationStrategy = newStrategy;
        else
            rotationStrategy = newStrategy;
    }

    public void ResetRotationStrategyToDefaultFreeLook()
    {
        if (rotationStrategy == frozenLockedRotationStrategy)
            previousRotationStrategy = freeLockRotationStrategy;
        else
            rotationStrategy = freeLockRotationStrategy;
    }
    public void StopPlayerRotation()
    {
        if (rotationStrategy == frozenLockedRotationStrategy) return;
        this.previousRotationStrategy = rotationStrategy;
        this.rotationStrategy = frozenLockedRotationStrategy;
    }
    public void ResumeRotationStrategy()
    {
        this.rotationStrategy = previousRotationStrategy;
    }

    public IEnumerator RotateTowards(Vector3 direction,float rotationMultiplier)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        while (Mathf.Abs(Mathf.Abs(Quaternion.Dot(transform.rotation, targetRotation))) < 0.999f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed*rotationMultiplier);
            yield return null;
        }
        transform.rotation = targetRotation;
    }

}
