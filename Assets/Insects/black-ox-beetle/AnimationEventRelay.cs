using System;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventRelay : MonoBehaviour
{
    public UnityEvent OnDoDamageEvent;
    public UnityEvent OnEndAttack;
    public UnityEvent OnParryEvent;
    public UnityEvent OnEndParry;
    public UnityEvent OnEndEating;
    public UnityEvent OnEndStun;
    public UnityEvent OnEndAnimation;
    public void EndAnimation()
    {
        OnEndAnimation?.Invoke();
    }
   
    public void EndAttack()
    {
        OnEndAttack?.Invoke();
    }

   
    public void DoDamageEvent()
    {
        OnDoDamageEvent?.Invoke();
    }
    public void EndParry()
    {
        OnEndParry?.Invoke();
    }
    public void DoParryEvent()
    {
        OnParryEvent?.Invoke();
    }
    public void EndEating()
    {
        OnEndEating?.Invoke();
    }
    public void EndStun()
    {
        OnEndStun?.Invoke();
    }
}