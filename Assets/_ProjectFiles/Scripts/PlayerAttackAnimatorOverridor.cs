using UnityEngine;

public class PlayerAttackAnimatorOverridor : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    private AnimatorOverrideController animatorOverrideController;
    private static readonly int attackStateHash = Animator.StringToHash("Attack"); //Attack is name of the state in the animator
    
    void Awake()
    {
        animatorOverrideController = new AnimatorOverrideController(playerAnimator.runtimeAnimatorController);
        playerAnimator.runtimeAnimatorController = animatorOverrideController;
    }
    public void OverrideAttackAnimation(AnimationClip attackAnimation)
    {
        //Placeholder_Attack is the name of the fake animationclip to be replaced
        animatorOverrideController["Placeholder_Attack"] = attackAnimation;
        playerAnimator.CrossFade(attackStateHash,0.05f, 0, 0);
    }
}
