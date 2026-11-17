using UnityEngine;

public class LockedOnRotationStrategy : IRotationStrategy
{
    private Transform targetTransform;
    public LockedOnRotationStrategy(Transform targetTransform)
    {
        this.targetTransform = targetTransform;
    }
    public Quaternion CalculateRotation(RotationContext rotationContext)
    {
        if (targetTransform == null)
        {
            return rotationContext.playerTransform.rotation; // Return current rotation if no target
        }
        Vector3 directionToTarget = targetTransform.position - rotationContext.playerTransform.position;
        directionToTarget.y = 0; // Keep the rotation on the horizontal plane
        if (directionToTarget.sqrMagnitude > 0.001f) // Avoid zero-length direction
        {
            return Quaternion.LookRotation(directionToTarget);
        }
        else
        {
            return rotationContext.playerTransform.rotation; // Return current rotation if target is too close
        }
    }

}
