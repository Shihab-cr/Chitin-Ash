using System;
using UnityEngine;

[Flags]
public enum PlayerAbility
{
    None = 0,
    Move = 1<<0,
    Rotate = 1<<1,
    Dodge = 1<<2,
    Block = 1<<3,
    Parry = 1<<4,
    Attack = 1<<5,
    Eat = 1<<6,
    Sprint = 1<<7,
    All = ~0
}
public enum PlayerActionType{
    Dodge,
    Block,
    Parry,
    Attack,
    Eat,
    Stun,
    Sprint

}
public class PlayerActionCoordinator : MonoBehaviour
{
    private PlayerAbility currAllowedAbilities = PlayerAbility.All;
    private PlayerAbility[] activeRestirctions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        int count = Enum.GetValues(typeof(PlayerActionType)).Length;
        activeRestirctions = new PlayerAbility[count];
        for (int i = 0; i < count; i++)
        {
            activeRestirctions[i] = PlayerAbility.All;
        }
    }
    public bool CanPerformAction(PlayerAbility playerAbility)
    {
        return (currAllowedAbilities & playerAbility) == playerAbility;
    }
    public void BeginAction(PlayerActionType actionType, PlayerAbility allowedDuringAction)
    {
        activeRestirctions[(int)actionType] = allowedDuringAction;
        RecalculateCapabilities();
    }
    public void EndAction(PlayerActionType action)
    {
        activeRestirctions[(int)action] = PlayerAbility.All;
        RecalculateCapabilities();
    }
    public void RecalculateCapabilities(){
        PlayerAbility combined = PlayerAbility.All;
        for(int i = 0; i < activeRestirctions.Length; i++)
        {
            combined &= activeRestirctions[i];
        }
        currAllowedAbilities = combined;
    }
     
}
