using UnityEngine;

public class FrozenLockedRotationStrategy : IRotationStrategy
{
    public Quaternion CalculateRotation(RotationContext rotationContext)
    {
        return rotationContext.playerTransform.rotation; // Return current rotation if no target
    }
}
