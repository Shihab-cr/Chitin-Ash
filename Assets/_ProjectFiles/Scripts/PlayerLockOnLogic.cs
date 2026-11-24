using UnityEngine;

public class PlayerLockOnLogic : MonoBehaviour
{
    [SerializeField] private float lockOnRange = 20f;
    [SerializeField] private LayerMask lockOnLayerMask;
    [SerializeField] private float maxLockOnDist = 30f;
    private GameObject lockedOnTarget;
    private LockedOnRotationStrategy lockedOnRotationStrategy;
    private PlayerRotationHandler playerRotationHandler;
    private bool isLockedOn = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRotationHandler = GetComponent<PlayerRotationHandler>();
    }
    void Update()
    {
        CheckPlayerToTargetDist();
    }
    public void LockOn()
    {
        if (playerRotationHandler == null) return;

        if (!isLockedOn && lockedOnTarget == null)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, lockOnRange, lockOnLayerMask);
            float closestDistance = Mathf.Infinity;
            GameObject closestTarget = null;
            foreach (Collider collider in hitColliders)
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = collider.gameObject;
                }
            }
            if (closestTarget != null && closestTarget.activeInHierarchy)
            {
                lockedOnTarget = closestTarget;
                HealthHandler targetHealthHandler = lockedOnTarget.GetComponent<HealthHandler>();
                if(targetHealthHandler != null)
                {
                    targetHealthHandler.OnDeath.AddListener(StopLockOn);
                }
                else
                {
                    Debug.LogError("Enemy Doesn't have HealthHandler attached!");
                }
                isLockedOn = true;
                lockedOnRotationStrategy = new LockedOnRotationStrategy(lockedOnTarget.transform);
                playerRotationHandler.SetRotationStrategy(lockedOnRotationStrategy);
            }
        }
        else
        {
            StopLockOn();
        }
    }
    public void StopLockOn()
    {
        if(lockedOnTarget != null)
        {
            HealthHandler targetHealthHandler = lockedOnTarget.GetComponent<HealthHandler>();
            if(targetHealthHandler != null)
            {
                targetHealthHandler.OnDeath.RemoveListener(StopLockOn);
            }
            else
            {
                Debug.LogError("Enemy doesn't have healthHandler script attached!");
            }
        }
        lockedOnTarget = null;
        isLockedOn = false;
        playerRotationHandler.ResetRotationStrategyToDefaultFreeLook();
    }
    private void CheckPlayerToTargetDist()
    {
        if(lockedOnTarget != null)
        {
            float playerToTargetDist = Vector3.Distance(transform.position, lockedOnTarget.transform.position);
            if (playerToTargetDist > maxLockOnDist || !lockedOnTarget.activeInHierarchy)
            {
                StopLockOn();
            }
        }
    }
    public bool GetIsLockedOn()
    {
        return this.isLockedOn;
    }
}
