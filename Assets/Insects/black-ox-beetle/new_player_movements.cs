using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerAnimatorControllerHandler))]
public class new_player_movements : MonoBehaviour
{
    [SerializeField] private PlayerAnimatorControllerHandler animHandler;
    private bool isSprinting = false;
    private void Awake()
    {
        if (animHandler == null)
            animHandler = GetComponent<PlayerAnimatorControllerHandler>();
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        HandleActions(keyboard);
        HandleBlockAndParry(keyboard);
        HandleMovement(keyboard);
    }

    private void HandleActions(Keyboard keyboard)
    {
        /*if (keyboard.tKey.wasPressedThisFrame)
        {
            animHandler.TriggerStun();
        }
        else if (keyboard.uKey.wasPressedThisFrame)
        {
            animHandler.TriggerDeath();
        }
        else if (keyboard.iKey.wasPressedThisFrame)
        {
            animHandler.TriggerLiteAttack();
        }
        else if (keyboard.oKey.wasPressedThisFrame)
        {
            animHandler.TriggerLiteAttack2();
        }
        else if (keyboard.pKey.wasPressedThisFrame)
        {
            animHandler.TriggerSpecialAttack();
        }
        else if (keyboard.gKey.wasPressedThisFrame)
        {
            animHandler.SetSleep(true);
        }
        else if (keyboard.hKey.wasPressedThisFrame)
        {
            animHandler.SetEat(true);
        }*/
    }

    private void HandleBlockAndParry(Keyboard keyboard)
    {
        /*if (keyboard.jKey.wasPressedThisFrame)
        {
            animHandler.TriggerBlockDamage();
        }

        if (keyboard.kKey.wasPressedThisFrame)
        {
            animHandler.TriggerParry();
        }

        bool isCtrlHeld = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
        animHandler.SetBlocking(isCtrlHeld);*/
    }

    private void HandleMovement(Keyboard keyboard)
    {
        

        bool a = keyboard.aKey.isPressed;
        bool d = keyboard.dKey.isPressed;
        bool s = keyboard.sKey.isPressed;
        bool w = keyboard.wKey.isPressed;

        float finalH = 0f;
        if (a && !d)
            finalH = isSprinting ? -2f : -1f;
        else if (d && !a)
            finalH = isSprinting ? 1f : 2f;

        float finalV = 0f;
        if (s && !w)
            finalV = isSprinting ? -2f : -1f;
        else if (w && !s)
            finalV = isSprinting ? 1f : 2f;

        animHandler.SetMovement(finalH, finalV);
    }
    public void SetIsSprinting(bool sprintingStatus)
    {
        isSprinting = sprintingStatus;
    }
}