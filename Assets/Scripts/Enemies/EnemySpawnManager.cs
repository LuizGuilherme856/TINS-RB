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
    private IObjectPool<PoolCreator> finalBossPool;
    private TowerController campaignTower;
    private EnemyHealth finalBossInstance;
    private bool finalBossDefeated;
    private Transform inactivePoolRoot;
    private Coroutine spawnRoutine;
    private int activeEnemies;
    private CampaignMissionEnemies campaignSettings;
    private float initialLightRadius;
    private float campaignGoalRadius;
    private float waveWarningEndsAt;
    private float waveRecoveryEndsAt;
    private readonly List<(CampaignEnemyEntry entry, IObjectPool<PoolCreator> original, IObjectPool<PoolCreator> fourDirections)> campaignPools = new();
    private readonly List<(float radius, int elitePoolIndex)> waveMilestones = new();
    private readonly HashSet<int> skippedEliteWaves = new();
    private int elitePoolCount;
    private int nextWaveMilestone;
    public int WaveMilestoneCount => waveMilestones.Count;
    public float WaveMilestoneRadius(int index) => waveMilestones[index].radius;
    public bool IsEliteWave(int index) => waveMilestones[index].elitePoolIndex >= 0 || IsFinalBossWave(index);
    public bool IsFinalBossWave(int index) => finalBossPool != null && index == waveMilestones.Count - 1;
    public bool CanFinishCampaign => finalBossPool == null || finalBossDefeated;
    public bool IsBossWaveAnnounced => WaveDirection != Vector2Int.zero && IsFinalBossWave(nextWaveMilestone);
    public string ActiveBossName => finalBossInstance != null ? finalBossInstance.enemyData.EnemyName : null;
    public float EliteWaveEndRadius(int index) => Mathf.Min(campaignGoalRadius,
        WaveMilestoneRadius(index) + Mathf.Max(0.01f, campaignSettings.eliteWaveSkipWidth));
    public bool IsWaveMilestonePending(int index) => index >= nextWaveMilestone && !skippedEliteWaves.Contains(index);
    public Vector2Int WaveDirection { get; private set; }
    public float WaveWarningRemaining => Mathf.Max(0f, waveWarningEndsAt - Time.time);

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
    public GameObject SpawnEnemy() => SpawnEnemy(Vector2Int.zero);

    private GameObject SpawnEnemy(Vector2Int direction, int elitePoolIndex = -1, bool finalBoss = false)
    {
        UpdateAdaptiveWaveSettings();
        if (!finalBoss && activeEnemies >= maxEnemies) return null;
        Transform chosenSpawn = GetRandomValidSpawnSpot(direction);
        if (chosenSpawn == null && direction == Vector2Int.zero && (campaignSettings == null || IsValid(spawnPoint)))
            chosenSpawn = spawnPoint;
        if (chosenSpawn == null && !finalBoss) return null;

        bool elite = true;
        IObjectPool<PoolCreator> selectedPool = finalBoss ? finalBossPool : SelectEnemyPool(elitePoolIndex, out elite);
        if (selectedPool == null) return null;
        PoolCreator enemy = selectedPool.Get();
        enemy.transform.SetParent(transform, true);
        Vector3 position = chosenSpawn != null ? chosenSpawn.position
            : centerLocation.position + (Vector3)(Vector2)direction * MinimumSpawnDistance;
        enemy.transform.SetPositionAndRotation(position, chosenSpawn != null ? chosenSpawn.rotation : Quaternion.identity);

        Rigidbody2D body = enemy.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            if (body.bodyType == RigidbodyType2D.Static)
                body.bodyType = RigidbodyType2D.Dynamic;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        enemy.GetComponent<EnemyHealth>()?.SetCampaignElite(elite);
        enemy.gameObject.SetActive(true);
        if (campaignTower != null)
        {
            if (enemy.TryGetComponent(out ChangeTarget targeting))
                targeting.SetNewTarget(campaignTower.transform);
            else
            {
                if (enemy.TryGetComponent(out EnemyAI ai)) ai.SetTarget(campaignTower.transform);
                if (enemy.TryGetComponent(out MoveTowardsPlayer chaser)) chaser.SetTarget(campaignTower.transform);
            }
            if (enemy.TryGetComponent(out EnemyTowerAttack attack)) attack.SetTargetTower(campaignTower);
        }
        activeEnemies++;
        return enemy.gameObject;
    }

    public void StartCampaignSpawning(CampaignMissionEnemies settings, float goalRadius,
        TowerController tower = null, PurificationReveal reveal = null)
    {
        StopAutomaticSpawning();
        campaignSettings = settings;
        campaignTower = tower;
        if (reveal != null) purificationManager = reveal;
        initialLightRadius = CurrentPressureRadius;
        campaignGoalRadius = goalRadius;
        campaignPools.Clear();
        waveMilestones.Clear();
        skippedEliteWaves.Clear();
        if (finalBossInstance != null) finalBossInstance.OnZeroHealth -= HandleFinalBossDefeated;
        finalBossInstance = null;
        finalBossPool = null;
        finalBossDefeated = false;
        elitePoolCount = 0;
        nextWaveMilestone = 0;

        if (settings != null)
        {
            var poolsByPrefab = new Dictionary<GameObject, IObjectPool<PoolCreator>>();
            foreach (CampaignEnemyEntry entry in settings.enemies)
            {
                if (entry == null || entry.weight <= 0f || !ValidatePrefab(entry.prefab, "Campaign")) continue;
                var original = PoolFor(entry.prefab);
                var fourDirections = entry.fourDirectionPrefab != null && ValidatePrefab(entry.fourDirectionPrefab, "Campaign four-direction")
                    ? PoolFor(entry.fourDirectionPrefab)
                    : null;
                campaignPools.Add((entry, original, fourDirections));
                if (entry.isElite) elitePoolCount++;
            }
            campaignPools.Sort((left, right) => left.entry.isElite == right.entry.isElite
                ? left.entry.minimumLightRadius.CompareTo(right.entry.minimumLightRadius)
                : right.entry.isElite.CompareTo(left.entry.isElite));

            for (int i = 0; i < elitePoolCount; i++)
            {
                float radius = Mathf.Max(initialLightRadius, campaignPools[i].entry.minimumLightRadius);
                if (radius < goalRadius) waveMilestones.Add((radius, i));
            }
            foreach (float radius in settings.waveRadii)
                if (radius >= initialLightRadius && radius < goalRadius)
                    waveMilestones.Add((radius, -1));
            waveMilestones.Sort((left, right) => left.radius.CompareTo(right.radius));
            if (settings.finalBoss != null && ValidatePrefab(settings.finalBoss, "Campaign final boss"))
            {
                finalBossPool = CreatePool(settings.finalBoss);
                waveMilestones.Add((goalRadius, -1));
            }

            IObjectPool<PoolCreator> PoolFor(GameObject prefab)
            {
                if (!poolsByPrefab.TryGetValue(prefab, out var pool))
                {
                    pool = CreatePool(prefab);
                    poolsByPrefab.Add(prefab, pool);
                }
                return pool;
            }
        }

        if (spawnOnStart) StartAutomaticSpawning();
    }

    public void SkipEliteWavesCrossedByDelivery(float startingRadius, float targetRadius)
    {
        // A wave that has already announced its direction is committed.
        int firstUnannounced = WaveDirection == Vector2Int.zero ? nextWaveMilestone : nextWaveMilestone + 1;
        for (int i = firstUnannounced; i < waveMilestones.Count; i++)
        {
            float bandStart = WaveMilestoneRadius(i);
            if (targetRadius < bandStart) break;
            if (IsFinalBossWave(i) || !IsEliteWave(i) || startingRadius > bandStart) continue;

            float bandEnd = EliteWaveEndRadius(i);
            if (targetRadius >= bandEnd || Mathf.Approximately(targetRadius, bandEnd))
                skippedEliteWaves.Add(i);
        }
    }

    private IObjectPool<PoolCreator> SelectEnemyPool(int elitePoolIndex, out bool elite)
    {
        elite = false;
        if (campaignSettings == null)
        {
            if (!EnsureLegacyPools()) return null;
            elite = elitePool != null && Random.Range(0f, 100f) < elitePercentege;
            return elite ? elitePool : regularPool;
        }

        if (elitePoolIndex >= 0)
        {
            elite = true;
            return CampaignVariant(elitePoolIndex);
        }

        float radius = CurrentPressureRadius;
        float totalWeight = 0f;
        for (int i = elitePoolCount; i < campaignPools.Count; i++)
        {
            CampaignEnemyEntry entry = campaignPools[i].entry;
            if (entry.minimumLightRadius <= radius) totalWeight += entry.weight;
        }
        if (totalWeight <= 0f) return null;

        float roll = Random.Range(0f, totalWeight);
        int selection = -1;
        for (int i = elitePoolCount; i < campaignPools.Count; i++)
        {
            var candidate = campaignPools[i];
            if (candidate.entry.minimumLightRadius > radius || candidate.entry.weight <= 0f) continue;
            selection = i;
            roll -= candidate.entry.weight;
            if (roll <= 0f) break;
        }
        return selection >= 0 ? CampaignVariant(selection) : null;
    }

    private IObjectPool<PoolCreator> CampaignVariant(int selection)
    {
        var selected = campaignPools[selection];
        float chance = campaignSettings.fourDirectionChance;
        return selected.fourDirections != null && (chance >= 1f || (chance > 0f && Random.value < chance))
            ? selected.fourDirections
            : selected.original;
    }

    public void StopAutomaticSpawning()
    {
        WaveDirection = Vector2Int.zero;
        waveWarningEndsAt = 0f;
        waveRecoveryEndsAt = 0f;
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
        int spawnsSinceGroup = 0;
        while (true)
        {
            UpdateAdaptiveWaveSettings();
            bool recovering = Time.time < waveRecoveryEndsAt;
            yield return new WaitForSeconds(Mathf.Max(0.1f, spawnInterval) * (recovering ? 2f : 1f));
            if (campaignSettings != null && CurrentPressureRadius >= campaignGoalRadius && CanFinishCampaign) yield break;
            while (skippedEliteWaves.Contains(nextWaveMilestone))
                nextWaveMilestone++;
            if (Time.time >= waveRecoveryEndsAt && nextWaveMilestone < waveMilestones.Count &&
                CurrentPressureRadius >= WaveMilestoneRadius(nextWaveMilestone))
            {
                Transform point = GetRandomValidSpawnSpot();
                if (point != null || IsFinalBossWave(nextWaveMilestone))
                {
                    spawnsSinceGroup = 0;
                    yield return RunAnnouncedWave(point != null ? DirectionOf(point) : Vector2Int.up);
                    continue;
                }
            }
            if (SpawnEnemy() == null || recovering || campaignSettings == null ||
                campaignSettings.spawnsBetweenReinforcements <= 0 || CurrentPressureRadius < campaignSettings.minimumReinforcementRadius)
                continue;
            if (++spawnsSinceGroup < campaignSettings.spawnsBetweenReinforcements) continue;

            spawnsSinceGroup = 0;
            for (int i = 0; i < campaignSettings.extraEnemiesInGroup; i++)
                if (SpawnEnemy() == null) break;
        }
    }

    private IEnumerator RunAnnouncedWave(Vector2Int direction)
    {
        WaveDirection = direction;
        float warning = Mathf.Max(0.5f, campaignSettings.waveWarningSeconds);
        waveWarningEndsAt = Time.time + warning;
        yield return new WaitForSeconds(warning);

        UpdateAdaptiveWaveSettings();
        int count = Mathf.Clamp(campaignSettings.waveEnemyCount, 1, maxEnemies);
        bool finalBoss = IsFinalBossWave(nextWaveMilestone);
        // Leave room for the full group without exceeding the planet's active-enemy cap.
        while (!finalBoss && activeEnemies + count > maxEnemies)
        {
            yield return null;
            UpdateAdaptiveWaveSettings();
        }

        int elitePoolIndex = waveMilestones[nextWaveMilestone].elitePoolIndex;
        GameObject leader = SpawnEnemy(direction, elitePoolIndex, finalBoss);
        if (leader != null)
        {
            if (finalBoss)
            {
                finalBossInstance = leader.GetComponent<EnemyHealth>();
                finalBossInstance.OnZeroHealth += HandleFinalBossDefeated;
            }
            for (int i = 1; i < count; i++)
            {
                yield return new WaitForSeconds(0.25f);
                if (SpawnEnemy(direction) == null) break;
            }
            nextWaveMilestone++;
            waveRecoveryEndsAt = Time.time + Mathf.Max(0f, campaignSettings.waveRecoverySeconds);
        }
        WaveDirection = Vector2Int.zero;
        waveWarningEndsAt = 0f;
    }

    private void HandleFinalBossDefeated()
    {
        finalBossInstance.OnZeroHealth -= HandleFinalBossDefeated;
        finalBossInstance = null;
        finalBossDefeated = true;
    }

    public float FarthestSpawnDistance(Vector2 origin)
    {
        float distance = spawnPoint != null ? Vector2.Distance(origin, spawnPoint.position) : 0f;
        foreach (Transform point in spawnPoints)
            if (point != null) distance = Mathf.Max(distance, Vector2.Distance(origin, point.position));
        return distance;
    }

    private float CurrentLightRadius => purificationManager != null ? Mathf.Max(0f, purificationManager.Radius) : 0f;
    private float CurrentPressureRadius => campaignSettings != null && CampaignMissionRuntime.Active != null
        ? CampaignMissionRuntime.Active.EnemyPressureRadius : CurrentLightRadius;

    private void UpdateAdaptiveWaveSettings()
    {
        float radius = CurrentPressureRadius;
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

    private Vector2Int DirectionOf(Transform point)
    {
        Transform center = centerLocation != null ? centerLocation : purificationManager?.transform;
        Vector2 offset = point.position - (center != null ? center.position : transform.position);
        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
            return offset.x >= 0f ? Vector2Int.right : Vector2Int.left;
        return offset.y >= 0f ? Vector2Int.up : Vector2Int.down;
    }

    protected Transform GetRandomValidSpawnSpot(Vector2Int direction = default)
    {
        validSpawnPoints.Clear();
        Transform nearest = null;
        float nearestDistance = float.PositiveInfinity;
        Transform lightCenter = centerLocation != null ? centerLocation : purificationManager?.transform;
        float bandStart = MinimumSpawnDistance;
        float bandEnd = bandStart + (campaignSettings != null ? Mathf.Max(0.1f, campaignSettings.spawnBandWidth) : 0f);

        foreach (Transform target in spawnPoints)
        {
            if (target == null || (direction != Vector2Int.zero && DirectionOf(target) != direction)) continue;
            float distance = lightCenter != null ? Vector2.Distance(lightCenter.position, target.position) : float.PositiveInfinity;
            if (distance < bandStart) continue;
            if (campaignSettings != null && lightCenter != null)
            {
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = target;
                }
                if (distance > bandEnd) continue;
            }
            validSpawnPoints.Add(target);
        }

        if (validSpawnPoints.Count == 0)
            return nearest;

        return validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
    }

    private float MinimumSpawnDistance
    {
        get
        {
            float radius = campaignSettings != null && CampaignMissionRuntime.Active != null
                ? CampaignMissionRuntime.Active.EnemySpawnBoundary
                : CurrentLightRadius;
            return radius + extraMargin;
        }
    }

    /// <summary>A valid spawn point is beyond the active light boundary plus margin.</summary>
    protected bool IsValid(Transform targetToCheck)
    {
        if (targetToCheck == null)
            return false;

        Transform lightCenter = centerLocation != null
            ? centerLocation
            : purificationManager != null ? purificationManager.transform : null;
        if (lightCenter == null)
            return true;

        return Vector2.Distance(lightCenter.position, targetToCheck.position) >= MinimumSpawnDistance;
    }
}
