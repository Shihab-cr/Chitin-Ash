using UnityEngine;
using Game.Spawning;

namespace Game.Larva
{
    [RequireComponent(typeof(PoolableEntity))] //Forces Unity to ensure PoolableEntity is present on the same GameObject
    public class HealingLarvaPickup : MonoBehaviour
    {
        [SerializeField] private bool storeInsteadOfInstantHeal = false;
        private PoolableEntity poolable;

        private void Awake() => poolable = GetComponent<PoolableEntity>();

        private void OnTriggerEnter(Collider other)
        //fired when another collider enters this prefab's trigger volume
        {
            if (!other.CompareTag("Player") || LarvaHealingManager.Instance == null)
                return;

            bool consumed = storeInsteadOfInstantHeal
                ? LarvaHealingManager.Instance.TryStoreLarva()
                : LarvaHealingManager.Instance.ApplyDirectHeal(other.GetComponent<HealthHandler>());

            if (consumed)
                poolable.ReturnToPool();
        }
    }
}