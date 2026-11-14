using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalDamage_StatsChange", menuName = "Upgrades/StatsChange/GlobalDamage")]
public class GlobalDamage_StatsChange : StatsChange
{
    [Header("Alterações de stats relacionadas ao dano global. Dano global influencia o dano de tudo.")]
    [Space(30)]

    [SerializeField] private VariableOperation<float> globalDamage_Multiplier;
    [SerializeField] private VariableOperation<float> increaseGlobalDamageBy;

    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        globalStats.ChangeGlobalDamageMultiplier(globalDamage_Multiplier.variable, globalDamage_Multiplier.operationType);
        globalStats.ChangeIncreaseGlobalDamageBy(increaseGlobalDamageBy.variable, increaseGlobalDamageBy.operationType);
  
    }

}
