using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Requirement : ScriptableObject
{
    public abstract event Action OnRequirementCompleted;
    public abstract bool RequirementCompleted { get; set;}
    public abstract void CheckRequirements(UpgradeManager upgradeManager);

}
