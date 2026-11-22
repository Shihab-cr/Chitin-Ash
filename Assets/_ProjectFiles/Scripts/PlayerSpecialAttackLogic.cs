using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpecialAttackLogic : MonoBehaviour, IAttackStrategy
{
    [SerializeField] private PlayerAttackSO[] specialAttacks;
    private PlayerStaminaManager playerStaminaManager;
    private PlayerAttackAnimatorOverridor playerAnimatorOverridor;
    private HashSet<PlayerAttackSO> onCoolDownAttacks = new HashSet<PlayerAttackSO>();
    private int specialAtkIndex;
    private PlayerAttackSO currAtkSO;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerStaminaManager = GetComponent<PlayerStaminaManager>();
        playerAnimatorOverridor = GetComponent<PlayerAttackAnimatorOverridor>();
        if (specialAttacks != null && specialAttacks.Length > 0) currAtkSO = specialAttacks[0];
    }
    public void ChooseSpecialAtk(int index)
    {
        if(index>=0 && index < specialAttacks.Length)
        {
            specialAtkIndex = index;
            currAtkSO = specialAttacks[specialAtkIndex];

        }
        else
        {
            currAtkSO = null;
        }

    }
    public void PerformAtk()
    {
        if (specialAtkIndex >= specialAttacks.Length) return;

        if (specialAttacks[specialAtkIndex] == null || onCoolDownAttacks.Contains(specialAttacks[specialAtkIndex])) return;

        

        if (currAtkSO == null) return;

        onCoolDownAttacks.Add(currAtkSO);
        StartCoroutine(AtkCoolDownCoroutine());
        if (playerAnimatorOverridor != null) playerAnimatorOverridor.OverrideAttackAnimation(currAtkSO.attackAnimation);
    }
    public bool CanPerformAttack()
    {
        if (currAtkSO != null)
            return !onCoolDownAttacks.Contains(currAtkSO) && playerStaminaManager.HasEnoughStamina(currAtkSO.staminaCost);
        else
            return false;
    }
    public float GetDamage(){
        if (currAtkSO != null)
            return currAtkSO.dmg;
        else
            return 0;
    }
    public float GetStaminaCost()
    {
        if (currAtkSO != null)
            return currAtkSO.staminaCost;
        else 
            return 0;
    }
    public void OnEndAtk()
    {
        
    }
    public PlayerAttackSO GetcurrAtkSO()
    {
        return currAtkSO;
    }
    private IEnumerator AtkCoolDownCoroutine()
    {
        PlayerAttackSO frozenAttack= currAtkSO;
        yield return new WaitForSeconds(frozenAttack.attackAnimation.length);
        yield return new WaitForSeconds(frozenAttack.coolDownDuration);
        onCoolDownAttacks.Remove(frozenAttack);
    }
}
