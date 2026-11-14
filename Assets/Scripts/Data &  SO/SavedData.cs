using System.Collections.Generic;


//Player fields included in the persistent save.

[System.Serializable]
// On-disk contract: field names and serialized types affect compatibility with existing BinaryFormatter saves.
// Renaming or repurposing fields requires a migration decision, not just an Inspector change.
public class SavedData 
{
    public int skillPoints;
    public int level;
    public int xp;
    public int horns;

    public List<SavebleSkillUpgrade> skillUpgrades = new();

    public bool isNinaUnlocked;
    public bool isHanbaUnlocked;
    public bool isDeNiroUnlocked;
    public bool isMikiUnlocked;

    public bool isSkillTreeUnlocked = false; //Whether the skill tree is unlocked.
    public bool isSkillTreeNpcSafe = false; //Whether the skill-tree NPC has been rescued.
    public bool isHealerNpcUnlocked = false; //Whether the healer NPC is unlocked.
    public bool isShopNpcUnlocked = false; //Whether the shop NPC is unlocked.
    public bool isTigerNpcUnlocked = false; //Whether the tiger NPC is unlocked.

    public bool isEndlessModeOn = false;
    public int luciferLinesSpoken;

    public bool isTutorialFinished;

    public bool[] isLevelUnlocked = new bool[10];

    public List<Score> scores = new();
    public int deaths;//Number of deaths.
    public int totalKillsSoFar;
    public int runsStarted;
    public int numberOfTimesKilledLucifer;

     #region Generic datas
    //Reserved persisted fields for future data.
    //Rename a reserved field before using it, and review compatibility with existing saves.
    public bool showedEndlessModeStation;
    public bool genericBool_2;
    public bool genericBool_3;
    public bool genericBool_4;
    public bool genericBool_5;

    public int playerFacedGael;
    public int playerReachedHeaven;
    public int genericInt_3;
    public int genericInt_4;
    public int genericInt_5;

    public string genericString_1;
    public string genericString_2;
    public string genericString_3;
    public string genericString_4;
    public string genericString_5;

    #endregion

    public SavedData(PlayerData playerData)
    {
        skillPoints = playerData.GetSkillPoints();
        level = playerData.level;
        xp = playerData.xp;
        skillUpgrades = playerData.skillUpgrades;

        horns = playerData.horns;

        isNinaUnlocked = playerData.isNinaUnlocked;
        isHanbaUnlocked = playerData.isHanbaUnlocked;
        isDeNiroUnlocked = playerData.isDeNiroUnlocked;
        isMikiUnlocked = playerData.isMikiUnlocked;

        isSkillTreeUnlocked = playerData.isSkillTreeUnlocked;
        isSkillTreeNpcSafe = playerData.isSkillTreeNpcSafe;
        isHealerNpcUnlocked = playerData.isHealerNpcUnlocked;
        isShopNpcUnlocked = playerData.isShopNpcUnlocked;
        isTigerNpcUnlocked = playerData.isTigerNpcUnlocked;

        luciferLinesSpoken = playerData.luciferLinesSpoken;

        isEndlessModeOn = playerData.isEndlessModeOn;

        scores = playerData.scores;
        deaths = playerData.deaths;
        totalKillsSoFar = playerData.totalKillsSoFar;
        runsStarted = playerData.runsStarted;

        isTutorialFinished = playerData.isTutorialFinished;

        numberOfTimesKilledLucifer = playerData.numberOfTimesKilledLucifer;

        #region  Generic Data
        showedEndlessModeStation = playerData.showedEndlessModeStation;
        genericBool_2 = playerData.genericBool_2;
        genericBool_3 = playerData.genericBool_3;
        genericBool_4 = playerData.genericBool_4;
        genericBool_5 = playerData.genericBool_5;

        playerFacedGael = playerData.playerFacedGael;
        playerReachedHeaven = playerData.playerReachedHeaven;
        genericInt_3 = playerData.genericInt_3;
        genericInt_4 = playerData.genericInt_4;
        genericInt_5 = playerData.genericInt_5;

        genericString_1 = playerData.genericString_1;
        genericString_2 = playerData.genericString_2;
        genericString_3 = playerData.genericString_3;
        genericString_4 = playerData.genericString_4;
        genericString_5 = playerData.genericString_5;
        #endregion
    }
}


