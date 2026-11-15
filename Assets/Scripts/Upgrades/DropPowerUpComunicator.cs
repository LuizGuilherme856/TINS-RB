using UnityEngine;

// Notifies PowerUpDropManager when this enemy should drop a power-up.
public class DropPowerUpComunicator : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)] private float spawnChance = 0.5f;
    [SerializeField] private bool bossPowerup;
    [SerializeField] private int powerUpIndex;
    [SerializeField] private GlobalStats globalStats;

    private RunData runData;
    private EnemyHealth enemyHealth;
    private PowerUpDropManager powerUpDropManager;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        runData = enemyHealth != null ? enemyHealth.GetRunData() : null;
        powerUpDropManager = FindFirstObjectByType<PowerUpDropManager>();
        if (globalStats == null) globalStats = AssetFinder.GetGlobalStats();
    }

    private void OnEnable()
    {
        if (enemyHealth != null) enemyHealth.OnZeroHealth += SpawnPowerUp;
    }

    private void OnDisable()
    {
        if (enemyHealth != null) enemyHealth.OnZeroHealth -= SpawnPowerUp;
    }

    private void SpawnPowerUp()
    {
        if (powerUpDropManager == null) return;
        if (bossPowerup)
        {
            powerUpDropManager.SpawnBossPowerUp(transform.position, powerUpIndex);
            return;
        }

        float bonusDropChance = globalStats != null ? globalStats.AddTodropChance : 0f;
        if (Random.Range(0f, 100f) < spawnChance + bonusDropChance)
        {
            powerUpDropManager.SpawnPowerUp(transform.position);
            return;
        }

        if (runData != null && runData.survivalMode)
            powerUpDropManager.SpawnSurvivalModeDrop(transform.position);
    }
}