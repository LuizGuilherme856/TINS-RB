using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Points_StatsChange", menuName = "Upgrades/StatsChange/Points")]
public class Points_StatsChange : StatsChange
{
    [Header("Alterações de stats relacionadas aos pontos.")]
    [Space(30)]

    [SerializeField] private VariableOperation<int> extraPointsPerKill;
    [SerializeField] private VariableOperation<int> extraInitialPoints;

    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        globalStats.ChangeExtraPointsPerKill(extraPointsPerKill.variable, extraPointsPerKill.operationType);
        globalStats.ChangeExtraInitialPoints(extraInitialPoints.variable, extraInitialPoints.operationType);
    }
}
