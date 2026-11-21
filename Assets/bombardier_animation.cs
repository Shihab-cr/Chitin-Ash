using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class bombardier_animation : MonoBehaviour
{
    private Animator anim;
    private bool isDead = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            anim.SetBool("eat", !anim.GetBool("eat"));
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            anim.SetTrigger("attack_aoe");
        }

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            anim.SetTrigger("attack_bite");
        }

        if (Keyboard.current.yKey.wasPressedThisFrame)
        {
            isDead = true;
            anim.SetBool("death", true);
            StartCoroutine(WaitForDeathAnimation());
        }

        if (Keyboard.current.uKey.wasPressedThisFrame)
        {
            anim.SetBool("sleep", !anim.GetBool("sleep"));
        }

        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            anim.SetBool("walk", !anim.GetBool("walk"));
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            anim.SetBool("idel", !anim.GetBool("idel"));
        }
    }

    private IEnumerator WaitForDeathAnimation()
    {
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Death_NC_01_SK_Bombardier01"))
        {
            yield return null;
        }

        while (anim.GetCurrentAnimatorStateInfo(0).IsName("Death_NC_01_SK_Bombardier01") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        enabled = false;
    }
}