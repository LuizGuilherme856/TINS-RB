using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OrbitalUpgrade_ActiveEffect", menuName = "Upgrades/ActiveEffects/Orbital Upgrade")]
public class OrbitalUpgrade_ActiveEffect : ActiveEffect
{
    [SerializeField] private int maxObjectsOfThisType = 5;
    [Tooltip("randomiza a velocidade em um valor dentro desse intervalo")]
    [SerializeField] private float _orbitRotationSpeed = 100;
    [Space(10)]
    [SerializeField] private float _orbitRadius = 10;
    [Space(10)]
    [SerializeField] private float _orbitCorrectionSpeed = 20;

    //[SerializeField] private RuneInventory runeInventory;
    [SerializeField] public MeleeData meleeDataToOrbit;


    public override void TriggerActiveEffect(UpgradeManager upgradeManager, GlobalStats globalStats)
    {
    try
    {
        if (upgradeManager == null)
        {
            Debug.LogError($"[{name}] upgradeManager está nulo.");
            return;
        }
        if (upgradeManager.GetOrbitalUpgradeManager() == null)
        {
            Debug.LogError($"[{name}] upgradeManager.orbitUpgrade está nulo.");
            return;
        }

        upgradeManager.GetOrbitalUpgradeManager().UpdateObjectSpeedAndRadius();
    }
    catch (System.Exception e)
    {
        Debug.LogError($"[{name}] Erro ao atualizar a velocidade do obj orbital: {e.Message}\nStackTrace: {e.StackTrace}");
    }
        var objectToOrbit = meleeDataToOrbit.meleeGameObject;

        //Ensure MMAutoRotate exists.
        Aux.EnsureComponent<MMAutoRotate>(objectToOrbit);
        Aux.EnsureComponent<OrbitalUpgradeHolder>(objectToOrbit);

        //Establish the cross-reference: MeleeData references the object, and the object needs its MeleeData.
        objectToOrbit.GetComponent<MeleeStats>().weaponData = meleeDataToOrbit;

        if(upgradeManager.orbitUpgrade.CountOrbitObjects(objectToOrbit) == maxObjectsOfThisType)
        {
            //The maximum number of this object type has been reached.
            return;
        }

        upgradeManager.orbitUpgrade.AddObjectToOrbit(objectToOrbit, this,  false, OrbitRotationSpeed, OrbitRadius, _orbitCorrectionSpeed);
        
    }


    //Getters and setters.
    public float OrbitRotationSpeed
    {
        //Randomize speeds by 1% so two objects do not share exactly the same orbit;
        //otherwise, overlapping objects would look like a single one.
        get => Random.Range(_orbitRotationSpeed - _orbitRotationSpeed *0.01f, _orbitRotationSpeed + _orbitRotationSpeed * 0.01f);
        set => _orbitRotationSpeed = value;
    }
    public float OrbitRadius
    {
        get => Random.Range(_orbitRadius - _orbitRadius / 20, _orbitRadius + _orbitRadius / 20);
        set => _orbitRadius = value;
    }
    public float OrbitCorrectionSpeed { get => _orbitCorrectionSpeed; set => _orbitCorrectionSpeed = value; }


}
