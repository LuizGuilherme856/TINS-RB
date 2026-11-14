using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BossFightArena))]
public class GivePlayerAmmoOnBossFight : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private GameObject storkPrefab; // Stork prefab; may be null when reusing a scene instance.
    [SerializeField] private Transform spawnPoint; // Optional Stork spawn/placement position.

    [Header("Referências de arma")]
    // Prefer GunInventory.AnyGunWithoutAmmo().
    [SerializeField] private GunInventory gunInventory;


    [Header("Config")]
    [SerializeField] private float spawnCooldownSeconds = 45f;

    private BossFightArena bossFightArena;
    private bool isChecking = false;
    private float lastSpawnTime = -Mathf.Infinity;

    private bool justSpawned = true;

    private void Awake()
    {
        bossFightArena = GetComponent<BossFightArena>();
        if (bossFightArena == null)
            Debug.LogError("GivePlayerAmmoOnBossFight precisa de BossFightArena no mesmo GameObject.");
    }

    private void OnEnable()
    {
        if (bossFightArena != null)
        {
            bossFightArena.OnStartBossFight += OnBossFightStarted;
            bossFightArena.OnEndBossFight -= OnBossFightStarted;
        }
    }

    private void OnDisable()
    {
        if (bossFightArena != null)
        {
            bossFightArena.OnStartBossFight -= OnBossFightStarted;
        }
    }

    private void OnBossFightStarted()
    {
        if (isChecking) return;
        isChecking = true;
        StartCoroutine(CheckAndSpawnLoop());
    }

    private IEnumerator CheckAndSpawnLoop()
    {
        // Check immediately when combat begins, then every ten seconds.
        // Spawning still requires spawnCooldownSeconds since the previous spawn.
        while (true)
        {
            if (justSpawned)
            {
                // Prevent another spawn immediately after the previous one.
                justSpawned = false;
                // Uncomment to wait for the initial cooldown before checking.
                 yield return new WaitForSeconds(spawnCooldownSeconds);
            }

            TrySpawnStorkIfNeeded();

            yield return new WaitForSeconds(10f); // Check every ten seconds.
        }
    }

    private void TrySpawnStorkIfNeeded()
    {
        // Do not spawn until the minimum spawn cooldown expires.
        if (Time.time - lastSpawnTime < spawnCooldownSeconds) return;

        // Try the GunInventory method first.
        if (gunInventory != null)
        {
            bool anyWithoutAmmo = false;
            try
            {
                anyWithoutAmmo = gunInventory.AnyGunWithoutAmmo();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"GivePlayerAmmoOnBossFight: erro ao chamar AnyGunWithoutAmmo(): {e.Message}");
                anyWithoutAmmo = false;
            }

            if (anyWithoutAmmo)
            {
                SpawnOrActivateStork();
                lastSpawnTime = Time.time;
            }

            return;
        }

    }

    private void SpawnOrActivateStork()
    {
        justSpawned = true;

        if (storkPrefab != null)
        {
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            Instantiate(storkPrefab, pos, Quaternion.identity);
            return;
        }

        Debug.LogWarning("GivePlayerAmmoOnBossFight: nenhum storkPrefab nem storkInstance atribuído.");
    }
}
