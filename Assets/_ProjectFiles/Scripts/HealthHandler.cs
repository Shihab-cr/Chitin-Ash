using System;
using UnityEngine;

public class HealthHandler : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currHealth;
    public event Action OnHit;
    public event Action OnDeath;
    public event Action OnHealed;
    public event Action OnResetHealth;
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
    public float CurrHealth { get => currHealth;}
    bool isDead = false;
    public void TakeDmg(float dmg)
    {
        if (isDead) return;
        currHealth -= dmg;
        if (currHealth <= 0)
        {
            currHealth = 0;
            isDead = true;
            OnDeath?.Invoke();
        }
        else
        {
            OnHit?.Invoke();
        }
    }
    public void Heal(float heal)
    {
        if (isDead) return;
        currHealth = Mathf.Min(currHealth+heal,maxHealth);
        OnHealed?.Invoke();
    }
    public void ResetHealth()
    {
        currHealth = maxHealth;
        isDead = false;
        OnResetHealth?.Invoke();
    }
    
}
