using UnityEngine;
using UnityEngine.UIElements;

public class FreeLockRotationStrategy : IRotationStrategy
{
    public Quaternion CalculateRotation(RotationContext rotationContext)
    {
        Vector3 currentPos = rotationContext.playerTransform.position;

        Vector3 cameraForward = rotationContext.cameraTransform.forward;
        Vector3 cameraRight = rotationContext.cameraTransform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        Vector3 relativeDirection = rotationContext.moveInput.y * cameraForward.normalized + rotationContext.moveInput.x * cameraRight.normalized;
        /*
       
        //I left this code for understanding the rotation or when studying this code later on

        Vector3 targetPos = rotationContext.playerTransform.position + relativeDirection * moveSpeed * Time.deltaTime;
        Vector3 targetDirection = targetPos - currentPos;
        */
        Quaternion currRotation = rotationContext.playerTransform.rotation;
        Quaternion targetRotation = currRotation;
        if (rotationContext.moveInput.magnitude != 0)
        {
            //targetRotation = Quaternion.LookRotation(targetDirection);
            targetRotation = Quaternion.LookRotation(relativeDirection);
        }

        return targetRotation;
    }
}
