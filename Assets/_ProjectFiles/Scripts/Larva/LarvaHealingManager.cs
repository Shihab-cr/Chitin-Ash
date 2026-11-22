using System;
using System.IO;
using UnityEngine;
using Game.Spawning;

namespace Game.Larva
{
    [Serializable]
    public class LarvaHealingData
    {
        public float healAmount = 25f;
        public int upgradeLevel = 1; //current larva lv
        public int storedLarvae = 0;
        public int maxStoredLarvae = 5;
    }

    public class LarvaHealingManager : MonoBehaviour //Singleton design pattern used
    {
        public static LarvaHealingManager Instance { get; private set; } 
        [SerializeField] private LarvaHealingData data = new();
        [SerializeField] private bool saveToJson = true;
        [SerializeField] private bool persistAcrossScenes = true; //Toggle controlling DontDestroyOnLoad

        public LarvaHealingData Data => data;
        public event Action<float> OnLarvaHealed; //observer event broadcasting the healed HP amount
        public event Action<LarvaHealingData> OnDataChanged; //broadcast changes

        private string SavePath => Path.Combine(Application.persistentDataPath, "larva_healing.json");

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; } //enforces a single instance
            Instance = this;
            if (persistAcrossScenes) DontDestroyOnLoad(gameObject);  //Keeps the manager alive across scene loads
            Load();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public bool ApplyDirectHeal(HealthHandler targetHealth)
        {
            if (targetHealth == null || targetHealth.CurrHealth >= targetHealth.MaxHealth)
                return false;

            targetHealth.Heal(data.healAmount);
            OnLarvaHealed?.Invoke(data.healAmount);
            return true;
        }

        public bool TryStoreLarva() //add one larva charge to stored inventory
        {
            if (data.storedLarvae >= data.maxStoredLarvae) return false;
            data.storedLarvae++;
            SaveAndNotify();
            return true;
        }

        public bool TryUseStoredLarva(HealthHandler targetHealth) //consume 1 larva to heal
        {
            if (data.storedLarvae <= 0 || !ApplyDirectHeal(targetHealth)) return false;
            data.storedLarvae--;
            SaveAndNotify();
            return true;
        }

        public void UpgradeHealing(float bonusHeal, int bonusCapacity = 0)
        {
            data.upgradeLevel++;
            data.healAmount += bonusHeal;
            data.maxStoredLarvae += bonusCapacity;
            SaveAndNotify();
        }

        public void Save() //actual save to disk
        {
            if (!saveToJson) return;
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        }

        public void Load() //read from disk
        {
            if (!saveToJson || !File.Exists(SavePath)) return;
            JsonUtility.FromJsonOverwrite(File.ReadAllText(SavePath), data);
            OnDataChanged?.Invoke(data);
        }

        private void SaveAndNotify() //save and broadcast changes
        {
            Save();
            OnDataChanged?.Invoke(data);
        }
    }
}