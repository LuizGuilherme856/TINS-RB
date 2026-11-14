using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alkarion_AnimatorFeedbacks : MonoBehaviour
{
    [SerializeField] private MMF_Player wing1, wing2, headPopOut;
    [SerializeField] private MMFeedbacks cameraShake;

    private GlobalLightManager controlGlobalLight;

    private void Awake()
    {
        controlGlobalLight = FindFirstObjectByType<GlobalLightManager>();
    }
    public void PlayWingFeedbacks(int wingNumber) //Called by the Animator.
    {
        if(wingNumber == 1) wing1.PlayFeedbacks();
        else wing2.PlayFeedbacks();
    }

    public void PlayHeadPopOutFeedback()
    {
        headPopOut.PlayFeedbacks();
    }

    public void CameraShakeFeedback()
    {
        cameraShake.PlayFeedbacks();
    }

    //Called by the Animator.
    public void ChangeGlobalLightIntensity(float intensity)
    {
        controlGlobalLight.SetLightIntensity(intensity);
    }

    public void SetDefaultLightIntensity()
    {
        controlGlobalLight.ReturnToDefaultIntensity();
    }
}
