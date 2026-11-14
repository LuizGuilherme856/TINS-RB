using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Upgrades/Upgrade Data")]
[Serializable]
public class UpgradeData : GenericData
{
    [Space(20)]
    public List<StatsChange> statsChanges;
    public List<ActiveEffect> activeEffects;
    public List<Requirement> requirements;

    public Sprite icon;
    public GlobalStats globalStats;
    [Space(10)]
    public bool statsChangeOn2ndTime = false;
    public List<StatsChange> statsChangesOn2ndTime;

    /// <summary>
    /// Attempt an upgrade after checking its requirements.
    /// Apply its effects when every requirement is satisfied.
    /// isFirstOfItsType identifies the first purchase of this rune type.
    /// </summary>
    // Requirements may complete asynchronously. This loop has no scene/run cancellation token;
    // changing upgrade lifetime requires preventing late completion from modifying a newer run.
    public async void ExecuteUpgradeData(UpgradeManager upgradeManager, bool isFirstOfItsType)
    {
        if (globalStats == null)
        {
            Debug.LogWarning("Global stats esta nulo! Procurando ele automaticamente. Corrija isso pois procurar automaticamente" +
                "causa perdas de performance. Nome deste objeto: " + name);

            globalStats = AssetFinder.GetGlobalStats();
        }

        //TODO: Replace frame polling with events for requirements that change only on specific actions.
        bool requirementsCompleted;
        TriggerRequirementVerification(upgradeManager);

        //Check asynchronously until every requirement is satisfied.
        while (true)
        {
            requirementsCompleted = true; //Keep this inside the loop.
            foreach (Requirement requirementItem in requirements)
            {
                if (requirementItem.RequirementCompleted == false)
                {
                    requirementsCompleted = false;
                    break;
                }
            }

            if (requirementsCompleted) break;

            await Task.Yield(); //Yield asynchronously before checking again; this is not a strict Unity frame boundary.

        }

        ApplyStatsChanges(isFirstOfItsType);
        ApplyActiveEffects(upgradeManager);
    }


    private void ApplyActiveEffects(UpgradeManager upgradeManager)
    {
        // Debug.Log("Applying all active effects");
        foreach (var activeEffectsItem in activeEffects)
        {
            activeEffectsItem.TriggerActiveEffect(upgradeManager, globalStats);
        }
    }

    /// <summary>
    /// Apply each modification in StatsChanges.
    /// </summary>
    private void ApplyStatsChanges(bool isFirstOfItsType)
    {
        if (globalStats == null)
        {
            Debug.Log($"GlobalStats esta nulo na rune Data: {name}");
        }

        //Some runes change their effect after the first purchase.
        if(isFirstOfItsType || statsChangeOn2ndTime == false)
        {
            // Debug.Log("Applying all stat modifiers");
            foreach (StatsChange statsChangeItem in statsChanges)
            { 
                if(statsChangeItem == null)
                {
                    Debug.LogWarning("StatsChangeItem is null! UpgradeData: " + name);
                    continue;
                }
                 statsChangeItem.ExecuteStatsChange(globalStats);
            }
        }
        else
        {
            foreach (StatsChange statsChangeItem in statsChangesOn2ndTime)
            {
                if (statsChangeItem == null)
                {
                    Debug.LogWarning("StatsChangeItem is null! UpgradeData: " + name);
                    continue;
                }
                statsChangeItem.ExecuteStatsChange(globalStats);
            }

        }

    }

    private void TriggerRequirementVerification(UpgradeManager upgradeManager)
    {
        foreach (Requirement requirementItem in requirements)
        {
            requirementItem.CheckRequirements(upgradeManager);
        }
    }
}
