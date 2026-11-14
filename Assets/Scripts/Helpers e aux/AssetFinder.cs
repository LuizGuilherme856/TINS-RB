using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static helper for accessing ScriptableObjects.
/// </summary>
public static class AssetFinder 
{
    public static EnemyWaveManager GetEnemyWaveManager()
    {
        return GameObject.FindGameObjectWithTag("ObjectHolder").GetComponent<HolderScriptableObjects>().enemyWaveManager;
    }

    public static RuneInventory GetRuneInventory()
    {
        return GameObject.FindGameObjectWithTag("ObjectHolder").GetComponent<HolderScriptableObjects>().runeInventory;
    }

    public static RunData GetRunData()
    {
        return GameObject.FindGameObjectWithTag("ObjectHolder").GetComponent<HolderScriptableObjects>().runData;
    }

    public static LanguageData GetLanguageData()
    {
        return GameObject.FindGameObjectWithTag("ObjectHolder").GetComponent<HolderScriptableObjects>().languageData;
    }

    public static ButtonsData GetButtonsData()
    {
        return GameObject.FindGameObjectWithTag("ObjectHolder").GetComponent<HolderScriptableObjects>().buttonsData;
    }

    public static GlobalStats GetGlobalStats()
    {
        return GameObject.FindGameObjectWithTag("ObjectHolder").GetComponent<HolderScriptableObjects>().globalStats;
    }

    public static StatsCalculator GetStatsCalculator()
    {
        return GameObject.FindGameObjectWithTag("ObjectHolder").GetComponent<HolderScriptableObjects>().statsCalculator;
    }

    public static SpawnManager GetSpawnManager()
    {
        return GameObject.FindGameObjectWithTag("ObjectHolder").GetComponent<HolderScriptableObjects>().spawnManager;
    }
}

