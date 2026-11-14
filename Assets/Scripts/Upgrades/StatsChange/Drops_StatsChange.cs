using UnityEngine;

[CreateAssetMenu(fileName = "Drops_StatsChange", menuName = "Upgrades/StatsChange/Drops")]
public class Drops_StatsChange : StatsChange
{
    [SerializeField] private VariableOperation<float> AddToDropChance;
    [SerializeField] private VariableOperation<int> AddToHpHealedByHearts;
    [SerializeField] private VariableOperation<float> powerUpGoTowardsPlayerMaxDistanceMultiplier;

    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        globalStats.ChangeAddToDropChance(AddToDropChance.variable, AddToDropChance.operationType);
        globalStats.ChangeAddToHpHealedByHearts(AddToHpHealedByHearts.variable, AddToHpHealedByHearts.operationType);
        globalStats.ChangePowerUpGoTowardsPlayerMaxDistanceMultiplier(powerUpGoTowardsPlayerMaxDistanceMultiplier.variable, powerUpGoTowardsPlayerMaxDistanceMultiplier.operationType);
    }
}
