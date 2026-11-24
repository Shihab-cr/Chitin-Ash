using UnityEngine;
using UnityEngine.Events;

public class PlayerStaminaManager : MonoBehaviour
{
    public UnityEvent<float,float> OnStaminaChanged;
    public UnityEvent<float> OnStaminaUpgraded;
    public UnityEvent OnInsufficientStamina;

    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regainSpeed = 45f;
    [SerializeField] private float regainDelay = 0.5f;
    private float currentStamina;
    public float CurrentStamina => currentStamina;
    private bool canRegainStamina = true;
    private float timeSinceLastRegain = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentStamina = maxStamina;
        OnStaminaChanged?.Invoke(currentStamina,maxStamina);
    }
    private void Update()
    {
        RegainStamina();
    }
    public bool UseStamina(float amount) {
        if(currentStamina >= amount) {
            currentStamina -= amount;
            timeSinceLastRegain = 0f;
            OnStaminaChanged?.Invoke(currentStamina,maxStamina);
            return true;
        }
        OnInsufficientStamina?.Invoke();
        Debug.Log("Not enough stamina!");
        return false;
    }
    public void RegainStamina() {
        if(!canRegainStamina) return;
        if(currentStamina >= maxStamina) return;
        if (timeSinceLastRegain < regainDelay)
        {
            timeSinceLastRegain += Time.deltaTime;
            return;
        }
        
        currentStamina += Time.deltaTime * regainSpeed;
        currentStamina = Mathf.Min(currentStamina, maxStamina);
        OnStaminaChanged?.Invoke(currentStamina,maxStamina);
        
    }
    public void UpgradeStamina(float amount) {
        float oldMaxStamina = maxStamina;
        maxStamina += amount;
        currentStamina = maxStamina;
        float upgradeRatio = maxStamina / oldMaxStamina;
        OnStaminaUpgraded?.Invoke(upgradeRatio);
        OnStaminaChanged?.Invoke(currentStamina,maxStamina);
    }
    public void DisableStaminaRegain()
    {
               canRegainStamina = false;
    }
    public void EnableStaminaRegain()
    {
               canRegainStamina = true;
    }
    public bool HasEnoughStamina(float amount)
    {
        return currentStamina >= amount;
    }
}
