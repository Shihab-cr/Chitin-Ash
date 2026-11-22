using System;
using UnityEngine;
using UnityEngine.Events;

public class HealthHandler : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    private float currHealth;
    public UnityEvent OnHit;
    public UnityEvent OnDeath;
    public UnityEvent OnHealed;
    public UnityEvent OnResetHealth;
    public UnityEvent OnMaxHealthUpgraded;
    public UnityEvent<float,float> OnHealthChanged;
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
    public float CurrHealth { get => currHealth;}
    bool isDead = false;
    private bool canTakeDmg = true;
    private void Start()
    {
        if (this.gameObject.CompareTag("Player")) { 
            currHealth = PlayerDataModel.LoadHealth();
            if (currHealth <= 0) currHealth = maxHealth;
        }
        else currHealth = maxHealth;
        OnHealthChanged?.Invoke(currHealth, maxHealth);
    }
    public void InitializeHealth(float health)
    {
        currHealth = health > 0 ? health : maxHealth;
        OnHealthChanged?.Invoke(currHealth,maxHealth);
    }
    public void TakeDmg(float dmg)
    {
        if (isDead) return;
        if(!canTakeDmg) return;

        currHealth = Mathf.Max(currHealth-dmg,0);
        OnHealthChanged?.Invoke(currHealth,maxHealth);

        if (currHealth <= 0)
        {
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
        if (this.gameObject.CompareTag("Player")) PlayerDataModel.SaveHealth(currHealth);
        OnHealed?.Invoke();
    }
    public void ResetHealth()
    {
        currHealth = maxHealth;
        isDead = false;
        if (this.gameObject.CompareTag("Player")) PlayerDataModel.SaveHealth(currHealth);
        OnResetHealth?.Invoke();
    }
    public void DisableTakingDamage()
    {
        canTakeDmg = false;
    }
    public void EnableTakingDamage()
    {
        canTakeDmg = true;
    }
    public void UpgradeMaxHealth(float newMaxHealth)
    {
        if (newMaxHealth < maxHealth) return;
        this.maxHealth = newMaxHealth;
        currHealth = newMaxHealth;
        OnHealthChanged?.Invoke(currHealth,maxHealth);
        OnMaxHealthUpgraded?.Invoke();
    }

}
