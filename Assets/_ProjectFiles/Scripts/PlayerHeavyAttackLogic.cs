using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public struct HeavyAtkTierStruct
{
    public int tierNum;
    public AudioClip audioClip;
    public AnimationClip animationClip;
    public ParticleSystem[] atkVFX;
    public Vector2 tierTimeSlot;
}

public class PlayerHeavyAttackLogic : MonoBehaviour, IAttackStrategy
{
    [SerializeField] private PlayerAttackSO heavyAttack;
    [Tooltip("Animation clip played while holding/charging the heavy attack.")]
    [SerializeField] private AnimationClip chargeAnimationClip;
    [SerializeField] private float dmgDurationMultiplier = 1.2f;
    [SerializeField] private float maxChargeDuration = 2f;
    [SerializeField] private HeavyAtkTierStruct[] heavyAtkTiers;
    [SerializeField] private float stamPerCharge = 1f;

    private HeavyAtkTierStruct currHeavyAtkTier;
    private PlayerAttackAnimatorOverridor playerAnimatorOverridor;
    private PlayerAnimatorControllerHandler playerAnimatorHandler;
    private PlayerAttackLogic playerAttackLogic;
    private PlayerStaminaManager playerStaminaManager;

    private Coroutine heavyAtkChargeCoroutine;
    private Coroutine atkCooldownCoroutine;
    private Coroutine unlockFailsafeCoroutine;

    private float chargeTimer;
    private int chargeStartFrame = -1;
    private bool canBeUsed = true;
    private bool isCharging = false;

    public bool IsCharging => isCharging;

    private void Awake()
    {
        playerAttackLogic = GetComponent<PlayerAttackLogic>();
        playerStaminaManager = GetComponent<PlayerStaminaManager>();
        playerAnimatorOverridor = GetComponent<PlayerAttackAnimatorOverridor>();
        playerAnimatorHandler = GetComponent<PlayerAnimatorControllerHandler>();
    }

    private void OnDisable()
    {
        CancelCharge();
        StopAttackCoroutines();
        canBeUsed = true;
    }

    public void ChargeHeavyAtk(InputAction.CallbackContext inputContext)
    {
        if (!isActiveAndEnabled) return;

        if (inputContext.performed)
        {
            CancelCharge();

            chargeTimer = 0f;
            chargeStartFrame = Time.frameCount;
            isCharging = true;

            if (heavyAtkTiers != null && heavyAtkTiers.Length > 0)
            {
                currHeavyAtkTier = heavyAtkTiers[0];
            }

            if (playerAnimatorHandler != null)
            {
                playerAnimatorHandler.PrepareForOverriddenAttack();
            }

            if (chargeAnimationClip == null)
            {
                Debug.LogWarning($"[{nameof(PlayerHeavyAttackLogic)}] Charge Animation Clip is not assigned in the Inspector on {name}.", this);
            }
            else if (playerAnimatorOverridor != null)
            {
                playerAnimatorOverridor.OverrideAttackAnimation(chargeAnimationClip);
            }

            heavyAtkChargeCoroutine = StartCoroutine(ChargeHeavyAtkCoroutine());
        }
        else if (inputContext.canceled)
        {
            CancelCharge();
        }
    }

    public void CancelCharge()
    {
        if (heavyAtkChargeCoroutine != null)
        {
            StopCoroutine(heavyAtkChargeCoroutine);
            heavyAtkChargeCoroutine = null;
        }
        isCharging = false;
    }

    public bool HasReachedMinCharge()
    {
        // Ignore same-frame canceled callbacks caused by Tap or Release-Only interactions
        if (Time.frameCount <= chargeStartFrame) return false;

        if (heavyAtkTiers == null || heavyAtkTiers.Length == 0) return chargeTimer >= 0.2f;
        return chargeTimer >= heavyAtkTiers[0].tierTimeSlot.x;
    }

    public void PerformAtk()
    {
        if (!isActiveAndEnabled || !canBeUsed || heavyAttack == null) return;

        CancelCharge();
        canBeUsed = false;

        AnimationClip currClip = heavyAttack.attackAnimation;
        if (heavyAtkTiers != null && heavyAtkTiers.Length > 0 && currHeavyAtkTier.animationClip != null)
        {
            currClip = currHeavyAtkTier.animationClip;
        }

        if (playerAnimatorHandler != null)
        {
            playerAnimatorHandler.PrepareForOverriddenAttack();
        }

        // Force-exits the charge animation clip and crossfades directly into the charged attack clip
        if (playerAnimatorOverridor != null && currClip != null)
        {
            playerAnimatorOverridor.OverrideAttackAnimation(currClip);
        }

        StopAttackCoroutines();
        unlockFailsafeCoroutine = StartCoroutine(UnlockFailsafeCoroutine(currClip));
    }

    private IEnumerator UnlockFailsafeCoroutine(AnimationClip atkClip)
    {
        float animLength = atkClip != null ? atkClip.length : 0.5f;
        yield return new WaitForSeconds(animLength + 0.05f);

        if (!canBeUsed)
        {
            if (playerAttackLogic != null)
                playerAttackLogic.EndAtk();
            else
                OnEndAtk();
        }

        unlockFailsafeCoroutine = null;
    }

    private IEnumerator PostSwingCooldownCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        canBeUsed = true;
        atkCooldownCoroutine = null;
    }

    private void StopAttackCoroutines()
    {
        if (unlockFailsafeCoroutine != null)
        {
            StopCoroutine(unlockFailsafeCoroutine);
            unlockFailsafeCoroutine = null;
        }

        if (atkCooldownCoroutine != null)
        {
            StopCoroutine(atkCooldownCoroutine);
            atkCooldownCoroutine = null;
        }
    }

    private IEnumerator ChargeHeavyAtkCoroutine()
    {
        int currTierIndex = 0;
        bool hasTiers = heavyAtkTiers != null && heavyAtkTiers.Length > 0;
        if (hasTiers)
        {
            currHeavyAtkTier = heavyAtkTiers[currTierIndex];
        }

        // Guard against an uninitialized 0 value in the Inspector auto-firing on frame 1
        float effectiveMaxCharge = Mathf.Max(0.25f, maxChargeDuration);
        float totalStamSpent = 0f;

        while (true)
        {
            chargeTimer += Time.deltaTime;

            bool hasStamina = playerStaminaManager == null || playerStaminaManager.HasEnoughStamina(stamPerCharge * Time.deltaTime);
            if (hasStamina)
            {
                if (playerStaminaManager != null && heavyAttack != null && totalStamSpent < heavyAttack.staminaCost)
                {
                    float drain = stamPerCharge * Time.deltaTime;
                    playerStaminaManager.UseStamina(drain);
                    totalStamSpent += drain;
                }
            }
            else
            {
                float minTierTime = hasTiers ? heavyAtkTiers[0].tierTimeSlot.x : 0.2f;
                isCharging = false;
                heavyAtkChargeCoroutine = null;

                if (chargeTimer >= minTierTime)
                {
                    if (playerAttackLogic != null) playerAttackLogic.ExecuteAttackLogic();
                }
                else
                {
                    if (playerAttackLogic != null) playerAttackLogic.EndAtk();
                }
                yield break;
            }

            if (hasTiers && chargeTimer >= currHeavyAtkTier.tierTimeSlot.y)
            {
                currTierIndex = Mathf.Min(currTierIndex + 1, heavyAtkTiers.Length - 1);
                currHeavyAtkTier = heavyAtkTiers[currTierIndex];
            }

            if (chargeTimer >= effectiveMaxCharge)
            {
                isCharging = false;
                heavyAtkChargeCoroutine = null;
                if (playerAttackLogic != null) playerAttackLogic.ExecuteAttackLogic();
                yield break;
            }

            yield return null;
        }
    }

    private float CalculateBonusDmg()
    {
        return dmgDurationMultiplier * chargeTimer;
    }

    public void OnEndAtk()
    {
        CancelCharge();
        chargeTimer = 0f;
        chargeStartFrame = -1;

        if (unlockFailsafeCoroutine != null)
        {
            StopCoroutine(unlockFailsafeCoroutine);
            unlockFailsafeCoroutine = null;
        }

        if (!isActiveAndEnabled)
        {
            canBeUsed = true;
            return;
        }

        float cooldown = heavyAttack != null ? heavyAttack.coolDownDuration : 0f;
        if (cooldown > 0f)
        {
            if (atkCooldownCoroutine != null) StopCoroutine(atkCooldownCoroutine);
            atkCooldownCoroutine = StartCoroutine(PostSwingCooldownCoroutine(cooldown));
        }
        else
        {
            canBeUsed = true;
        }
    }

    public float GetStaminaCost()
    {
        return 0f;
    }

    public bool CanPerformAttack()
    {
        if (!isActiveAndEnabled || !canBeUsed || heavyAttack == null) return false;

        float minStaminaCharge = heavyAttack.staminaCost;
        if (heavyAtkTiers != null && heavyAtkTiers.Length > 0)
        {
            minStaminaCharge = stamPerCharge * heavyAtkTiers[0].tierTimeSlot.x;
        }

        return playerStaminaManager == null || playerStaminaManager.HasEnoughStamina(minStaminaCharge);
    }

    public float GetDamage()
    {
        float baseDmg = heavyAttack != null ? heavyAttack.dmg : 0f;
        return baseDmg + CalculateBonusDmg();
    }

    public PlayerAttackSO GetcurrAtkSO()
    {
        return heavyAttack;
    }

    public void PlayCurrentTierAudio()
    {
        // Play currHeavyAtkTier.audioClip
    }

    public void PlayCurrentTierVFX()
    {
        // Play currHeavyAtkTier.atkVFX
    }
}