using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Delay Requirement", menuName = "Upgrades/Requirements/Delay")]

/// <summary>
/// Require the configured delay before execution.
/// </summary>
public class DelayRequirement : Requirement
{
    public float delay = 5f;

    public override bool RequirementCompleted { get; set ;}

    public override event Action OnRequirementCompleted;


    public override void CheckRequirements(UpgradeManager upgradeManager)
    {
        RequirementCompleted = false;
        //Use the supplied MonoBehaviour to start coroutines; ScriptableObjects cannot run them directly.
        //Use a MonoBehaviour as the coroutine host.
        upgradeManager.StartCoroutine(Delay());
    }

    private IEnumerator Delay()
    {
        Debug.Log("Esperando o delay");
        yield return new WaitForSeconds(delay);
        OnRequirementCompleted?.Invoke();
        RequirementCompleted = true;
        Debug.Log("Completou o delay");
    }


}
