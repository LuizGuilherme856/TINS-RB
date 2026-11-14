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

        playerDataHolder = FindFirstObjectByType<PlayerDataHolder>();
        characterDataManager = GetComponent<CharacterDataManager>();
        pointManager = FindFirstObjectByType<PointManager>();
        gunInventory = GetComponent<GunInventory>();

        if (globalStats == null) globalStats = AssetFinder.GetGlobalStats();

        if (pointManager == null)
        {
            pointManager = FindFirstObjectByType<PointManager>();
            Debug.LogError("Erro nao achou o point manager");
        }

        health = GetComponent<Health>();
        health.OnHealthChange += UpdateHealth;

        try
        {
            pointManager.OnPointChange += UpdatePoints;

        }
        catch (System.Exception)
        {

            Debug.Log("Consertar");
        }
        gunInventory.OnWeaponChange += UpdateGuns;
        
          if(runData.currentCharacter == null) runData.currentCharacter = defaultCharacterData;
    }


    void Start()
    {
        if(onMenu) return; //Do nothing in menus.
        health.SetCurrentHealth(runData.currentHp);
        pointManager.SetPoints(runData.currentpoints);
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
        runData.currentHp = playerData.GetInitialHealth();

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
        if (globalStats.numberOfRandomGunsToStartWith > 0)
        {
            for (int i = 0; i < globalStats.numberOfRandomGunsToStartWith; i++)
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

                    randomGun = globalStats.GetRandomGun();
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
