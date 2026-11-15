using UnityEngine;

/// <summary>Notifies the altar ritual when an enemy dies inside its range.</summary>
public class SacrificeCommunicator : MonoBehaviour
{
    private OrbSpawner orbSpawner;
    private EnemyHealth enemyHealth;
    private Transform altarTransform;
    [SerializeField] private int numberOfSoulsToSpawn = 1;
    [SerializeField] private GameObject sacrificeIcon;
    private ManageBloodRitual manageRitual;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnZeroHealth -= HandleDeath;
            enemyHealth.OnZeroHealth += HandleDeath;
        }
        if (sacrificeIcon != null) sacrificeIcon.SetActive(false);
    }

    private void OnDisable()
    {
        if (enemyHealth != null) enemyHealth.OnZeroHealth -= HandleDeath;
        if (manageRitual != null) manageRitual.OnCompleted -= DeactivateSacrificeIconOnComplete;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Altar")) return;

        orbSpawner = collision.GetComponent<OrbSpawner>();
        manageRitual = collision.GetComponent<ManageBloodRitual>();
        if (orbSpawner == null || manageRitual == null || manageRitual.ritualCenter == null) return;

        altarTransform = manageRitual.ritualCenter;
        if (orbSpawner.IsCompleted()) return;

        if (sacrificeIcon != null) sacrificeIcon.SetActive(true);
        manageRitual.OnCompleted -= DeactivateSacrificeIconOnComplete;
        manageRitual.OnCompleted += DeactivateSacrificeIconOnComplete;
    }

    private void DeactivateSacrificeIconOnComplete()
    {
        if (sacrificeIcon != null) sacrificeIcon.SetActive(false);
        if (manageRitual != null) manageRitual.OnCompleted -= DeactivateSacrificeIconOnComplete;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Altar")) return;

        if (sacrificeIcon != null) sacrificeIcon.SetActive(false);
        if (manageRitual != null) manageRitual.OnCompleted -= DeactivateSacrificeIconOnComplete;
        orbSpawner = null;
        manageRitual = null;
        altarTransform = null;
    }

    private void HandleDeath()
    {
        if (orbSpawner == null || altarTransform == null ||
            Vector2.Distance(altarTransform.position, transform.position) >= orbSpawner.GetRange())
            return;

        for (int i = 0; i < numberOfSoulsToSpawn; i++)
        {
            Vector3 spawnPosition = transform.position;
            if (i > 0)
                spawnPosition += new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0f);
            orbSpawner.SpawnOrb(spawnPosition);
        }
    }
}