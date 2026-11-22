using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttackSO", menuName = "Scriptable Objects/PlayerAttackSO")]
public class PlayerAttackSO : ScriptableObject
{
    public float dmg;
    public float staminaCost;
    public AnimationClip attackAnimation;

    public float damageRadius;
    public float coolDownDuration;
    public ParticleSystem attackVFX;
    public AudioClip attackSFX;

}
