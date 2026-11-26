using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerEatingAlignmentHandler : MonoBehaviour
{
    [SerializeField] private float alignmentDuration = 2f;
    [SerializeField] private bool preserveGroundY = true;
    [SerializeField] private PlayerAnimatorControllerHandler playerAnimatorController;
    private Transform alignmentPosition;
    public UnityEvent OnPlayerEating;
    private PlayerActionCoordinator playerActionCoordinator;
    private Coroutine alignmentCoroutine;
    private Rigidbody playerRb;
    private bool isEating;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerActionCoordinator = GetComponent<PlayerActionCoordinator>();
    }

    public bool StartEat(Transform alignmentPoint)
    {
        if (isEating || alignmentPoint == null) return false;
        if (playerActionCoordinator == null || !playerActionCoordinator.CanPerformAction(PlayerAbility.Eat)) return false;

        isEating = true;
        playerActionCoordinator.BeginAction(PlayerActionType.Eat, PlayerAbility.None);
        

        if (playerRb != null)
        {
            playerRb.linearVelocity = new Vector3(0f, playerRb.linearVelocity.y, 0f);
            playerRb.angularVelocity = Vector3.zero;
        }

        if (alignmentCoroutine != null) StopCoroutine(alignmentCoroutine);
        alignmentCoroutine = StartCoroutine(AlignmentCoroutine(alignmentPoint));
        return true;
    }
    public void EndEat()
    {
        if (!isEating) return;

        isEating = false;
        if (alignmentCoroutine != null)
        {
            StopCoroutine(alignmentCoroutine);
            alignmentCoroutine = null;
        }

        if (playerActionCoordinator != null)
        {
            playerActionCoordinator.EndAction(PlayerActionType.Eat);
        }
    }
    private void OnDisable()
    {
        EndEat();
    }
    private IEnumerator AlignmentCoroutine(Transform alignmentPoint)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 targetPos = alignmentPoint.position;
        if (preserveGroundY)
        {
            targetPos.y = startPos.y;
        }

        Vector3 flatForward = alignmentPoint.forward;
        flatForward.y = 0f;
        Quaternion targetRot = flatForward.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(flatForward)
            : startRot;

        float elapsed = 0f;
        float motionSped = this.alignmentDuration < 1 ? 1/this.alignmentDuration : 1;
        if (playerAnimatorController != null)
        {
            playerAnimatorController.SetFastSpeed(motionSped);
        }
        while (elapsed < alignmentDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / alignmentDuration));

            Vector3 nextPos = Vector3.Lerp(startPos, targetPos, t);
            Quaternion nextRot = Quaternion.Slerp(startRot, targetRot, t);
            if(playerAnimatorController != null)
            {
                playerAnimatorController.SetMovement(nextPos.normalized.x, nextPos.normalized.z);
            }
            transform.position = nextPos;
            transform.rotation = nextRot;
            if (playerRb != null)
            {
                playerRb.position = nextPos;
                playerRb.rotation = nextRot;
            }

            yield return null;
        }
        if (playerAnimatorController != null) playerAnimatorController.ResetNormalSpeed();
        transform.position = targetPos;
        transform.rotation = targetRot;
        if (playerRb != null)
        {
            playerRb.position = targetPos;
            playerRb.rotation = targetRot;
        }

        OnPlayerEating?.Invoke();

        alignmentCoroutine = null;
    }
}
