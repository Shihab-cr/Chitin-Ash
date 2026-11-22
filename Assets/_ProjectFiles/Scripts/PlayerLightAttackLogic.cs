using UnityEngine;
using System.Collections;
public class PlayerLightAttackLogic : MonoBehaviour, IAttackStrategy
{
    [SerializeField] private PlayerAttackSO[] lightAttacks;
    private PlayerAttackSO currAtkSO;
    private PlayerAttackAnimatorOverridor playerAnimatorOverridor;
    private PlayerStaminaManager playerStaminaManager;
    private Coroutine lightAtkComboTimeOutCoroutine;
    private int lightAtkComboIndex = 0;
    private bool canBeUsed = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerAnimatorOverridor = GetComponent<PlayerAttackAnimatorOverridor>();
        playerStaminaManager = GetComponent<PlayerStaminaManager>();
    }
    public void PerformAtk()
    {
        if (lightAttacks.Length <= 0 || !canBeUsed) return;

       currAtkSO = lightAttacks[lightAtkComboIndex];

        canBeUsed = false;
        if (lightAtkComboTimeOutCoroutine != null) StopCoroutine(lightAtkComboTimeOutCoroutine);
        lightAtkComboTimeOutCoroutine = null;

        if (lightAtkComboTimeOutCoroutine == null) lightAtkComboTimeOutCoroutine = StartCoroutine(LightAtkComboCoroutine(currAtkSO));
        if (playerAnimatorOverridor != null) playerAnimatorOverridor.OverrideAttackAnimation(currAtkSO.attackAnimation);
        //play the animation in the controller
        lightAtkComboIndex = (lightAtkComboIndex + 1) % lightAttacks.Length;
    }
    private IEnumerator LightAtkComboCoroutine(PlayerAttackSO lightAtkSO)
    {
        yield return new WaitForSeconds(lightAtkSO.attackAnimation.length + lightAtkSO.coolDownDuration);
        lightAtkComboIndex = 0;
        lightAtkComboTimeOutCoroutine = null;
    }
    public bool CanPerformAttack()
    {
        if (lightAttacks == null || lightAttacks.Length <= 0) return false;
        return canBeUsed && playerStaminaManager.HasEnoughStamina(lightAttacks[lightAtkComboIndex].staminaCost);
    }
    public float GetStaminaCost()
    {
        return lightAttacks[lightAtkComboIndex].staminaCost;
    }
    public float GetDamage()
    {
        return currAtkSO.dmg;
    }
    public void OnEndAtk()
    {
        canBeUsed = true;
    }
    public PlayerAttackSO GetcurrAtkSO()
    {
        return lightAttacks[lightAtkComboIndex];
    }
}
