using System;
using System.Collections;
using System.Collections.Generic;
using TechBooh;
using UnityEngine;
using UnityEngine.SceneManagement;

// Maps gameplay events to Steam achievement IDs. Preserve the unlock conditions while adapting
// the platform service; development reset methods must remain inaccessible to release users.
public class AchievementManager : MonoBehaviour
{

    #region ReadOnlyStrings
    //Achievement Strings
    private readonly string FIRST_DEATH = "FIRST_DEATH";
    private readonly string DEATH_100 = "DEATHS_100";
    private readonly string DEATH_333 = "DEATHS_1000";
    private readonly string CHALICE = "RECEIVED_CHALICE";
    private readonly string SHOOT_STRANGER = "SHOOT_STRANGER";
    private readonly string WAVE_5_DEATH = "WAVE_5_DEATH";
    private readonly string WAVE_10_REACHED = "WAVE_10_REACHED";
    private readonly string WAVE_15_REACHED = "WAVE_15_REACHED";
    private readonly string WAVE_20_REACHED = "WAVE_20_REACHED";
    private readonly string WAVE_30_REACHED = "WAVE_30_REACHED";
    private readonly string WAVE_40_REACHED = "WAVE_40_REACHED";
    private readonly string WAVE_50_REACHED = "WAVE_50_REACHED";
    private readonly string KILLS_100 = "KILLS_100";
    private readonly string KILLS_1000 = "KILLS_1000";
    private readonly string KILLS_10000 = "KILLS_10000";
    private readonly string KILLS_100000 = "KILLS_100000";
    private readonly string BOSS_GORFO = "BOSS_GORFO";
    private readonly string BOSS_BELLS = "BOSS_BELLS";
    private readonly string BOSS_ALKARION = "BOSS_ALKARION";
    private readonly string BOSS_GAEL = "BOSS_GAEL"; /// TODO: Add this achievement on Steam.
    private readonly string BOSS_PSYCHOPATH = "BOSS_PSYCHOPATH";
    private readonly string BOSS_LUCIFER = "BOSS_LUCIFER";
    private readonly string ENTER_CHURCH = "ENTER_CHURCH";
    private readonly string ENTER_CATACOMBS = "ENTER_CATACOMBS";
    private readonly string ENTER_HELL = "ENTER_HELL";
    private readonly string ENTER_STAIRWAY = "ENTER_STAIRWAY";
    private readonly string ENTER_HEAVEN = "ENTER_HEAVEN";
    private readonly string UPGRADE_WEAPON = "UPGRADE_WEAPON_ANGEL";
    private readonly string LIGHT_LANTERN = "LIGHT_LANTERN_CATACOMBS";
    private readonly string OPEN_ALL_MAIDENS = "OPEN_ALL_IRON_MAIDENS";
    private readonly string BREAK_20_LAMPS = "BREAK_20_LAMPS";
    private readonly string ALL_ACHIEVEMENTS = "ALL_ACHIEVEMENTS";
    //Scene Strings
    private const string MAP_CEM = "Cemetery gates";
    private const string MAP_CHURCH = "Cathedral inside";
    private const string SAFE_CATACOMBS = "Safe Zone Catacombs";
    private const string MAP_CATACOMBS = "Catacombs";
    private const string MAP_HELL = "Hell 2.0";
    private const string MAP_STAIRWAY = "Stairway To Heaven";
    private const string HEAVEN = "South of Heaven";


    #endregion


    protected Health playerHealth;
    [SerializeField] protected PlayerData playerData;
    [SerializeField] protected RunData runData;
    [Tooltip("So � preenchido manualmente no nivel do hell 2.0")]
    [SerializeField] protected BossFightArena targetBFA; // Assigned in the Editor in the Hell scene.

    protected bool checkKillsCoRoutineActive = false;
    [SerializeField] protected float targetDelayBetweenChecks = 5.0f;

    [Tooltip("Procura o pack a punch pra dar a conquista pro player. Nao procura por ele no menu e no hub")]
    public bool lookForPackaPunch = true;
    public void Awake()
    {

        //Reset event subscriptions to avoid duplicate listeners.
        playerData.ResetAchivementsEvents();
        runData.ResetAchivementsEvents();

        playerData.OnRegisterDeath += UnlockPlayerDeath;
        playerData.OnCheckFirstLine += UnlockChalice;
        playerData.OnLuciHit += UnlockLuciHit;
        playerData.OnAddTotalKills += CheckKills;
        runData.OnChangeWave += CheckWaveReached;



        /*
         *  Legacy coroutine-based checking implementation.
        if (!checkKillsCoRoutineActive)
        {
            StartCoroutine(CheckKills_CoRoutine(targetDelayBetweenChecks));
            checkKillsCoRoutineActive = true;
        }
         */



    }

    public void Start()
    {
        // TechBooh: retroactively grant the meta achievement to players who
        // unlocked all other achievements before it was added.
        TryUnlockAllAchievements_TechBooh();

        if (lookForPackaPunch == true)
        {
            // Find the scene's Pack-a-Punch station and subscribe to it.
            UpgradeWeapon packAPunch = Try.FindObjectOfType<UpgradeWeapon>();
            if (packAPunch != null)
                packAPunch.OnUpgrade += UnlockWeaponUpgrade; // Check for possible duplicate handling here.
        }

    }

    public void OnEnable()
    {
        CheckMapRelated();
    }
    public void UnlockAchievement(string achievementID)
    {
        try
        {
            var achiv = new Steamworks.Data.Achievement(achievementID);
            if (!achiv.State)
            {
                achiv.Trigger();
                Debug.Log($"Achievement {achievementID} unlocked!");
            }
            else
            {
                Debug.Log($"Achievement {achievementID} already unlocked.");
            }

            // Exclude the meta achievement from its own requirements.
            // All other achievements use this centralized check.
            if (achievementID != ALL_ACHIEVEMENTS)
            {
                TryUnlockAllAchievements_TechBooh(achievementID);
            }

        }
        catch (Exception exception)
        {
            Debug.LogError($"Erro ao aplicar conquista {achievementID}: {exception.Message}");
        }

    }
    [ContextMenu("Resetar Achivements")]
    public void ClearAllAchievmentStatus()
    {
        ClearAchievementStatus(FIRST_DEATH);
        ClearAchievementStatus(DEATH_100);
        ClearAchievementStatus(DEATH_333);
        ClearAchievementStatus(CHALICE);
        ClearAchievementStatus(SHOOT_STRANGER);
        ClearAchievementStatus(WAVE_5_DEATH);
        ClearAchievementStatus(WAVE_10_REACHED);
        ClearAchievementStatus(WAVE_15_REACHED);
        ClearAchievementStatus(WAVE_20_REACHED);
        ClearAchievementStatus(WAVE_30_REACHED);
        ClearAchievementStatus(WAVE_40_REACHED);
        ClearAchievementStatus(WAVE_50_REACHED);
        ClearAchievementStatus(KILLS_100);
        ClearAchievementStatus(KILLS_1000);
        ClearAchievementStatus(KILLS_10000);
        ClearAchievementStatus(KILLS_100000);
        ClearAchievementStatus(BOSS_GORFO);
        ClearAchievementStatus(BOSS_BELLS);
        ClearAchievementStatus(BOSS_ALKARION);
        ClearAchievementStatus(BOSS_PSYCHOPATH);
        ClearAchievementStatus(BOSS_LUCIFER);
        ClearAchievementStatus(ENTER_CHURCH);
        ClearAchievementStatus(ENTER_CATACOMBS);
        ClearAchievementStatus(ENTER_HELL);
        ClearAchievementStatus(ENTER_STAIRWAY);
        ClearAchievementStatus(ENTER_HEAVEN);
        ClearAchievementStatus(UPGRADE_WEAPON);
        ClearAchievementStatus(LIGHT_LANTERN);
        ClearAchievementStatus(OPEN_ALL_MAIDENS);
        ClearAchievementStatus(BREAK_20_LAMPS);
        ClearAchievementStatus(BOSS_GAEL);
        ClearAchievementStatus(ALL_ACHIEVEMENTS);
    }

    /// <summary>
    /// TechBooh: check all base Steam achievements and unlock the meta achievement
    /// when none are missing. Treat the supplied ID as already unlocked
    /// without waiting for the Steam cache to refresh.
    /// </summary>
    private void TryUnlockAllAchievements_TechBooh(string achievementUnlockedThisCall = null)
    {
        try
        {
            var allAchievements = GetAchievementsRequiredForCompletion_TechBooh();
            foreach (string achievementID in allAchievements)
            {
                if (achievementID == achievementUnlockedThisCall)
                {
                    continue;
                }

                var achievement = new Steamworks.Data.Achievement(achievementID);
                if (!achievement.State)
                {
                    return;
                }
            }

            var allAchievementsAchievement = new Steamworks.Data.Achievement(ALL_ACHIEVEMENTS);
            if (!allAchievementsAchievement.State)
            {
                allAchievementsAchievement.Trigger();
                Debug.Log($"Achievement {ALL_ACHIEVEMENTS} unlocked!");
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"Erro ao verificar a conquista {ALL_ACHIEVEMENTS}: {exception.Message}");
        }
    }

    /// <summary>
    /// TechBooh: single source of achievement requirements for completion.
    /// ALL_ACHIEVEMENTS is intentionally excluded from this list.
    /// </summary>
    private string[] GetAchievementsRequiredForCompletion_TechBooh()
    {
        return new[]
        {
            FIRST_DEATH,
            DEATH_100,
            DEATH_333,
            CHALICE,
            SHOOT_STRANGER,
            WAVE_5_DEATH,
            WAVE_10_REACHED,
            WAVE_15_REACHED,
            WAVE_20_REACHED,
            WAVE_30_REACHED,
            WAVE_40_REACHED,
            WAVE_50_REACHED,
            KILLS_100,
            KILLS_1000,
            KILLS_10000,
            KILLS_100000,
            BOSS_GORFO,
            BOSS_BELLS,
            BOSS_ALKARION,
            BOSS_GAEL,
            BOSS_PSYCHOPATH,
            BOSS_LUCIFER,
            ENTER_CHURCH,
            ENTER_CATACOMBS,
            ENTER_HELL,
            ENTER_STAIRWAY,
            ENTER_HEAVEN,
            UPGRADE_WEAPON,
            LIGHT_LANTERN,
            OPEN_ALL_MAIDENS,
            BREAK_20_LAMPS
        };
    }

    public void ClearAchievementStatus(string id)
    {
        var ach = new Steamworks.Data.Achievement(id);
        ach.Clear();
        Debug.Log($"Achievement {id} cleared");
    }
    /// <summary>
    /// Check achievements associated with player death counts.
    /// </summary>
    public void UnlockPlayerDeath()
    {

        //Use threshold ranges instead of equality so checks can recover after an early exit.
        //The achievement can then be granted by a later check.
        /*
         // Legacy implementation replaced for consistency.
                if (playerData.deaths >= 1 && playerData.deaths <= 10 )
        {
          UnlockAchievement(FIRST_DEATH);
        }
        else if (playerData.deaths >= 100 && playerData.deaths <= 110)
        {
            UnlockAchievement(DEATH_100);
        }
        else if (playerData.deaths >= 1000 && playerData.deaths <= 1020)
        {
            UnlockAchievement(DEATH_1000);
        }

         */

        if (playerData.deaths >= 333)
        {
            UnlockAchievement(DEATH_333);
            UnlockAchievement(DEATH_100);
            UnlockAchievement(FIRST_DEATH);
        }
        else if (playerData.deaths >= 100)
        {
            UnlockAchievement(DEATH_100);
            UnlockAchievement(FIRST_DEATH);
        }
        else if (playerData.deaths >= 1)
        {
            UnlockAchievement(FIRST_DEATH);
        }




        //Check whether the player died on wave 5.
        if (runData.currentWaveNumber == 5)
        {
            UnlockAchievement(WAVE_5_DEATH);
        }

    }

    public void CheckWaveReached()
    {
        if (runData.currentWaveNumber >= 10 && runData.currentWaveNumber <= 12)
        {
            UnlockAchievement(WAVE_10_REACHED);
        }
        else if (runData.currentWaveNumber >= 15 && runData.currentWaveNumber <= 17)
        {
            UnlockAchievement(WAVE_15_REACHED);
        }
        else if (runData.currentWaveNumber >= 20 && runData.currentWaveNumber <= 22)
        {
            UnlockAchievement(WAVE_20_REACHED);
        }
        else if (runData.currentWaveNumber >= 30 && runData.currentWaveNumber <= 32)
        {
            UnlockAchievement(WAVE_30_REACHED);
        }
        else if (runData.currentWaveNumber >= 40 && runData.currentWaveNumber <= 42)
        {
            UnlockAchievement(WAVE_40_REACHED);
        }
        else if (runData.currentWaveNumber >= 50 && runData.currentWaveNumber <= 52)
        {
            UnlockAchievement(WAVE_50_REACHED);
        }
    }
    /// <summary>
    /// Check map-dependent achievements.
    /// </summary>
    public void CheckMapRelated()
    {

        switch (SceneManager.GetActiveScene().name)
        {
            case MAP_CEM:
                MapCem();
                break;
            case MAP_CHURCH:
                MapChurch();
                break;
            case MAP_CATACOMBS:
                MapCatacombs();

                break;
            case MAP_HELL:
                MapHell();
                break;
            case MAP_STAIRWAY:
                MapStair();
                break;
            case HEAVEN:
                MapHeaven();
                break;
            case SAFE_CATACOMBS:
                PrepareForLightAchievment();
                break;
            default:
                // Fallback for other or unidentified maps.
                break;
        }
    }

    private void MapCem()
    {
        targetBFA = Try.FindObjectOfType<BossFightArena>();
        targetBFA.ResetAchievmentCheck(); //Avoid duplicate listeners.
        targetBFA.OnAchievmentCheck += UnlockKillGorfo;

        try
        {
            BreakOnTouchManager targetBreakManager = Try.FindObjectOfType<BreakOnTouchManager>();
            targetBreakManager.OnBreakAll += UnlockBreackLamp;// When every object must be broken.
            targetBreakManager.OnBreakMin += UnlockBreackLamp;// When a specified count must be broken.

        }
        catch (System.Exception)
        {
            Debug.LogError("Tentou ajustar a consquista das lamps");
            //throw;
        }

    }

    private void MapChurch()
    {
        UnlockAchievement(ENTER_CHURCH);
        // Cathedral-specific logic.
        targetBFA = Try.FindObjectOfType<BossFightArena>();
        targetBFA.ResetAchievmentCheck();
        targetBFA.OnAchievmentCheck += UnlockKillBells;
    }

    private void MapHeaven()
    {
        // Heaven-specific logic.
        UnlockAchievement(ENTER_HEAVEN);
        targetBFA = Try.FindObjectOfType<BossFightArena>();
        targetBFA.ResetAchievmentCheck(); //Avoid duplicate listeners.
        targetBFA.OnAchievmentCheck += UnlockKillLucifer;
    }

    private void MapStair()
    {
        // Stairway to Heaven-specific logic.
        UnlockAchievement(ENTER_STAIRWAY);
        targetBFA = Try.FindObjectOfType<BossFightArena>();
        targetBFA.ResetAchievmentCheck(); //Avoid duplicate listeners.
        targetBFA.OnAchievmentCheck += UnlockKillPato;
    }

    private void MapHell()
    {
        // Hell-specific logic.
        UnlockAchievement(ENTER_HELL);
        try
        {
            targetBFA.ResetAchievmentCheck(); //Avoid duplicate listeners.
            targetBFA.OnAchievmentCheck += UnlockKillGael;
        }
        catch (System.Exception)
        {
            Debug.LogError("N�o conseguiu linkar o achievment de matar o boss hell, verifique se a arena esta linkada ao achievment manager no mapa hell 2.0");
        }
        //TODO: Account for the two arenas here.
    }

    private void MapCatacombs()
    {
        // Catacombs-specific logic.
        UnlockAchievement(ENTER_CATACOMBS);
        targetBFA = Try.FindObjectOfType<BossFightArena>();
        targetBFA.ResetAchievmentCheck(); //Avoid duplicate listeners.
        targetBFA.OnAchievmentCheck += UnlockKillAlkarion;
        //Light achievement.
        PrepareForLightAchievment();
        //Iron achievement.
        try
        {
            IronMaidensManager targetIronManager = Try.FindObjectOfType<IronMaidensManager>();
            targetIronManager.ResetAchEvents();//Avoid duplicate listeners.
            targetIronManager.OnTriggerIronACH += UnlockIron;
        }
        catch (System.Exception)
        {
            Debug.LogError("tentou preparar ach das iron maidens");
            throw;
        }

    }

    private void PrepareForLightAchievment()
    {
        //The Catacombs includes the lamp achievement.
        TurnOnLight[] targetLightList = Try.FindObjectsOfType_Plural<TurnOnLight>();
        foreach (TurnOnLight targetLight in targetLightList)
        {
            targetLight.OnTurnOnLight += UnlockTurnOnLight;
        }
    }

    public void UnlockChalice()
    {
        UnlockAchievement(CHALICE);
    }

    public void UnlockLuciHit()
    {
        UnlockAchievement(SHOOT_STRANGER);
    }


    #region KillBoosRelated
    public void UnlockKillGorfo()
    {
        UnlockAchievement(BOSS_GORFO);
    }

    public void UnlockKillBells()
    {
        UnlockAchievement(BOSS_BELLS);
    }

    public void UnlockKillAlkarion()
    {
        UnlockAchievement(BOSS_ALKARION);
    }

    public void UnlockKillGael()
    {
        UnlockAchievement(BOSS_GAEL);
    }

    public void UnlockKillPato()
    {
        UnlockAchievement(BOSS_PSYCHOPATH);
    }

    public void UnlockKillLucifer()
    {
        UnlockAchievement(BOSS_LUCIFER);
    }
    #endregion

    public void UnlockWeaponUpgrade()
    {
        UnlockAchievement(UPGRADE_WEAPON);
    }

    public void UnlockTurnOnLight()
    {
        UnlockAchievement(LIGHT_LANTERN);
    }

    public void UnlockIron()
    {
        UnlockAchievement(OPEN_ALL_MAIDENS);
    }

    public void UnlockBreackLamp()
    {
        UnlockAchievement(BREAK_20_LAMPS);
    }
    /// <summary>
    /// Retained for possible future coroutine use.
    /// 
    /// </summary>
    /// <param name="delayBetweenChecks"></param>
    /// <returns></returns>
    protected IEnumerator CheckKills_CoRoutine(float delayBetweenChecks = 5f)
    {
        while (true)
        {
            if (playerData.totalKillsSoFar >= 100 && playerData.totalKillsSoFar <= 120)
            {
                UnlockAchievement(KILLS_100);
            }
            else if (playerData.totalKillsSoFar >= 1000 && playerData.totalKillsSoFar <= 1050)
            {
                UnlockAchievement(KILLS_1000);
            }
            else if (playerData.totalKillsSoFar >= 10000 && playerData.totalKillsSoFar <= 10100)
            {
                UnlockAchievement(KILLS_10000);
            }
            else if (playerData.totalKillsSoFar >= 100000 && playerData.totalKillsSoFar <= 100100)
            {
                UnlockAchievement(KILLS_100000);
            }

            yield return new WaitForSeconds(delayBetweenChecks);
        }
    }

    public void CheckKills()
    {
        if (playerData.totalKillsSoFar >= 100000)
        {
            UnlockAchievement(KILLS_100000);
            UnlockAchievement(KILLS_10000);
            UnlockAchievement(KILLS_1000);
            UnlockAchievement(KILLS_100);
        }
        else if (playerData.totalKillsSoFar >= 10000)
        {
            UnlockAchievement(KILLS_10000);
            UnlockAchievement(KILLS_1000);
            UnlockAchievement(KILLS_100);
        }
        else if (playerData.totalKillsSoFar >= 1000)
        {
            UnlockAchievement(KILLS_1000);
            UnlockAchievement(KILLS_100);
        }
        else if (playerData.totalKillsSoFar >= 100)
        {
            UnlockAchievement(KILLS_100);
        }

    }

}
