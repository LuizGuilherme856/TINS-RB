using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemies_StatsChange", menuName = "Upgrades/StatsChange/Enemies")]
public class Enemies_StatsChange : StatsChange
{
    [Header("Alterações de stats relacionadas à saúde dos inimigos.")]
    [Space(30)]

    [SerializeField] private VariableOperation<float> enemyHealthMultiplier;

    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        globalStats.ChangeEnemyHealthMultiplier(enemyHealthMultiplier.variable, enemyHealthMultiplier.operationType);
    }
}
