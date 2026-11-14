using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldMinionManger : MonoBehaviour
{
    public List<Transform> spawnPositions; // Available spawn positions.
    public GameObject enemyPrefab; // Enemy prefab.
    public GameObject bossShield; // Boss shield object.
    public GameObject lineRendererPrefab; // LineRenderer prefab.
    public EnemyHealth bossHealth; // Boss Health reference.
    private List<GameObject> spawnedEnemies = new List<GameObject>(); // Spawned enemy list.
    private List<LineRenderer> lineRenderers = new List<LineRenderer>(); // LineRenderer list.
    public float spawnChance = 0.2f; // Chance to spawn enemies when the boss takes damage.
    private float nextShieldThreshold; // Next health threshold that triggers the shield.
    private float lastSpawnTime; // Time of the last spawn.
    public float spawnCooldown = 20f; // Twenty-second cooldown between spawns.

    // Start is called before the first frame update
    void Start()
    {
        bossHealth.OnTakeDamage += OnBossTakeDamage;
        nextShieldThreshold = bossHealth.maxHealth * 0.8f; // Initialize the first 20% threshold.
        lastSpawnTime = -spawnCooldown; // Initialize so the first spawn can happen immediately.
    }

    void SpawnEnemies(int numberOfEnemiesToSpawn)
    {
        if (spawnPositions.Count == 0 || enemyPrefab == null || lineRendererPrefab == null)
        {
            Debug.LogWarning("Nenhuma posi��o de spawn, prefab de inimigo ou prefab de LineRenderer foi definida.");
            return;
        }

        if(Time.time - lastSpawnTime < spawnCooldown)
        {
            return;
        }

        for (int i = 0; i < numberOfEnemiesToSpawn; i++)
        {
            // Choose a random spawn position.
            Transform spawnPosition = spawnPositions[Random.Range(0, spawnPositions.Count)];

            // Spawn the enemy.
            GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition.position, spawnPosition.rotation);
            spawnedEnemies.Add(spawnedEnemy);

            // Spawn the LineRenderer.
            GameObject lineRendererObject = Instantiate(lineRendererPrefab);
            LineRenderer lineRenderer = lineRendererObject.GetComponent<LineRenderer>();
            lineRenderers.Add(lineRenderer);

            // Configure LineRenderer.
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, bossShield.transform.position);
            lineRenderer.SetPosition(1, spawnedEnemy.transform.position);

            // Get the spawned enemy's EnemyHealth component.
            EnemyHealth enemyHealth = spawnedEnemy.GetComponent<EnemyHealth>();

            // Subscribe to zero health to disable the shield.
            enemyHealth.OnZeroHealth += () => DisableLineRenderer(lineRenderer);
        }

        // Enable the boss shield.
        bossShield.SetActive(true);

    }

    void OnBossTakeDamage()
    {

        bool spawned = TrySpawnEnemies();
        // Check whether the shield has already spawned.
        if (spawned == false)
        {

            if (bossHealth.currentHealth <= nextShieldThreshold)
            {
                // Trigger the shield.
                SpawnEnemies(5); // Spawn five enemies.

                // Advance to the next 20% threshold.
                nextShieldThreshold -= bossHealth.maxHealth * 0.2f;
                if (nextShieldThreshold <= bossHealth.maxHealth * 0.05f)
                {
                    nextShieldThreshold = -1; //Stop triggering once the boss reaches 5% health.
                }
            }

        }

        if (bossHealth.currentHealth <= 0)
        {
            // Disable the boss shield.
            bossShield.SetActive(false);
            // Destroy all spawned enemies.
            foreach (var enemy in spawnedEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy);
                }
            }
            // Clear enemy and LineRenderer lists.
            spawnedEnemies.Clear();


            foreach (var lineRenderer in lineRenderers)
            {
                if (lineRenderer != null)
                {
                    DisableLineRenderer(lineRenderer);
                }
            }
            lineRenderers.Clear();

        }

    }

    bool TrySpawnEnemies()
    {
        if (Random.value < spawnChance)
        {
            SpawnEnemies(3); // Spawn three enemies.
            return true;
        }
        return false;
    }

    void DisableLineRenderer(LineRenderer lineRenderer)
    {
        // Disable the LineRenderer.
        lineRenderer.enabled = false;
        lineRenderer.sortingLayerName = "Default"; // Ensure it is not visible.
        lineRenderer.sortingOrder = -99999; // Ensure it is not visible.

        // Check whether all spawned enemies are dead.
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null && enemy.GetComponent<EnemyHealth>().currentHealth > 0)
            {
                return;
            }
        }

        lastSpawnTime = Time.time;
        // Disable the boss shield.
        bossShield.SetActive(false);
    }

    void Update()
    {
        // Update LineRenderer positions.
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            if (spawnedEnemies[i] != null)
            {
                lineRenderers[i].SetPosition(1, spawnedEnemies[i].transform.position);
            }
        }
    }
}
