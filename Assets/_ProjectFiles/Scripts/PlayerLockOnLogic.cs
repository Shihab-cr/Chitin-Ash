using UnityEngine;

public class PlayerLockOnLogic : MonoBehaviour
{
    [SerializeField] private float lockOnRange = 20f;
    [SerializeField] private LayerMask lockOnLayerMask;
    private GameObject lockedOnTarget;
    private PlayerMovementHandler playerMovementHandler;
    private bool isLockedOn = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerMovementHandler = GetComponent<PlayerMovementHandler>();
    }
    public void LockOn()
    {
        if (playerMovementHandler == null) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, lockOnRange, lockOnLayerMask);
        if (!isLockedOn && lockedOnTarget == null)
        {
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
            if (closestTarget != null)
            {
                lockedOnTarget = closestTarget;
                isLockedOn = true;
                Vector3 targetDir = (lockedOnTarget.transform.position - transform.position).normalized;
                Vector2 moveDir = new Vector2(targetDir.x, targetDir.z).normalized;
                playerMovementHandler.HandlePlayerRotation(moveDir);
            }
        }
        else
        {
            if (hitColliders.Length > 0)
            {
                foreach (Collider collider in hitColliders)
                {
                    if (collider.gameObject != lockedOnTarget)
                    {
                        lockedOnTarget = collider.gameObject;
                        isLockedOn = true;
                        break;
                    }
                }
            }
            else
            {
                lockedOnTarget = null;
                isLockedOn = false;
            }
        }
    }
}
