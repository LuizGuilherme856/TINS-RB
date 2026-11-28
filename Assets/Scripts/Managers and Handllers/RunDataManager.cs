using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Restore player state from RunData when changing scenes.
public class RunDataManager : MonoBehaviour
{
    [SerializeField] private RunData runData;
    [SerializeField] private GlobalStats globalStats;
    [SerializeField] private CharacterData defaultCharacterData;
    public bool onMenu = false; //Skip updates in menus.
    
    private PlayerDataHolder playerDataHolder;
    private Health health;
    private PointManager pointManager;
    private GunInventory gunInventory;
    private CharacterDataManager characterDataManager;

    private void Awake()
    {
        if (onMenu) return; //Do nothing in menus.

        playerDataHolder = GetComponent<PlayerDataHolder>();
        characterDataManager = GetComponent<CharacterDataManager>();
        pointManager = FindFirstObjectByType<PointManager>();
        gunInventory = GetComponent<GunInventory>();

        if (globalStats == null) globalStats = AssetFinder.GetGlobalStats();

        health = GetComponent<Health>();
        // StartNewRun checks health in Start. Restore it before any Start runs.
        health?.SetCurrentHealth(runData.currentHp);
        if (runData.currentCharacter == null) runData.currentCharacter = defaultCharacterData;
    }

    private void OnEnable()
    {
        if (onMenu) return;
        if (health != null) health.OnHealthChange += UpdateHealth;
        if (pointManager != null) pointManager.OnPointChange += UpdatePoints;
        if (gunInventory != null) gunInventory.OnWeaponChange += UpdateGuns;
    }

    private void OnDisable()
    {
        if (health != null) health.OnHealthChange -= UpdateHealth;
        if (pointManager != null) pointManager.OnPointChange -= UpdatePoints;
        if (gunInventory != null) gunInventory.OnWeaponChange -= UpdateGuns;
    }

    private void Start()
    {
        if (!onMenu) pointManager?.SetPoints(runData.currentpoints);
    }

    // Reset the temporary attempt state here, not permanent PlayerData unlocks. Internal scene changes
    // restore from RunData instead of calling this new-run reset.
    public void ResetRunData()
    {
        Debug.Log("resetando a runData...");
        runData.ResetCurrentRunScoreFinalization();
        runData.ResetGameplayEnemySpawned();

        PlayerData playerData = playerDataHolder.GetPlayerData();
        CharacterData characterData = characterDataManager.GetCurrentCharacterData();

        runData.waveStatistics.Clear();

        runData.SetXp(0);
        runData.currentHp = playerData.GetInitialHealth(characterDataManager);

        runData.currentpoints = pointManager.GetInitialPoints(characterData.initialPoints);

        runData.SetPointsInThisRun(0);

        runData.guns.Clear();
        runData.guns.Add(characterData.defaultGun);
        gunInventory.ResetGuns(characterData.defaultGun);

        AddRandomGunToPlayer();

        runData.meleeWeapon = characterData.defaultMelee;
        runData.specialAbility = characterData.defaultSpecialAbility;
        runData.dashData = characterData.defaultDash;

        runData.numberOfPowerUpsCollected = 0;

        runData.SetCurrentWave(0);
        runData.enemiesAlive = 0;
        runData.currentEnemyWave = null;

        runData.SetEnemiesKilled(0);
        runData.timeElapsed = 0;

        if(runData.currentCharacter == null) runData.currentCharacter = defaultCharacterData;

    }

    private void AddRandomGunToPlayer()
    {
        if (globalStats.ForCurrentRun.numberOfRandomGunsToStartWith > 0)
        {
            for (int i = 0; i < globalStats.ForCurrentRun.numberOfRandomGunsToStartWith; i++)
            {
                int count = 0;
                GunData randomGun;
                //Only choose a weapon the player does not already own.
                do
                {
                    #region Redundance Check
                    //Bound retries to avoid remaining in the loop too long.
                    count++;
                    if (count > 100) 
                    {
                        Debug.LogError("Ap�s 100 tentativas, n�o conseguiu sortear uma arma que o player nao tivesse. Retornando");
                        return;
                    }

                    #endregion

                    randomGun = globalStats.ForCurrentRun.GetRandomGun();
                }
                while (gunInventory.HasThisGun(randomGun) == true);

                gunInventory.AddGunToInventory(randomGun);
            }

        }
    }

    private void UpdateHealth()
    {
        runData.currentHp = health.GetCurrentHealth();
    }

    private void UpdatePoints()
    {
        runData.currentpoints = pointManager.currentPoints;
    }

    private void UpdateGuns()
    {
        if (!gunInventory.IsCampaignLoadout)
            runData.guns = gunInventory.GetGuns();
    }

    public RunData GetRunData()
    {
        return runData;
    }

    public void EnemyKilled()
    {
        runData.AddEnemiesKilled(1);
    }

    /// <summary>
    /// Update elapsed time in seconds.
    /// </summary>
    /// <param name="timeInSeconds">Elapsed time in seconds.</param>
    public void TimeElapsed(float timeInSeconds) 
    {
        runData.timeElapsed += timeInSeconds;
    }

    public float GetTimeElapsed()
    {
        return runData.timeElapsed;
    }

    // public void SetCharacter(Character character)
    // {
    //     runData.currentCharacter= character;
    // }
}
