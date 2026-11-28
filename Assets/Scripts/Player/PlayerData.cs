using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//[CreateAssetMenu(fileName = "player", menuName = "player")]
/// <summary>
/// Stores persistent player progression and blessings.
/// </summary>
public class PlayerData : ScriptableObject
{
    [SerializeField] private RuneInventory runeUpgradeManager;
    public List<SavebleSkillUpgrade> skillUpgrades = new();
    [field: SerializeField] public bool isTutorialFinished { get;  set; }
    [field: SerializeField] public bool isBeta { get; set; }

    [Header("MOVEMENT")]
    public float walkSpeed;
    public float minVelocity;

    [Header(" # -- Player xp")]
    [SerializeField] private int skillPoints;
    public int level;
    public readonly int maxLevel = 55;
    public int xp;
    public int horns;
    [SerializeField] private AnimationCurve xpCurve; // Curve: X = level, Y = required XP.



    [Header(" # -- Statstics")]
    public List<Score> scores = new List<Score>();
    //[HideInInspector] public int extraHP { get;  set; }
    public int deaths, totalKillsSoFar;
    public int runsStarted;
    [NonSerialized] private bool hasRegisteredCurrentRunStart_TechBooh;


    [Space(10)]
    [Header(" # -- Unlockables ")]
    public bool isSkillTreeUnlocked = false; //Whether the skill tree is unlocked.
    public bool isSkillTreeNpcSafe = false; //Whether the skill-tree NPC has been rescued.
    public bool isHealerNpcUnlocked = false; //Whether the healer NPC is unlocked.
    public bool isShopNpcUnlocked = false; //Whether the shop NPC is unlocked.
    public bool isTigerNpcUnlocked = false; //Whether the tiger NPC is unlocked.
    //characters
    [field: SerializeField] public bool isNinaUnlocked { get; private set; }
    [field: SerializeField] public bool isHanbaUnlocked { get; private set; }
    [field: SerializeField] public bool isDeNiroUnlocked { get; private set; }
    [field: SerializeField] public bool isMikiUnlocked { get; private set; }

    public int luciferLinesSpoken = 0; //Number of Lucifer lines already spoken; selects the next dialogue line.

    [Space(30)]
    [Header("  # -- OTHER CONFIGS")]
     [HideInInspector]public bool showCheats = false;
    public string version = "Beta v0.7";
    
    public int numberOfTimesKilledLucifer = 0; //Number of Lucifer defeats; also indicates whether he has been defeated before.
    public bool isEndlessModeOn = false; //Whether Endless mode is enabled.
    public event Action OnHornsChange;
    public event Action OnSkillPointsChange;
    // Achivement Related
    public event Action OnRegisterDeath; //Player-death achievements are checked here.
    public event Action OnCheckFirstLine; // Chalice achievement.
    public event Action OnLuciHit; //Achievement for hitting Lucifer.
    public event Action OnAddTotalKills; //Achievement for hitting Lucifer.

    [SerializeField] private StatsCalculator statsCalculator;


    public bool showedEndlessModeStation;

    /// <summary>
    /// Represents the current state of the Gael boss fight.
    /// </summary>
    /// <remarks>0: not encountered; 1: encountered; 2: heard Lucifer's dialogue about him.</remarks>
    [Range(0, 2)] public int playerFacedGael = 0;
    /// <remarks>0: Heaven not reached; 1: reached; 2: heard Lucifer's dialogue about Heaven.</remarks>
    [Range(0, 2)] public int playerReachedHeaven;


    #region Generic datas
    //Reserved persisted fields for future data.
    //Rename a reserved field before using it, and review compatibility with existing saves.
    [HideInInspector] public bool genericBool_2;
    [HideInInspector] public bool genericBool_3;
    [HideInInspector] public bool genericBool_4;
    [HideInInspector] public bool genericBool_5;


    [HideInInspector] public int genericInt_3;
    [HideInInspector] public int genericInt_4;
    [HideInInspector] public int genericInt_5;

    [HideInInspector] public string genericString_1;
    [HideInInspector] public string genericString_2;
    [HideInInspector] public string genericString_3;
    [HideInInspector] public string genericString_4;
    [HideInInspector] public string genericString_5;
    #endregion

    // Both a missing file and a failed read currently lead to an in-memory reset. A later save can
    // overwrite the original slot; recovery UI/backups must be added before treating corruption as a new game.
    public void LoadPlayerData()
    {
        SavedData data = null;
        try
        {
            data = SaveSystem.LoadData(isBeta);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erro ao ler o save do player: {ex.Message}\n{ex.StackTrace}");
        }


        if (data == null)
        {
            Debug.Log("nao tinha nada salvo");
            ResetSavedData(); //Reset missing-save data so serialized Editor progress cannot become a new player's progress.
            return;
        }

        skillPoints = data.skillPoints;
        level = data.level;
        xp = data.xp;

        skillUpgrades = data.skillUpgrades;
        if (skillUpgrades == null) skillUpgrades = new(); //Initialize missing lists after loading to prevent null-reference errors.

        horns = data.horns;

        isNinaUnlocked = data.isNinaUnlocked;
        isHanbaUnlocked = data.isHanbaUnlocked;
        isDeNiroUnlocked = data.isDeNiroUnlocked;
        isMikiUnlocked = data.isMikiUnlocked;

        isSkillTreeUnlocked = data.isSkillTreeUnlocked;
        isSkillTreeNpcSafe = data.isSkillTreeNpcSafe; //Whether the skill-tree NPC has been rescued.
        isHealerNpcUnlocked = data.isHealerNpcUnlocked;
        isShopNpcUnlocked = data.isShopNpcUnlocked;
        isTigerNpcUnlocked = data.isTigerNpcUnlocked;

        isEndlessModeOn = data.isEndlessModeOn;
        luciferLinesSpoken = data.luciferLinesSpoken;

        scores = data.scores ?? new List<Score>();

        // TechBooh: remove legacy tutorial/Hub entries with wave zero
        // and migrate legacy zero-based attempt numbers.
        scores.RemoveAll(score => score == null || score.wave <= 0);
        foreach (Score score in scores)
        {
            score.attempt = Mathf.Max(1, score.attempt);
        }
        deaths = data.deaths;
        totalKillsSoFar = data.totalKillsSoFar;
        runsStarted = data.runsStarted;

        isTutorialFinished = data.isTutorialFinished;

        numberOfTimesKilledLucifer = data.numberOfTimesKilledLucifer;

        #region Generic Datas
        showedEndlessModeStation = data.showedEndlessModeStation;
        genericBool_2 = data.genericBool_2;
        genericBool_3 = data.genericBool_3;
        genericBool_4 = data.genericBool_4;
        genericBool_5 = data.genericBool_5;

        playerFacedGael = data.playerFacedGael;
        playerReachedHeaven = data.playerReachedHeaven;
        genericInt_3 = data.genericInt_3;
        genericInt_4 = data.genericInt_4;
        genericInt_5 = data.genericInt_5;

        genericString_1 = data.genericString_1;
        genericString_2 = data.genericString_2;
        genericString_3 = data.genericString_3;
        genericString_4 = data.genericString_4;
        genericString_5 = data.genericString_5;
        
        #endregion
    }

    // This resets only the fields listed below, not every persisted field. Keep defaults and migration
    // rules explicit when extending SavedData, particularly for profile switching or failed loads.
    private void ResetSavedData()
    {
        try
        {
            skillUpgrades.Clear();
        }
        catch
        {
            Debug.LogError("Erro ao zerar a lista de upgrades");
        }

        isNinaUnlocked = false;
        isHanbaUnlocked = false;
        isDeNiroUnlocked = false;
        isMikiUnlocked = false;

        isSkillTreeUnlocked = false;
        isHealerNpcUnlocked = false;
        isShopNpcUnlocked = false;
        isTigerNpcUnlocked = false;

        scores.Clear();
        deaths = 0;
        totalKillsSoFar = 0;
        runsStarted = 0;
        hasRegisteredCurrentRunStart_TechBooh = false;

        isTutorialFinished = false;
        
        #region Generic Datas
        showedEndlessModeStation = false;
        genericBool_2 = false;
        genericBool_3 = false;
        genericBool_4 = false;
        genericBool_5 = false;

        playerFacedGael = 0;
        playerReachedHeaven = 0;
        genericInt_3 = 0;
        genericInt_4 = 0;
        genericInt_5 = 0;

        genericString_1 = string.Empty;
        genericString_2 = string.Empty;
        genericString_3 = string.Empty;
        genericString_4 = string.Empty;
        genericString_5 = string.Empty;
        #endregion
    }


    public int GetInitialHealth(CharacterDataManager characters = null)
    {
        if (characters == null) characters = FindFirstObjectByType<CharacterDataManager>();
        int defaultHealth = (int)(characters?.GetCurrentCharacterData()?.defaultHp ?? 0f);
        return statsCalculator.CalculatePlayerInitialHealthPoints(defaultHealth);
    }

    public int GetSkillPoints()
    {
        return skillPoints;
    }

    public void AddSkillPoints(int amount)
    {
        if(skillPoints >= 100)
        {
            Debug.Log("ERRO! n�o deveria chegar a ter 100 skill points");
            return;
        }

        skillPoints += amount;
        OnSkillPointsChange?.Invoke();
    }

    public void RemoveSkillPoints(int amount)
    {
        if (skillPoints - amount < 0)
        {
            Debug.Log("ERRO! n�o deveria chegar a ter menos de 0 skill points");
            return;
        }
        skillPoints -= amount;
        OnSkillPointsChange?. Invoke();
    }

    public void SetSkillPoints(int amount)
    {
        if (amount < 0)
        {
            Debug.Log("ERRO! n�o deveria chegar a ter menos de 0 skill points");
            return;
        }
        if (amount >= 100)
        {
            Debug.Log("ERRO! n�o deveria chegar a ter 100 skill points");
            return;
        }
        skillPoints = amount;
        OnSkillPointsChange?.Invoke();
    }

    public int GetAttemptNumber()
    {
        // TechBooh: attempts are started-run ordinals, not death counts.
        // The first attempt is 1, and restarting or leaving advances correctly.
        return Mathf.Max(1, runsStarted);
    }

    public int GetNumberOfDeaths()
    {
        return deaths;
    }

    public void AddDeath()
    {
        deaths++;
        OnRegisterDeath?.Invoke();
    }

    /// <summary>
    /// TechBooh:
    /// Several flows may try to start the same run more than once.
    /// For example, during startup and again during death or reload handling.
    /// This guard increments runsStarted only once per actual run.
    /// </summary>
    public void RegisterRunStart_TechBooh()
    {
        if (hasRegisteredCurrentRunStart_TechBooh)
        {
            return;
        }

        runsStarted++;
        hasRegisteredCurrentRunStart_TechBooh = true;
    }

    public void ResetRunStartRegistration_TechBooh()
    {
        hasRegisteredCurrentRunStart_TechBooh = false;
    }

    public void ResetAchivementsEvents()
    {
        OnRegisterDeath = null;
        OnCheckFirstLine = null;
        OnLuciHit = null;
        OnAddTotalKills = null;
    }

    public void AddLuciLineSpoken()
    {
        luciferLinesSpoken++;
        OnCheckFirstLine?.Invoke();
    }

    public void TriggerCheckFirstLine()
    {
        OnCheckFirstLine?.Invoke();
    }

    public void TriggerLuciHit()
    {
        OnLuciHit?.Invoke();
    }

    public int GetTotalKills()
    {
        return totalKillsSoFar;
    }

    public void AddToTotalKills(int kills)
    {
        totalKillsSoFar += kills;
        OnAddTotalKills?.Invoke();
    }



    public void AddHorns(int quantity)
    {
        horns += quantity;
        OnHornsChange?.Invoke();
    }


    public void UnlockNina() { isNinaUnlocked = true; }
    public void UnlockHanba() { isHanbaUnlocked = true; }
    public void UnlockDeNiro() { isDeNiroUnlocked = true; }
    
    public void UnlockMiki() { isMikiUnlocked = true; }


public int CalculateXpToNextLevel()
    {
        if (xpCurve != null && xpCurve.length > 0)
        {
            // Evaluate the curve at the current level.
            float xp = xpCurve.Evaluate(level);
            return Mathf.FloorToInt(xp);
        }
        // Use the legacy formula if no curve is assigned.
        return (int)Math.Floor(925 + 71.8 * level + 43.1 * Math.Pow(level, 2.0));
    }


}
