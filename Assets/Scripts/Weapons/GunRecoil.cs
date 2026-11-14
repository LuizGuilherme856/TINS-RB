using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class GunRecoil : MonoBehaviour
{
    [SerializeField] private  GunInventory gunInventory;
    [SerializeField] private Impulse playerImpulse;

    [SerializeField] private MMF_Player mmfPlayer;
    MMF_Rotation rotationFeedback;

    [SerializeField] private bool playerClone = false;


     private GunData currentGunData;

    private void Awake()
    {
        rotationFeedback = mmfPlayer.GetFeedbackOfType<MMF_Rotation>();
    }

    private void Start()
    {
        if (playerClone) return;

        gunInventory.OnWeaponChange += UpdateGunData;
        UpdateGunData();
    }

    /*
    private void Update()
    {
        if(mmfPlayer.enabled == false)
        {
            Debug.Log("Inactive; enabling now");
            mmfPlayer.enabled = true;
        }
    }

    */

    public void TriggerRecoil(bool isFacingRight, float aimAngle, float recoilForceOnPlayer, bool useRotationFeedback = true)
    {
        if (currentGunData == null)
        {
            Debug.Log("ARMA nula");
            return;
        }
        if (isFacingRight)
        {
            rotationFeedback.RemapCurveOne = currentGunData.recoilAngle;
        }
        else
        {
            rotationFeedback.RemapCurveOne = -currentGunData.recoilAngle;
        }
        

        playerImpulse.TriggerImpulse(aimAngle, recoilForceOnPlayer, true);

        if(useRotationFeedback) mmfPlayer.PlayFeedbacks();
    }
    private void UpdateGunData()
    {
        if(playerClone == false) currentGunData = gunInventory.GetCurrentEquipedGun();
        if (currentGunData == null)
        {
            Debug.Log("nao conseguiu atualizar a arma. Arma nula");
            return;
        }
        try //Catch failures when the player clone lacks the expected components.
        {
            rotationFeedback.AnimateRotationDuration = currentGunData.recoilDuration;

        }
        catch (System.Exception e)
        {

            Debug.Log("Erro aqui, mas ignora se quiser...: " + e);
        }
    }

    public void SetCurrentGunData(GunData gunData) //Assign a weapon explicitly because the clone has no GunInventory.
    {
        currentGunData = gunData;
        UpdateGunData();
    }

}
