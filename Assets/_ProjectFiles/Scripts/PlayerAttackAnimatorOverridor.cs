using UnityEngine;

public class PlayerAttackAnimatorOverridor : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private string attackStateName = "Attack";
    [SerializeField] private string placeholderClipName = "Placeholder_Attack";
    [SerializeField] private float crossFadeDuration = 0.05f;

    private AnimatorOverrideController animatorOverrideController;
    private int attackStateHash;

    private void Awake()
    {
        if (playerAnimator == null)
            playerAnimator = GetComponentInChildren<Animator>();

        if (playerAnimator == null)
        {
            Debug.LogError($"[{nameof(PlayerAttackAnimatorOverridor)}] No Animator found on {name} or its children.", this);
            enabled = false;
            return;
        }

        attackStateHash = Animator.StringToHash(attackStateName);

        if (playerAnimator.runtimeAnimatorController is AnimatorOverrideController existingOverride)
        {
            animatorOverrideController = existingOverride;
        }
        else
        {
            animatorOverrideController = new AnimatorOverrideController(playerAnimator.runtimeAnimatorController);
            playerAnimator.runtimeAnimatorController = animatorOverrideController;
        }
    }

    public void OverrideAttackAnimation(AnimationClip attackAnimation)
    {
        if (animatorOverrideController == null || attackAnimation == null) return;

        animatorOverrideController[placeholderClipName] = attackAnimation;

        // Force the Animator to bind the newly swapped clip before restarting the state
        playerAnimator.Update(0f);
        playerAnimator.CrossFadeInFixedTime(attackStateHash, crossFadeDuration, 0, 0f);
    }
}