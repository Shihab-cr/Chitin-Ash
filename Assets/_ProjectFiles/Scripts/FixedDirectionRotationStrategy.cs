using UnityEngine;

public class FixedDirectionRotationStrategy : IRotationStrategy
{
    private bool hasValidDir = true;
    private Quaternion targetRotation;
    public FixedDirectionRotationStrategy(Vector3 dir)
    {
        dir.y = 0f;
        hasValidDir = dir.magnitude > 0.001f;
        targetRotation =  hasValidDir? Quaternion.LookRotation(dir) : Quaternion.identity;
    }
    public Quaternion CalculateRotation(RotationContext rotationContext)
    {
        
        return hasValidDir? targetRotation: rotationContext.playerTransform.rotation;
    }
}
