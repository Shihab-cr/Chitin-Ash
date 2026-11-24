using UnityEngine;
public struct RotationContext
{
    public Vector2 moveInput;
    public Transform cameraTransform;
    public Transform playerTransform;

    public RotationContext(Vector2 moveInput, Transform cameraTransform, Transform playerTransform)
    {
        this.moveInput = moveInput;
        this.cameraTransform = cameraTransform;
        this.playerTransform = playerTransform;
    }
}
public interface IRotationStrategy
{
    public Quaternion CalculateRotation(RotationContext rotationContext);
}
