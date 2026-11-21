using System;
using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    public event Action OnAnimationStart;
    public event Action OnAnimationEnd;

    public void StartAnimationEvent()
    {
        OnAnimationStart?.Invoke();
    }

    public void EndAnimationEvent()
    {
        OnAnimationEnd?.Invoke();
    }
}