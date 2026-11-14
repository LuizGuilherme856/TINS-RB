using UnityEngine;

//[CreateAssetMenu(fileName = "StatsChange", menuName = "Upgrades/StatsChange")]

/// <summary>
/// Base class for concrete stat modifications applied to
/// globalStats
/// </summary>
public abstract class StatsChange : ScriptableObject
{
    public abstract void ExecuteStatsChange(GlobalStats globalStats);
}
