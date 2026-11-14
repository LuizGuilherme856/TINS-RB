using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialWeaponInventory : MonoBehaviour
{
    //Special weapons reuse MeleeManager.

    [SerializeField] MeleeManager meleeManager;

    private RunData runData;

    private void Awake()
    {
        runData = GetComponent<RunDataManager>().GetRunData();
    }
    private void Start()
    {
        
        meleeManager.SetMeleeWeapon(runData.specialAbility);
    }


    public void ChangeSpecialWeapon(MeleeData meleeData)
    {
        if (meleeData == null) return;

        runData.specialAbility = meleeData;
        meleeManager.SetMeleeWeapon(meleeData);
    }
}
