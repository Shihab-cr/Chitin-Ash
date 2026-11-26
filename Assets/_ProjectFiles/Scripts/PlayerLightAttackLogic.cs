using System.Collections;
using UnityEngine;

public class PlayerLightAttackLogic : MonoBehaviour, IAttackStrategy
{
    [SerializeField] private PlayerAttackSO[] lightAttacks;
    [Tooltip("Post-swing grace window (in seconds) to continue the combo after returning to idle.")]
    [SerializeField] private float comboResetGraceWindow = 0.75f;
    [Tooltip("Short recovery pause (in seconds) after the final hit of the combo before starting a new combo.")]
    [SerializeField] private float finisherCooldown = 0.2f;
    [Tooltip("Playback speed multiplier for light attack animations (1 = normal, 1.25 = 25% faster).")]
    [SerializeField] private float attackSpeedMultiplier = 1.15f;

    private PlayerAttackSO currAtkSO;
    private PlayerAttackLogic playerAttackLogic;
    private PlayerAttackAnimatorOverridor playerAnimatorOverridor;
    private PlayerAnimatorControllerHandler playerAnimatorHandler;
    private PlayerStaminaManager playerStaminaManager;

    private Coroutine unlockFailsafeCoroutine;
    private Coroutine comboResetCoroutine;
    private Coroutine finisherCooldownCoroutine;

    private int lightAtkComboIndex = 0;
    private bool canBeUsed = true;
    private bool isFinisherLocked = false;
    private bool wasLastHitFinisher = false;

    private void Awake()
    {
        playerAttackLogic = GetComponent<PlayerAttackLogic>();
        playerAnimatorOverridor = GetComponent<PlayerAttackAnimatorOverridor>();
        playerAnimatorHandler = GetComponent<PlayerAnimatorControllerHandler>();
        playerStaminaManager = GetComponent<PlayerStaminaManager>();
    }

    public void PerformAtk()
    {
        if (!isActiveAndEnabled || lightAttacks == null || lightAttacks.Length == 0 || !canBeUsed || isFinisherLocked) return;

        currAtkSO = lightAttacks[lightAtkComboIndex];
        if (currAtkSO == null) return;

        canBeUsed = false;
        wasLastHitFinisher = (lightAtkComboIndex == lightAttacks.Length - 1);

        CancelComboResetTimer();

        if (playerAnimatorHandler != null)
        {
            playerAnimatorHandler.PrepareForOverriddenAttack();
            playerAnimatorHandler.SetFastSpeed(attackSpeedMultiplier);
        }

        if (playerAnimatorOverridor != null && currAtkSO.attackAnimation != null)
        {
            playerAnimatorOverridor.OverrideAttackAnimation(currAtkSO.attackAnimation);
        }

        // Advance to the next combo slot for the following swing
        lightAtkComboIndex = (lightAtkComboIndex + 1) % lightAttacks.Length;

        if (unlockFailsafeCoroutine != null) StopCoroutine(unlockFailsafeCoroutine);
        unlockFailsafeCoroutine = StartCoroutine(UnlockFailsafeCoroutine(currAtkSO));
    }

    private IEnumerator UnlockFailsafeCoroutine(PlayerAttackSO executedAtkSO)
    {
        float speed = Mathf.Max(0.1f, attackSpeedMultiplier);
        float animLength = executedAtkSO.attackAnimation != null ? (executedAtkSO.attackAnimation.length / speed) : 0.4f;
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

    private IEnumerator ComboResetCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        lightAtkComboIndex = 0;
        comboResetCoroutine = null;
    }

    private IEnumerator FinisherCooldownCoroutine(float duration)
    {
        isFinisherLocked = true;
        yield return new WaitForSeconds(duration);
        isFinisherLocked = false;
        finisherCooldownCoroutine = null;
    }

    public void CancelComboResetTimer()
    {
        if (comboResetCoroutine != null)
        {
            StopCoroutine(comboResetCoroutine);
            comboResetCoroutine = null;
        }
    }

    public bool HasStaminaForNextAttack()
    {
        if (lightAttacks == null || lightAttacks.Length == 0) return false;
        PlayerAttackSO nextAtk = lightAttacks[lightAtkComboIndex];
        if (nextAtk == null) return false;

        return playerStaminaManager == null || playerStaminaManager.HasEnoughStamina(nextAtk.staminaCost);
    }

    public bool CanPerformAttack()
    {
        return canBeUsed && !isFinisherLocked && HasStaminaForNextAttack();
    }

    public float GetStaminaCost()
    {
        if (lightAttacks == null || lightAttacks.Length == 0) return 0f;
        PlayerAttackSO targetSO = (!canBeUsed && currAtkSO != null) ? currAtkSO : lightAttacks[lightAtkComboIndex];
        return targetSO != null ? targetSO.staminaCost : 0f;
    }

    public float GetDamage()
    {
        if (currAtkSO != null) return currAtkSO.dmg;
        if (lightAttacks != null && lightAttacks.Length > 0 && lightAttacks[0] != null) return lightAttacks[0].dmg;
        return 0f;
    }

    public void OnEndAtk()
    {
        // If the Player GameObject is being disabled/deactivated, reset without starting coroutines
        if (!isActiveAndEnabled)
        {
            ResetCombo();
            return;
        }

        canBeUsed = true;
        playerAnimatorHandler?.ResetNormalSpeed();

        if (unlockFailsafeCoroutine != null)
        {
            StopCoroutine(unlockFailsafeCoroutine);
            unlockFailsafeCoroutine = null;
        }

        CancelComboResetTimer();

        if (wasLastHitFinisher)
        {
            // Combo just finished: reset immediately to index 0 and apply a short, controllable finisher pause
            lightAtkComboIndex = 0;
            wasLastHitFinisher = false;

            if (finisherCooldown > 0f)
            {
                if (finisherCooldownCoroutine != null) StopCoroutine(finisherCooldownCoroutine);
                finisherCooldownCoroutine = StartCoroutine(FinisherCooldownCoroutine(finisherCooldown));
            }
        }
        else
        {
            float cooldown = currAtkSO != null ? currAtkSO.coolDownDuration : 0f;
            comboResetCoroutine = StartCoroutine(ComboResetCoroutine(cooldown + comboResetGraceWindow));
        }
    }

    public void ResetCombo()
    {
        if (unlockFailsafeCoroutine != null)
        {
            StopCoroutine(unlockFailsafeCoroutine);
            unlockFailsafeCoroutine = null;
        }

        if (finisherCooldownCoroutine != null)
        {
            StopCoroutine(finisherCooldownCoroutine);
            finisherCooldownCoroutine = null;
        }

        CancelComboResetTimer();
        playerAnimatorHandler?.ResetNormalSpeed();
        lightAtkComboIndex = 0;
        canBeUsed = true;
        isFinisherLocked = false;
        wasLastHitFinisher = false;
    }

    public PlayerAttackSO GetcurrAtkSO()
    {
        if (!canBeUsed && currAtkSO != null) return currAtkSO;
        if (lightAttacks != null && lightAttacks.Length > 0) return lightAttacks[lightAtkComboIndex];
        return null;
    }
}