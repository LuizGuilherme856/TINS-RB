using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RunStart_StatsChange", menuName = "Upgrades/StatsChange/RunStart")]
public class RunStart_StatsChange : StatsChange
{
    [Header("Alterações de stats relacionadas ao início da run.")]
    [Space(30)]

    [SerializeField] private VariableOperation<int> numberOfRandomRunesToStartWith;
    [SerializeField] private VariableOperation<int> numberOfRandomGunsToStartWith;

    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        globalStats.ChangeNumberOsRandomRunesToStartWith(numberOfRandomRunesToStartWith.variable, numberOfRandomRunesToStartWith.operationType);
        globalStats.ChangeNumberOsRandomGunsToStartWith(numberOfRandomGunsToStartWith.variable, numberOfRandomGunsToStartWith.operationType);
    }
}
