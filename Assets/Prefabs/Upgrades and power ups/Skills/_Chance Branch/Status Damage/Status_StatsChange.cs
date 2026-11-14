using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Status_StatsChange", menuName = "Upgrades/StatsChange/Status")]
public class Status_StatsChange : StatsChange
{
    [Header("Alterações de stats relacionadas ao status.")]
    [Space(30)]

    [SerializeField] private VariableOperation<float> statusChanceMultiplier;
    [SerializeField] private VariableOperation<float> statusDamageMultiplier;
    [SerializeField] private VariableOperation<float> statusDurationMultiplier;

    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        globalStats.ChangeStatusChanceMultiplier(statusChanceMultiplier.variable, statusChanceMultiplier.operationType);
        globalStats.ChangeStatusDamageMultiplier(statusDamageMultiplier.variable, statusDamageMultiplier.operationType);
        globalStats.ChangeStatusDurationMultiplier(statusDurationMultiplier.variable, statusDurationMultiplier.operationType);
    }
}
