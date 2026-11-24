using System;
using UnityEngine;

namespace Game.Spawning
{
    public interface IPoolableResettable
    {
        void OnSpawnedFromPool();
        void OnDespawnedToPool();
    }

    public class PoolableEntity : MonoBehaviour
    //lifecycle adapter attached to each prefab that makes an Enemy or Healing Larva reusable inside Unity's ObjectPool
    {
        private HealthHandler health;
        private Rigidbody rb;
        private IPoolableResettable[] resettables;
        private Action<PoolableEntity> releaseCallback;
        private bool isInitialized;
        private bool isInPool = true;

        private void Awake() => EnsureInitialized();

        private void OnEnable()
        {
            EnsureInitialized();
            //if (health != null) health.OnDied += ReturnToPool;
        }

        private void OnDisable()
        {
            //if (health != null) health.OnDied -= ReturnToPool;
        }

        private void EnsureInitialized()
        {
            if (isInitialized) return;
            health = GetComponent<HealthHandler>();
            rb = GetComponent<Rigidbody>();
            resettables = GetComponentsInChildren<IPoolableResettable>(true);
            isInitialized = true;
        }

        public void Bind(Action<PoolableEntity> onRelease) => releaseCallback = onRelease;

        public void PrepareForSpawn(Vector3 pos, Quaternion rot)
        {
            EnsureInitialized();
            isInPool = false;
            transform.SetPositionAndRotation(pos, rot);

            if (rb != null && !rb.isKinematic)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = Vector3.zero;
#else
                rb.velocity = Vector3.zero;
#endif
                rb.angularVelocity = Vector3.zero;
            }

            health?.ResetHealth();
            gameObject.SetActive(true);

            foreach (var r in resettables) r.OnSpawnedFromPool();
        }

        public void OnReturnedToPool()
        {
            EnsureInitialized();
            isInPool = true;
            foreach (var r in resettables) r.OnDespawnedToPool();
            gameObject.SetActive(false);
        }

        public void ReturnToPool()
        {
            if (!isInPool) releaseCallback?.Invoke(this);
        }
    }
}