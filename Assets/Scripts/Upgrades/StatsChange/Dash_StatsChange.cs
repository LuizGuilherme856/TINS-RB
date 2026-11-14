using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Dash_StatsChange", menuName = "Upgrades/StatsChange/Dash")]
public class Dash_StatsChange : StatsChange
{
    [SerializeField] private VariableOperation<float> changeDashCoolDownMultiplier;
    [SerializeField] private VariableOperation<int> changeDashMaxQuantity;
    [SerializeField] private VariableOperation<float> dashDamageMultiplier;
    public DamageType dashDamageType = DamageType.None;

    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        Debug.Log(this.name);
        globalStats.ChangeDashCooldownMultiplier(changeDashCoolDownMultiplier.variable, changeDashCoolDownMultiplier.operationType);
        globalStats.ChangeMaxDashActionQuantity(changeDashMaxQuantity.variable, changeDashMaxQuantity.operationType);
        globalStats.ChangeDashDamageMultiplier(dashDamageMultiplier.variable, dashDamageMultiplier.operationType);
        if(dashDamageType != DamageType.None) globalStats.ChangeDashDamageType(dashDamageType);
    }
}
