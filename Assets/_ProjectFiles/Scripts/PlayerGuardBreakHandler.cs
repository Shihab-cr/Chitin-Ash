using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerGuardBreakHandler : MonoBehaviour
{
    [SerializeField] private float guardBreakDuration = 1.2f;
    private PlayerActionCoordinator playerActionCoordinator;
    private Rigidbody playerRb;
    private Coroutine guardBreakCoroutine;
    private bool isGuardBroken;
    public bool IsGuardBroken => isGuardBroken;
    public UnityEvent OnGuardBreakStart;
    public UnityEvent OnGuardBreakEnd;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        playerActionCoordinator = GetComponent<PlayerActionCoordinator>();
    }
    void OnDisable()
    {

    }
    
    public void TriggerGuardBreak()
    {
        if (isGuardBroken) return;

        isGuardBroken = true;
        if (playerActionCoordinator != null) playerActionCoordinator.BeginAction(PlayerActionType.Stun, PlayerAbility.None);
        if(playerRb != null)
        {
            playerRb.linearVelocity = playerRb.linearVelocity.y * Vector3.up;
            playerRb.angularVelocity = Vector3.zero;
        }

        OnGuardBreakStart?.Invoke();

        if (guardBreakCoroutine != null) StopCoroutine(guardBreakCoroutine);
        guardBreakCoroutine = StartCoroutine(GuardBreakCoroutine());
    }
    public void EndGuardBreak()
    {
        if (!isGuardBroken) return;

        isGuardBroken = false;
        if(guardBreakCoroutine != null)
        {
            StopCoroutine(guardBreakCoroutine);
            guardBreakCoroutine = null;
        }
        if(playerActionCoordinator != null)
        {
            playerActionCoordinator.EndAction(PlayerActionType.Stun);
        }
        OnGuardBreakEnd?.Invoke();
        
    }
    public IEnumerator GuardBreakCoroutine()
    {
        yield return new WaitForSeconds(guardBreakDuration);
        guardBreakCoroutine = null;
        EndGuardBreak();
    }
}
