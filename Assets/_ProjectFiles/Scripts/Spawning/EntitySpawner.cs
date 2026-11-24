using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Spawning  // a good practice to group all spawning classes under the same name
{
    public enum SpawnerMode { Enemy, HealingLarvae }

    public class EntitySpawner : MonoBehaviour
    {
        [Header("Core Config")]
        [SerializeField] private SpawnerMode mode = SpawnerMode.Enemy;
        [SerializeField] private PoolableEntity entityPrefab;
        [SerializeField, Min(1)] private int maxActiveEntities = 5;
        [SerializeField, Min(0.1f)] private float spawnInterval = 2f;
        [SerializeField, Min(1f)] private float maxActivationRange = 35f;
        [SerializeField, Min(1f)] private float spawnRadius = 15f;
        [SerializeField, Min(1)] private int maxSpawnAttempts = 12;

        [Header("Modules")]
        [SerializeField] private SpawnPointValidator validator = new();
        [SerializeField] private EnemyWaveStrategy enemyMode = new();
        [SerializeField] private HealingLarvaeStrategy larvaeMode = new();

        [Header("Scene Refrences (Auto-ignored if Empty)")] //just optional overrides if we needed them when these fles are integrated to diff spawning things
        [SerializeField] private Transform player;
        [SerializeField] private HealthHandler playerHealth;
        [SerializeField] private Camera mainCamera;

        private ISpawnStrategy strategy;
        private ObjectPool<PoolableEntity> pool;
        private readonly List<PoolableEntity> active = new(); //to tracks every currently active entity but without adding or removing
        private int poolCapacity;
        private float timer;
        private bool isPrewarming, isShuttingDown; //just used to prevent spawning at start/warmup or during scene ending

        public int ActiveCount => active.Count;
        public bool IsCleared => mode == SpawnerMode.Enemy && enemyMode.IsQuotaExhausted && active.Count == 0;

        private void OnValidate()
        {
            larvaeMode.ClampToRange(maxActivationRange, maxActiveEntities);
            spawnRadius = Mathf.Min(spawnRadius, maxActivationRange);
        }

        private void Awake()
        {
            if (entityPrefab == null) { enabled = false; return; }

            ResolveRefs();
            //strategy = mode == SpawnerMode.Enemy ? enemyMode : larvaeMode;
            strategy.Initialize();

            poolCapacity = strategy.GetActiveCap(maxActiveEntities);
            PrewarmPool(poolCapacity);
        }

        private void OnEnable() => strategy?.Subscribe(ReleaseAll); //ReleaseAll prevents null refrence 
        private void OnDisable() => strategy?.Unsubscribe();

        private void OnDestroy()
        {
            isShuttingDown = true;
            active.Clear();
            pool?.Dispose();
        }

        private void Update()
        {
            if (!CanSpawnNow()) return;

            timer += Time.deltaTime;
            if (timer < spawnInterval) return;

            timer = 0f;
            if (!validator.TryFindPoint(transform.position, spawnRadius, player, mainCamera, maxSpawnAttempts, out Vector3 pos))
                return;

            pool.Get().PrepareForSpawn(pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            strategy.OnEntitySpawned(); //notifies the strategy that an entity spawned
        }

        private bool CanSpawnNow()
        {
            if (player == null || mainCamera == null) ResolveRefs();
            if (player == null || mainCamera == null) return false;
            if (active.Count >= poolCapacity) return false;

            float dist = Vector3.Distance(transform.position, player.position);
            return dist <= maxActivationRange && strategy.CanSpawn(dist, playerHealth);
        }

        private void PrewarmPool(int size)
        {
            pool = new ObjectPool<PoolableEntity>(
                createFunc: () =>
                {
                    var obj = Instantiate(entityPrefab, transform);
                    obj.gameObject.SetActive(false); //clone inactive until spawned
                    obj.Bind(e => { if (!isShuttingDown) pool.Release(e); });
                    return obj;
                },
                actionOnGet: e => { if (!isPrewarming) active.Add(e); },
                actionOnRelease: e =>
                {
                    if (isPrewarming) return;
                    e.OnReturnedToPool();
                    active.Remove(e);
                },
                actionOnDestroy: e => { if (e != null && isShuttingDown) Destroy(e.gameObject); },
                collectionCheck: true, //built check that catches accidental double-releases
                defaultCapacity: size,
                maxSize: size
            );

            isPrewarming = true;
            var temp = new PoolableEntity[size];
            for (int i = 0; i < size; i++) temp[i] = pool.Get(); //creating them early
            for (int i = 0; i < size; i++) pool.Release(temp[i]);//returns all size instances into the inactive pool stack
            isPrewarming = false;
        }

        public void ReleaseAll() //Returns every active entity back to the pool
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                if (i < active.Count) active[i]?.ReturnToPool();
            }
            timer = 0f; //reset spawn timer after clearing active entities
        }

        private void ResolveRefs() //Auto-populates missing Inspector scene references
        {
            mainCamera ??= Camera.main;
            if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerHealth == null && player != null) playerHealth = player.GetComponent<HealthHandler>();
        }

    }
}