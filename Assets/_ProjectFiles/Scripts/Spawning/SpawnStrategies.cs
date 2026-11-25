using System;
using UnityEngine;

namespace Game.Spawning
{
    public interface ISpawnStrategy
    {
        void Initialize();
        void Subscribe(Action onResetPool);
        void Unsubscribe();
        int GetActiveCap(int globalMax);
        bool CanSpawn(float distToPlayer, HealthHandler playerHealth);
        void OnEntitySpawned();
    }

    [Serializable]
    public class EnemyWaveStrategy : ISpawnStrategy
    {
        [SerializeField, Min(1)] private int totalWaveQuota = 10;

        private int remainingQuota;
        private Action onResetPool; //store EntitySpawner.ReleaseAll callback (using subscribe())
        public int RemainingQuota => remainingQuota;
        public bool IsQuotaExhausted => remainingQuota <= 0;

        public void Initialize() => remainingQuota = totalWaveQuota;
        public int GetActiveCap(int globalMax) => globalMax;
        public bool CanSpawn(float distToPlayer, HealthHandler _) => remainingQuota > 0; // _ is written to discard handler parameter
        public void OnEntitySpawned() => remainingQuota = Mathf.Max(0, remainingQuota - 1);

        public void Subscribe(Action onReset)
        {
            onResetPool = onReset;
            ProgressionEvents.OnCheckpointRested += HandleCheckpointRest;
        }

        public void Unsubscribe() => ProgressionEvents.OnCheckpointRested -= HandleCheckpointRest; //remove the listener when disabled

        private void HandleCheckpointRest(string _)
        {
            remainingQuota = totalWaveQuota;
            onResetPool?.Invoke(); //return all active enemies to the pool
        }
    }

    [Serializable]
    public class HealingLarvaeStrategy : ISpawnStrategy
    {
        [SerializeField, Min(0f)] private float minRange = 0f; //inner radius of proximity zone
        [SerializeField, Min(1)] private int maxActiveLarvae = 3;

        public void Initialize() { }
        public void Subscribe(Action _) { }
        public void Unsubscribe() { }
        public void OnEntitySpawned() { }

        public int GetActiveCap(int globalMax) => Mathf.Min(globalMax, maxActiveLarvae);

        public bool CanSpawn(float distToPlayer, HealthHandler playerHealth) =>
            distToPlayer >= minRange && //Player must be inside the spawner's proximity band
            playerHealth != null &&
            playerHealth.CurrHealth < playerHealth.MaxHealth;

        public void ClampToRange(float maxActivationRange, int maxActiveEntities)
        {
            minRange = Mathf.Clamp(minRange, 0f, maxActivationRange);
            maxActiveLarvae = Mathf.Clamp(maxActiveLarvae, 1, maxActiveEntities);
        }
    }
}