using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using TINS.Campaign;

/// <summary>Adaptive enemy spawner with independent pools for regular and elite enemies.</summary>
public sealed class EnemySpawnManager : MonoBehaviour
{
    [Header("Pool")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject elitePrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] protected List<Transform> validSpawnPoints = new List<Transform>();
    [SerializeField] private Transform spawnPoint;
    [Min(1)] [SerializeField] private int maxInactive = 20;

    [Header("Adaptive wave balancing")]
    [SerializeField, Range(0f, 100f)] protected float elitePercentege;
    [SerializeField, Min(0f)] protected float mulMaxEnemies = 1f;
    [SerializeField, Min(0.01f)] protected float divSpawnInterval = 15f;
    [SerializeField, Min(0.1f)] protected float spawnInterval = 3f;
    [FormerlySerializedAs("maxActiveEnemies")]
    [SerializeField, Min(1)] protected int maxEnemies = 5;
    [SerializeField, Min(1)] protected int limitOfActiveEnemies = 15;

    [Header("Light radius")]
    public Transform centerLocation;
    public PurificationReveal purificationManager;
    public float extraMargin = 1.0f;

    [Header("Automatic spawning")]
    [SerializeField] private bool spawnOnStart = true;

    private IObjectPool<PoolCreator> regularPool;
    private IObjectPool<PoolCreator> elitePool;
    private Transform inactivePoolRoot;
    private Coroutine spawnRoutine;
    private int activeEnemies;
    private CampaignMissionEnemies campaignSettings;
    private float initialLightRadius;
    private readonly List<(CampaignEnemyEntry entry, IObjectPool<PoolCreator> pool)> campaignPools = new();

    private void Awake()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            spawnPoints = spawnPoint != null ? new[] { spawnPoint } : new Transform[0];

        if (validSpawnPoints == null)
            validSpawnPoints = new List<Transform>();

        if (purificationManager == null)
            purificationManager = FindFirstObjectByType<PurificationReveal>();
        if (centerLocation == null && purificationManager != null)
            centerLocation = purificationManager.transform;

        GameObject poolContainer = new GameObject($"{name} - Inactive Pool");
        poolContainer.transform.SetParent(transform, false);
        poolContainer.SetActive(false);
        inactivePoolRoot = poolContainer.transform;

        UpdateAdaptiveWaveSettings();
    }

    private bool EnsureLegacyPools()
    {
        if (regularPool != null) return true;
        if (!ValidatePrefab(enemyPrefab, "regular"))
            return false;

        if (elitePrefab != null && !ValidatePrefab(elitePrefab, "elite"))
        {
            Debug.LogWarning("Elite prefab is invalid; elite rolls will use the regular enemy instead.", this);
            elitePrefab = null;
        }

        regularPool = CreatePool(enemyPrefab);
        if (elitePrefab != null) elitePool = CreatePool(elitePrefab);
        return true;
    }

    private void Start()
    {
        if (spawnOnStart && !CampaignCombatRules.IsCampaignMission)
            StartAutomaticSpawning();
    }

    /// <summary>Spawns one enemy chosen from the current adaptive elite chance.</summary>
    public GameObject SpawnEnemy()
    {
        UpdateAdaptiveWaveSettings();
        if (activeEnemies >= maxEnemies) return null;
        Transform chosenSpawn = GetRandomValidSpawnSpot();
        if (chosenSpawn == null) chosenSpawn = spawnPoint;
        if (chosenSpawn == null) return null;

        IObjectPool<PoolCreator> selectedPool = SelectEnemyPool();
        if (selectedPool == null) return null;
        PoolCreator enemy = selectedPool.Get();
        enemy.transform.SetParent(transform, true);
        enemy.transform.SetPositionAndRotation(chosenSpawn.position, chosenSpawn.rotation);

        Rigidbody2D body = enemy.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            if (body.bodyType == RigidbodyType2D.Static)
                body.bodyType = RigidbodyType2D.Dynamic;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        enemy.gameObject.SetActive(true);
        activeEnemies++;
        return enemy.gameObject;
    }

    public void StartCampaignSpawning(CampaignMissionEnemies settings)
    {
        StopAutomaticSpawning();
        campaignSettings = settings;
        initialLightRadius = CurrentLightRadius;

        if (settings != null)
        {
            var poolsByPrefab = new Dictionary<GameObject, IObjectPool<PoolCreator>>();
            campaignPools.Clear();
            foreach (CampaignEnemyEntry entry in settings.enemies)
            {
                if (entry == null || entry.weight <= 0f || !ValidatePrefab(entry.prefab, "Campaign")) continue;
                if (!poolsByPrefab.TryGetValue(entry.prefab, out IObjectPool<PoolCreator> pool))
                {
                    pool = CreatePool(entry.prefab);
                    poolsByPrefab.Add(entry.prefab, pool);
                }
                campaignPools.Add((entry, pool));
            }
        }

        if (spawnOnStart) StartAutomaticSpawning();
    }

    private IObjectPool<PoolCreator> SelectEnemyPool()
    {
        if (campaignSettings == null)
        {
            if (!EnsureLegacyPools()) return null;
            return elitePool != null && Random.Range(0f, 100f) < elitePercentege ? elitePool : regularPool;
        }

        float radius = CurrentLightRadius;
        float totalWeight = 0f;
        foreach (var candidate in campaignPools)
            if (candidate.entry.minimumLightRadius <= radius) totalWeight += Mathf.Max(0f, candidate.entry.weight);
        if (totalWeight <= 0f) return null;

        float roll = Random.Range(0f, totalWeight);
        IObjectPool<PoolCreator> lastEligible = null;
        foreach (var candidate in campaignPools)
        {
            if (candidate.entry.minimumLightRadius > radius || candidate.entry.weight <= 0f) continue;
            lastEligible = candidate.pool;
            roll -= candidate.entry.weight;
            if (roll <= 0f) return candidate.pool;
        }
        return lastEligible;
    }

    public void StopAutomaticSpawning()
    {
        if (spawnRoutine == null)
            return;

        StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    public void StartAutomaticSpawning()
    {
        if (spawnRoutine != null || (campaignSettings == null && !EnsureLegacyPools())) return;
        if (campaignSettings != null && campaignPools.Count == 0) return;

        int initialCount = campaignSettings == null ? 1 :
            Mathf.Clamp(campaignSettings.initialEnemyCount, 0, campaignSettings.ActiveEnemyLimit(0f));
        for (int i = 0; i < initialCount; i++)
            if (SpawnEnemy() == null) break;
        spawnRoutine = StartCoroutine(AutomaticSpawnLoop());
    }

    private IEnumerator AutomaticSpawnLoop()
    {
        while (true)
        {
            UpdateAdaptiveWaveSettings();
            yield return new WaitForSeconds(Mathf.Max(0.1f, spawnInterval));
            SpawnEnemy();
        }
    }

    private float CurrentLightRadius => purificationManager != null ? Mathf.Max(0f, purificationManager.Radius) : 0f;

    private void UpdateAdaptiveWaveSettings()
    {
        float radius = CurrentLightRadius;
        if (campaignSettings != null)
        {
            float growth = radius - initialLightRadius;
            spawnInterval = campaignSettings.SpawnInterval(growth);
            maxEnemies = campaignSettings.ActiveEnemyLimit(growth);
            return;
        }

        float safeDivisor = Mathf.Max(0.01f, divSpawnInterval);

        // Requested formula: interval = 4.5 - ceil(radius) / divSpawnInterval.
        if (radius <= 17f)
        {
            spawnInterval = Mathf.Max(0.2f, 4.5f - Mathf.Ceil(radius) / safeDivisor);
        }
        else 
        {
            spawnInterval = Mathf.Max(0.2f, 1.9f - Mathf.Ceil(radius) / 20);
        }
            
        maxEnemies = Mathf.Min(Mathf.Max(1, 2 + Mathf.FloorToInt(radius * Mathf.Max(0f, mulMaxEnemies))),limitOfActiveEnemies);
        if (radius <= 3.6f)
        {
            elitePercentege = 0f;
        }
        else 
        {
            elitePercentege = Mathf.Clamp(1f + Mathf.Floor(radius), 0f, 100f);
        }
           // elitePercentege = radius <= 3.6f ? 0f : Mathf.Clamp(1f + Mathf.Floor(radius), 0f, 100f);
    }

    private IObjectPool<PoolCreator> CreatePool(GameObject prefab)
    {
        IObjectPool<PoolCreator> createdPool = null;
        createdPool = new ObjectPool<PoolCreator>(
            () => CreateEnemy(prefab, createdPool),
            OnTakeFromPool,
            OnReturnToPool,
            OnDestroyPooledEnemy,
            collectionCheck: true,
            defaultCapacity: Mathf.Max(1, maxInactive),
            maxSize: Mathf.Max(1, maxInactive));
        return createdPool;
    }

    private PoolCreator CreateEnemy(GameObject prefab, IObjectPool<PoolCreator> owningPool)
    {
        PoolCreator enemy = Instantiate(prefab, inactivePoolRoot).GetComponent<PoolCreator>();
        enemy.gameObject.SetActive(false);
        enemy.SetPool(owningPool);
        return enemy;
    }

    private void OnTakeFromPool(PoolCreator enemy)
    {
        enemy.gameObject.SetActive(false);
    }

    private void OnReturnToPool(PoolCreator enemy)
    {
        activeEnemies = Mathf.Max(0, activeEnemies - 1);
        enemy.gameObject.SetActive(false);
        enemy.transform.SetParent(inactivePoolRoot, false);
    }

    private static void OnDestroyPooledEnemy(PoolCreator enemy)
    {
        if (enemy != null)
            Destroy(enemy.gameObject);
    }

    private bool ValidatePrefab(GameObject prefab, string role)
    {
        if (prefab == null)
        {
            Debug.LogError($"{nameof(EnemySpawnManager)} on {name} needs a {role} enemy prefab.", this);
            return false;
        }

        if (prefab.GetComponent<PoolCreator>() == null || prefab.GetComponent<EnemyHealth>() == null)
        {
            Debug.LogError($"{role} enemy prefab '{prefab.name}' must have PoolCreator and EnemyHealth components.", prefab);
            return false;
        }

        return true;
    }

    protected Transform GetRandomValidSpawnSpot()
    {
        validSpawnPoints.Clear();
        foreach (Transform target in spawnPoints)
        {
            if (target != null && IsValid(target))
                validSpawnPoints.Add(target);
        }

        if (validSpawnPoints.Count == 0)
            return null;

        return validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
    }

    /// <summary>A valid spawn point is outside the purification radius plus margin.</summary>
    protected bool IsValid(Transform targetToCheck)
    {
        if (targetToCheck == null)
            return false;

        Transform lightCenter = centerLocation != null
            ? centerLocation
            : purificationManager != null ? purificationManager.transform : null;
        if (lightCenter == null)
            return true;

        float radius = purificationManager != null ? purificationManager.Radius : 0f;
        float minDistance = radius + extraMargin;
        return Vector2.Distance(lightCenter.position, targetToCheck.position) >= minDistance;
    }
}
