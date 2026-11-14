using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keep ScriptableObject references here as a fallback when lookup fails.
/// </summary>
public class HolderScriptableObjects : MonoBehaviour
{
    public RunData runData;
    public RuneInventory runeInventory;
    public GlobalStats globalStats;
    public StatsCalculator statsCalculator;
    public ButtonsData buttonsData;
    public LanguageData languageData;
    public SpawnManager spawnManager;
    public EnemyWaveManager enemyWaveManager;

}
