using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ElementalTrail_StatsChange", menuName = "Upgrades/StatsChange/Elemental Trail")]
public class ElementalTrail_StatsChange : StatsChange
{
    public DamageType elementalTrailDamageType;

    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        globalStats.ChangeWalkTrailDamageType(elementalTrailDamageType, OperationType.Replace);
    }


}
