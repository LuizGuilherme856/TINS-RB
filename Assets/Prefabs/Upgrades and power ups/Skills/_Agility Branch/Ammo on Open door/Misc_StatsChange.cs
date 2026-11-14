using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Misc_StatsChange", menuName = "Upgrades/StatsChange/Misc")]
public class Misc_StatsChange : StatsChange
{
    [Header("Alterações de stats relacionadas a várias funcionalidades.")]
    [Space(30)]

    [SerializeField] private VariableOperation<bool> refilAmmoOnOpenDoor;
    [SerializeField] private VariableOperation<int> freeRevives;
    //[SerializeField] private VariableOperation<DamageType> walkTrailDamageType;

    public override void ExecuteStatsChange(GlobalStats globalStats)
    {
        globalStats.ChangeRefilAmmoOnOpenDoor(refilAmmoOnOpenDoor.variable, refilAmmoOnOpenDoor.operationType);
        globalStats.ChangeFreeRevives(freeRevives.variable, freeRevives.operationType);
        //globalStats.ChangeWalkTrailDamageType(walkTrailDamageType.variable, walkTrailDamageType.operationType);
    }
}