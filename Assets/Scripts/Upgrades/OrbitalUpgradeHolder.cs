using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Remember which OrbitalUpgrade_ActiveEffect spawned this object.
/// </summary>
public class OrbitalUpgradeHolder : MonoBehaviour
{
    public OrbitalUpgrade_ActiveEffect orbitalUpgrade_ActiveEffect;

    private void OnEnable()
    {
        //Wait two seconds before checking orbital ownership; checking during OnEnable can precede initialization.
        Invoke(nameof(Check), 2);
    }

    private void Check()
    {
        if (orbitalUpgrade_ActiveEffect == null)
        {
            Debug.LogError($"Não foi encontrado OrbitalUpgrade_ActiveEffect nesse objeto: {gameObject.name}.");
        }
    }
}
