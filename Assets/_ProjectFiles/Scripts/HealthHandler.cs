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

    private void Start()
    {
        if (this.gameObject.CompareTag("Player")) { 
            currHealth = PlayerDataModel.LoadHealth();
            if (currHealth <= 0) currHealth = maxHealth;
        }
        else currHealth = maxHealth;
    }
    public void TakeDmg(float dmg)
    {
        if (isDead) return;
        currHealth -= dmg;
        if (currHealth <= 0)
        {
            currHealth = 0;
            isDead = true;
            if(this.gameObject.CompareTag("Player")) PlayerDataModel.SaveHealth(maxHealth);
            OnDeath?.Invoke();
        }
        else
        {
            if(this.gameObject.CompareTag("Player")) PlayerDataModel.SaveHealth(currHealth);
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
    
}
