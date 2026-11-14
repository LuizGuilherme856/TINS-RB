using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActiveEffect : ScriptableObject
{
    public abstract void TriggerActiveEffect(UpgradeManager upgradeManager, GlobalStats globalStats);

}
