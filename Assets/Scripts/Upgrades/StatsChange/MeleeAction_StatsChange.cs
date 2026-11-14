using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAction_StatsChange", menuName = "Upgrades/StatsChange/MeleeAction")]
public class MeleeAction_StatsChange : StatsChange
{
    [SerializeField] private VariableOperation<int> changeMaxMeleeActionQuantity;
    [SerializeField] private VariableOperation<float> changeMeleeAttackRateMultiplier;

    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        globalStats.ChangeMaxMeleeActionQuantity(changeMaxMeleeActionQuantity.variable, changeMaxMeleeActionQuantity.operationType);
        globalStats.ChangeMeleeAttackRateMultiplier(changeMeleeAttackRateMultiplier.variable, changeMeleeAttackRateMultiplier.operationType);
    }
}
