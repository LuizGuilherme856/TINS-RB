using UnityEngine;

[CreateAssetMenu(fileName = "SpecialAction_StatsChange", menuName = "Upgrades/StatsChange/SpecialAction")]
public class SpecialAction_StatsChange : StatsChange
{
    [SerializeField] private VariableOperation<int> changeMaxSpecialActionQuantity;
    [SerializeField] private VariableOperation<float> changeSpecialAbilityCooldownMultiplier;
    [SerializeField] private VariableOperation<float> resetSpecialAbilityOnEnemyDeathChance;

    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        globalStats.ChangeMaxSpecialActionQuantity(changeMaxSpecialActionQuantity.variable, changeMaxSpecialActionQuantity.operationType);
        globalStats.ChangeSpecialAbilityCooldownMultiplier(changeSpecialAbilityCooldownMultiplier.variable, changeSpecialAbilityCooldownMultiplier.operationType);
        globalStats.ChangeResetSpecialAbilityOnEnemyDeathChance(resetSpecialAbilityOnEnemyDeathChance.variable, resetSpecialAbilityOnEnemyDeathChance.operationType);
    }
}
