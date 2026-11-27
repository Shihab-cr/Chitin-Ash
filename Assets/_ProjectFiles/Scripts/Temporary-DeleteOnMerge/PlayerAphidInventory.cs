using System;
using UnityEngine;

public class PlayerAphidInventory : MonoBehaviour
{
    [SerializeField] private int redAphidCount = 0;

    public int RedAphidCount => redAphidCount;
    public event Action<int> OnRedAphidCountChanged;

    public void AddRedAphid(int amount = 1)
    {
        if (amount <= 0) return;

        redAphidCount += amount;
        OnRedAphidCountChanged?.Invoke(redAphidCount);
        Debug.Log($"Red Aphids collected: {redAphidCount}");
    }
}