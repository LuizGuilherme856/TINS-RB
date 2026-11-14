using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CthulhuShield : MonoBehaviour
{
    [SerializeField] private GameObject smallShield, bigShield;
    [SerializeField] private BossNextForm bossNextForm;
    [SerializeField] private MMF_Player activateSmallShieldFeedback, deactivateSmallShieldFeedback;
    [SerializeField] private MMF_Player activateBigShieldFeedback, deactivateBigShieldFeedback;
    private int currentForm = 0;

    private void OnEnable()
    {
        bossNextForm.OnChangeForm += () => currentForm = bossNextForm.currentForm;
    }

    public void ActivateShield()
    {
        smallShield.SetActive(true);
        activateSmallShieldFeedback.PlayFeedbacks();
        if (currentForm >= 2)
        {
            bigShield.SetActive(true);
            activateBigShieldFeedback.PlayFeedbacks();
        }
            
    }

    public void DeactivateShield()
    {
        deactivateSmallShieldFeedback.PlayFeedbacks();
        if (currentForm >= 2)
        {
            bigShield.SetActive(false);
            deactivateBigShieldFeedback.PlayFeedbacks();
        }
            
            
        //Should this be disabled through feedbacks?
    }

}
