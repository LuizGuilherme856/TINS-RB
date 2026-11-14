using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// At the start of a run, apply UpgradeData for every owned permanent skill.
/// Update GlobalStats.
/// </summary>
public class SkillManager : MonoBehaviour
{
    [SerializeField] private UpgradeManager upgradeManager;

    [SerializeField] private PlayerData playerData;

    public List<UpgradeData> upgradeDataList;



    public void ExecuteSkillUpgrades()
   {


            foreach (SavebleSkillUpgrade skillUpgrade in playerData.skillUpgrades)
            {
                try
                {
                    Debug.Log("Executando skill upgrade: " + skillUpgrade.upgradeDataName);
                    GetUpgradeDataFromName(skillUpgrade.upgradeDataName).ExecuteUpgradeData(upgradeManager, true);

                }
                catch
                {
                    Debug.LogError("Erro ao aplicar skillUpgrade.upgradeDataName. ignorando ela");
                }
            }


    }

    public void AddSkillUpgrade(SkillUpgrade skillUpgrade)
    {
        if(playerData.skillUpgrades.Count >= 100)
        {
            Debug.LogError("Tentando adicionar mais de 100 skill upgrades. Isso � um erro!"); //Invalid state, but keep the game running if it occurs.
            return;
        }


        Debug.Log("Adicionando skill upgrade: " + skillUpgrade.upgradeData.name);
        playerData.skillUpgrades.Add(GetSavebleSkillUpgrade(skillUpgrade));

    }

    public void RemoveSkillUpgrade(SkillUpgrade skillUpgrade)
    {
        playerData.skillUpgrades.Remove(GetSavebleSkillUpgrade( skillUpgrade));
    }



    public UpgradeData GetUpgradeDataFromName(string upgradeDataName)
    {
        foreach (UpgradeData upgradeData in upgradeDataList)
        {
            if (upgradeData.name.Equals(upgradeDataName))
            {
                return upgradeData;
            }
        }

        Debug.LogError("Nao achou o upgrade data com o nome: " + upgradeDataName);
        return null;
    }

    public SavebleSkillUpgrade GetSavebleSkillUpgrade(SkillUpgrade skillUpgrade)
    {
        return new SavebleSkillUpgrade(skillUpgrade.upgradeData.name, skillUpgrade.ID);
    }


#if UNITY_EDITOR

    /// <summary>
    /// Discover UpgradeData assets automatically instead of assigning them individually.
    /// </summary>
    [ContextMenu(nameof(ReadUpgradeDataFromDisc))]
    private void ReadUpgradeDataFromDisc()
    {
        upgradeDataList = GetAllUpgradeData();
    }

    //Collect all UpgradeData assets because this component requires the complete list.
    //Automatically.

    public static List<UpgradeData> GetAllUpgradeData()
    {
        List<UpgradeData> upgradeDataList = new List<UpgradeData>();
        string[] guids = AssetDatabase.FindAssets("t:UpgradeData", new[] { "Assets" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            UpgradeData upgradeData = AssetDatabase.LoadAssetAtPath<UpgradeData>(path);
            if (upgradeData != null)
            {
                upgradeDataList.Add(upgradeData);
            }
        }

        return upgradeDataList;
    }
#endif

}

[Serializable]
public struct SkillUpgrade
{
    public UpgradeData upgradeData;
    /// <summary>
    /// Unique skill ID.
    /// </summary>
    public string ID;
    public SkillUpgrade(UpgradeData upgradeData, string ID)
    {
        this.upgradeData = upgradeData;
        this.ID = ID;
    }
}

/// <summary>
/// Serializable skill upgrade; store UpgradeData's name as a string.
/// Do not serialize the ScriptableObject instance directly.
/// </summary>
 [Serializable]
public struct SavebleSkillUpgrade
{
    public string upgradeDataName;

    /// <summary>
    /// Unique skill ID.
    /// </summary>
    public string ID;
    public SavebleSkillUpgrade(string upgradeDataName, string ID)
    {
        this.upgradeDataName = upgradeDataName;
        this.ID = ID;
    }
}

