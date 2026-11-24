using UnityEngine;

public class ParryStateBehaviour : StateMachineBehaviour
{
    private PlayerParryLogic playerParryLogic;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playerParryLogic == null)
        {
            playerParryLogic = animator.GetComponentInParent<PlayerParryLogic>();
        }

        if (playerParryLogic != null)
        {
            playerParryLogic.NotifyParryStateEntered();
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playerParryLogic != null)
        {
            playerParryLogic.NotifyParryStateExited();
        }
    }
}