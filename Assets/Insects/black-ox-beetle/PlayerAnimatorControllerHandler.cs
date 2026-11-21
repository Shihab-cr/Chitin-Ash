using UnityEngine;

public class PlayerAnimatorControllerHandler : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float movementDampTime = 0.15f;

    [Header("Block Mask Layer Settings")]
    [SerializeField] private string blockLayerName = "mask1";
    [SerializeField] private float weightIncreaseSpeed = 5f;
    [SerializeField] private float weightDecreaseSpeed = 5f;
    [SerializeField] private float blockDamageDropSpeed = 25f;

    private static readonly int EatHash = Animator.StringToHash("eat");
    private static readonly int SleepHash = Animator.StringToHash("sleep");
    private static readonly int IdleHash = Animator.StringToHash("idle");
    private static readonly int WalkingHash = Animator.StringToHash("walking");
    private static readonly int HorizontalHash = Animator.StringToHash("Horizontal");
    private static readonly int VerticalHash = Animator.StringToHash("Vertical");

    private static readonly int BlockHash = Animator.StringToHash("block");
    private static readonly int StunHash = Animator.StringToHash("stun");
    private static readonly int LiteAttackHash = Animator.StringToHash("lite attack");
    private static readonly int LiteAttack2Hash = Animator.StringToHash("lite attack 2");
    private static readonly int SpecialAttackHash = Animator.StringToHash("Special attack");
    private static readonly int DeathHash = Animator.StringToHash("death");
    private static readonly int BlockDamageHash = Animator.StringToHash("block_damage");
    private static readonly int ParryHash = Animator.StringToHash("Parry");

    private static readonly int BlockDamageStateHash = Animator.StringToHash("damage while blocking");
    private static readonly int ParryStateHash = Animator.StringToHash("Parry");

    private int blockLayerIndex = -1;
    private float currentMaskWeight = 0f;
    private bool isBlockingInput = false;

    private bool isBlockDamageLocked = false;
    private bool hasEnteredBlockDamageState = false;

    private bool isParryLocked = false;
    private bool hasEnteredParryState = false;

    public float CurrentMaskWeight => currentMaskWeight;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        blockLayerIndex = animator.GetLayerIndex(blockLayerName);
        if (blockLayerIndex != -1)
        {
            animator.SetLayerWeight(blockLayerIndex, 0f);
        }
    }

    private void Update()
    {
        UpdateBlockMaskLayer();
    }

    public void SetMovement(float horizontal, float vertical)
    {
        bool isIdle = Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f);
        bool isWalking = !isIdle;

        animator.SetFloat(HorizontalHash, horizontal, movementDampTime, Time.deltaTime);
        animator.SetFloat(VerticalHash, vertical, movementDampTime, Time.deltaTime);
        animator.SetBool(IdleHash, isIdle);
        animator.SetBool(WalkingHash, isWalking);
    }

    public void SetBlocking(bool isBlocking)
    {
        isBlockingInput = isBlocking;
    }

    public void SetEat(bool isEating)
    {
        if (isEating) ResetAllActions();
        animator.SetBool(EatHash, isEating);
    }

    public void SetSleep(bool isSleeping)
    {
        if (isSleeping) ResetAllActions();
        animator.SetBool(SleepHash, isSleeping);
    }

    public void TriggerBlock()
    {
        animator.SetTrigger(BlockHash);
    }

    public void TriggerLiteAttack()
    {
        ResetAllActions();
        animator.SetTrigger(LiteAttackHash);
    }

    public void TriggerLiteAttack2()
    {
        ResetAllActions();
        animator.SetTrigger(LiteAttack2Hash);
    }

    public void TriggerSpecialAttack()
    {
        ResetAllActions();
        animator.SetTrigger(SpecialAttackHash);
    }

    public void TriggerStun()
    {
        ResetAllActions();
        animator.SetTrigger(StunHash);
    }

    public void TriggerDeath()
    {
        ResetAllActions();
        animator.SetTrigger(DeathHash);
    }

    public void TriggerBlockDamage()
    {
        if (isBlockDamageLocked) return;

        animator.SetTrigger(BlockDamageHash);
        isBlockDamageLocked = true;
        hasEnteredBlockDamageState = false;
    }

    public void TriggerParry()
    {
        if (isParryLocked) return;

        animator.SetTrigger(ParryHash);
        isParryLocked = true;
        hasEnteredParryState = false;
    }

    public void ResetAllActions()
    {
        animator.ResetTrigger(StunHash);
        animator.ResetTrigger(DeathHash);
        animator.ResetTrigger(LiteAttackHash);
        animator.ResetTrigger(LiteAttack2Hash);
        animator.ResetTrigger(SpecialAttackHash);
        animator.ResetTrigger(BlockDamageHash);
        animator.ResetTrigger(ParryHash);
        animator.ResetTrigger(BlockHash);

        animator.SetBool(SleepHash, false);
        animator.SetBool(EatHash, false);
    }

    public bool IsPlayingState(string stateName, int layerIndex = 0)
    {
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(layerIndex);
        AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(layerIndex);
        return currentState.IsName(stateName) || nextState.IsName(stateName);
    }

    private void UpdateBlockMaskLayer()
    {
        if (blockLayerIndex == -1) return;

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(0);

        bool isPlayingBlockDamage = currentState.shortNameHash == BlockDamageStateHash || nextState.shortNameHash == BlockDamageStateHash;
        bool isPlayingParry = currentState.shortNameHash == ParryStateHash || nextState.shortNameHash == ParryStateHash;

        if (isBlockDamageLocked)
        {
            if (isPlayingBlockDamage)
            {
                hasEnteredBlockDamageState = true;
            }
            else if (hasEnteredBlockDamageState)
            {
                isBlockDamageLocked = false;
                hasEnteredBlockDamageState = false;
            }
        }

        if (isParryLocked)
        {
            if (isPlayingParry)
            {
                hasEnteredParryState = true;
            }
            else if (hasEnteredParryState)
            {
                isParryLocked = false;
                hasEnteredParryState = false;
            }
        }

        if (isParryLocked || isPlayingParry)
        {
            currentMaskWeight = 0f;
        }
        else if (isBlockDamageLocked || isPlayingBlockDamage)
        {
            currentMaskWeight = Mathf.MoveTowards(currentMaskWeight, 0f, blockDamageDropSpeed * Time.deltaTime);
        }
        else if (isBlockingInput)
        {
            currentMaskWeight = Mathf.MoveTowards(currentMaskWeight, 1f, weightIncreaseSpeed * Time.deltaTime);
        }
        else
        {
            currentMaskWeight = Mathf.MoveTowards(currentMaskWeight, 0f, weightDecreaseSpeed * Time.deltaTime);
        }

        animator.SetLayerWeight(blockLayerIndex, currentMaskWeight);
    }
}