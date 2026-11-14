using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Thorn_StatsChange", menuName = "Upgrades/StatsChange/Thorn")]
public class Thorn_StatsChange : StatsChange
{
    [Header("Alterações de stats relacionadas ao dano de espinhos.")]
    [Space(30)]

    [SerializeField] private VariableOperation<int> ThornDamage;
    [SerializeField] private VariableOperation<DamageType> ThornDamageType;
    [SerializeField] private VariableOperation<float> ThornInstaKillChance;

    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        globalStats.ChangeThornDamageBy(ThornDamage.variable, ThornDamage.operationType);
        globalStats.ChangeThornDamageType(ThornDamageType.variable, ThornDamageType.operationType);
        globalStats.ChangeThornInstaKillChance(ThornInstaKillChance.variable, ThornInstaKillChance.operationType);
    }
}
