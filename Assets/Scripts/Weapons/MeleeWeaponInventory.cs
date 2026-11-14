using System;
using UnityEngine;

public class MeleeWeaponInventory : MonoBehaviour
{
    [SerializeField] MeleeManager meleeManager;
    //Event used by HandlePlayerHeadHit to update health.
    public event Action OnChangeMeleeWeapon;
    private RunData runData;

    private void Awake()
    {
        runData = GetComponent<RunDataManager>().GetRunData();
    }

    private void Start()
    {

        if (meleeManager.GetPlayerAction() == PlayerAction.melee)
        {
            meleeManager.SetMeleeWeapon(runData.meleeWeapon);
        }
        else if (meleeManager.GetPlayerAction() == PlayerAction.dash)
        {
            if (runData.dashData == null) Debug.Log("Dash data ta null no run Data. Está certo isso?"); 
            meleeManager.SetMeleeWeapon(runData.dashData);
        }
    }

    public void ChangeMeleeWeapon(MeleeData meleeData)
    {
        runData.meleeWeapon= meleeData;
        meleeManager.SetMeleeWeapon(meleeData);
        OnChangeMeleeWeapon?.Invoke();
    }

    public MeleeData GetMeleeWeapon()
    {
        return runData != null ? runData.meleeWeapon : null;
    }
}
