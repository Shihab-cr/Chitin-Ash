using System;

/// <summary>
/// Small Event Bus for progression. Gameplay scripts (enemies, consume logic, aphid pickups, ...)
/// just call ProgressionEvents.ReportXxx(...) and don't need to know about quests or skills.
///
/// Examples for teammates:
///   ProgressionEvents.ReportEnemyDefeated("BlackAnt");      // in the enemy's OnDeath
///   ProgressionEvents.ReportEnemyConsumed("Snail");          // in the Press-E consume logic
///   ProgressionEvents.ReportEliteDefeated("BullAnt");        // when an elite arena is won
///   ProgressionEvents.ReportRedAphidsCollected(1);           // in the red aphid pickup
///
/// Rule: whoever subscribes MUST unsubscribe in OnDisable/OnDestroy (static events survive scene loads).
/// </summary>
public static class ProgressionEvents
{
    public static event Action<string> OnEnemyDefeated;
    public static event Action<string> OnEnemyConsumed;
    public static event Action<string> OnEliteDefeated;
    public static event Action<int> OnRedAphidsCollected;
    public static event Action<string> OnCheckpointRested;

    public static void ReportEnemyDefeated(string enemyId)
    {
        OnEnemyDefeated?.Invoke(enemyId);
    }

    public static void ReportEnemyConsumed(string enemyId)
    {
        OnEnemyConsumed?.Invoke(enemyId);
    }

    public static void ReportEliteDefeated(string eliteId)
    {
        OnEliteDefeated?.Invoke(eliteId);
    }

    public static void ReportRedAphidsCollected(int amount)
    {
        if (amount <= 0) return;
        OnRedAphidsCollected?.Invoke(amount);
    }

    public static void ReportCheckpointRested(string checkpointId)
    {
        OnCheckpointRested?.Invoke(checkpointId);
    }
}
