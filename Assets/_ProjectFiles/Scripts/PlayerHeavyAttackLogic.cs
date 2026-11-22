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
    [SerializeField] private float dmgDurationMultiplier = 1.2f;
    [SerializeField] private float maxChargeDuration = 2f;
    [SerializeField] private HeavyAtkTierStruct[] heavyAtkTiers;
    [SerializeField] private float stamPerCharge= 1;
    private HeavyAtkTierStruct currHeavyAtkTier;
    private PlayerAttackAnimatorOverridor playerAnimatorOverridor;
    private PlayerAttackLogic playerAttackLogic;
    private PlayerStaminaManager playerStaminaManager;
    private Coroutine heavyAtkChargeCoroutine;
    private float chargeTimer;
    private bool canBeUsed=true;
    private bool isCharging = false;
    public bool IsCharging { get => isCharging; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerAttackLogic = GetComponent<PlayerAttackLogic>();
        playerStaminaManager = GetComponent<PlayerStaminaManager>();
        playerAnimatorOverridor = GetComponent<PlayerAttackAnimatorOverridor>();
    }
    public void ChargeHeavyAtk(InputAction.CallbackContext inputContext)
    {
        if (inputContext.performed)
        {
            if (heavyAtkChargeCoroutine != null) StopCoroutine(heavyAtkChargeCoroutine);
            heavyAtkChargeCoroutine = StartCoroutine(ChargeHeavyAtkCoroutine());
            isCharging = true;
            //go to charging animation
        }
        else if (inputContext.canceled)
        {
            if (heavyAtkChargeCoroutine != null) StopCoroutine(heavyAtkChargeCoroutine);
            heavyAtkChargeCoroutine = null;
            isCharging = false;
        }
    }
    public void PerformAtk()
    {
        if (!canBeUsed || heavyAttack == null) return;


        canBeUsed = false;

        AnimationClip currClip = heavyAttack.attackAnimation;
        if (currHeavyAtkTier.animationClip != null && heavyAtkTiers.Length > 0) currClip = currHeavyAtkTier.animationClip;
        if (playerAnimatorOverridor != null) playerAnimatorOverridor.OverrideAttackAnimation(currClip);
        StartCoroutine(AtkCoolDownCoroutine(currClip));
        //play heavy atk animation in controller
    }

    private IEnumerator AtkCoolDownCoroutine(AnimationClip atkClip)
    {
        yield return new WaitForSeconds(atkClip.length);
        yield return new WaitForSeconds(heavyAttack.coolDownDuration);
        canBeUsed = true;
    }
    private IEnumerator ChargeHeavyAtkCoroutine()
    {
        int currTierIndex = 0;
        currHeavyAtkTier = heavyAtkTiers[currTierIndex];
        float totalStamSpent = 0f;
        while (true)
        {
            chargeTimer += Time.deltaTime;
            if (playerStaminaManager.HasEnoughStamina(stamPerCharge*Time.deltaTime))
            {
                if (totalStamSpent < heavyAttack.staminaCost)
                {

                    playerStaminaManager.UseStamina(stamPerCharge * Time.deltaTime);
                    totalStamSpent += stamPerCharge * Time.deltaTime;
                }
            }
            else
            {
                if (chargeTimer >= heavyAtkTiers[0].tierTimeSlot.x)
                {
                    if (playerAttackLogic != null) playerAttackLogic.ExecuteAttackLogic();
                    isCharging = false;
                    break;
                }
                else
                {
                    if (playerAttackLogic != null) playerAttackLogic.EndAtk();
                    isCharging = false;
                    break;
                }
            }
            if (chargeTimer >= currHeavyAtkTier.tierTimeSlot.y)
            {
                currTierIndex = Mathf.Min(currTierIndex + 1, heavyAtkTiers.Length - 1);
                currHeavyAtkTier = heavyAtkTiers[currTierIndex];
            }
            if (currTierIndex >= heavyAtkTiers.Length - 1 && chargeTimer >= maxChargeDuration)
            {
                if (playerAttackLogic != null) playerAttackLogic.ExecuteAttackLogic();
                isCharging = false;
                break;
            }
            yield return null;
        }
    }
    private float CalculateBonusDmg()
    {
        float chargeTimeDmg = 0f;
        chargeTimeDmg = dmgDurationMultiplier * chargeTimer;
        return chargeTimeDmg;
    }
    public void OnEndAtk()
    {
        chargeTimer = 0f;
        isCharging = false;
        
    }
    public float GetStaminaCost()
    {
        return 0;
    }
    public bool CanPerformAttack()
    {
        float minStaminaCharge = heavyAttack.staminaCost;
        if (heavyAtkTiers != null && heavyAtkTiers.Length>0 ) minStaminaCharge = stamPerCharge * heavyAtkTiers[0].tierTimeSlot.x;
        return canBeUsed && playerStaminaManager.HasEnoughStamina(minStaminaCharge);
    }
    public float GetDamage()
    {
        return heavyAttack.dmg + CalculateBonusDmg();
    }
    public PlayerAttackSO GetcurrAtkSO()
    {
        return heavyAttack;
    }

    //audio and visuals should be played by the animation itself
    public void PlayCurrentTierAudio()
    {
        //play tier audio
    }
    public void PlayCurrentTierVFX()
    {
        //Play tier vfx
    }

}
