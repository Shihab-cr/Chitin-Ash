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
    public event Action<float> OnHealthChanged;
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
    }
    public void InitializeHealth(float health)
    {
        currHealth = health > 0 ? health : maxHealth;
        OnHealthChanged?.Invoke(currHealth);
    }
    public void TakeDmg(float dmg)
    {
        if (isDead) return;
        if(!canTakeDmg) return;

        currHealth = Mathf.Max(currHealth-dmg,0);
        OnHealthChanged?.Invoke(currHealth);

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

}
