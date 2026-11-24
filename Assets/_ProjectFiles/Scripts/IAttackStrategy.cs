using UnityEngine;

public interface IAttackStrategy
{
    public void PerformAtk();
    public bool CanPerformAttack();
    public float GetDamage();
    public void OnEndAtk();
    public float GetStaminaCost();
    public PlayerAttackSO GetcurrAtkSO();
}
