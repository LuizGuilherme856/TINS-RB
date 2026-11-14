using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderFeedbacks : MonoBehaviour
{
    [SerializeField] private float minThunderTime = 10f, maxThunderTime = 40f;
    [SerializeField] private float minThunderBloodTime = 4f, maxThunderBloodTime = 20f;

    [SerializeField] private RainManager rainManager;
    [SerializeField] private GlobalLightManager controlGlobalLight;
    [SerializeField] private MMF_Player feedbacks;

    private float currentTimer=1f;
    private Animator myAnimator;
    private RainStages currentRainStage = RainStages.noRain;

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();

        //InvokeRepeating(nameof(PlayThunderFeedbacks), 1, 5);
    }

    private void Start()
    {
        rainManager.OnChangeRainStage += RefreshRainStage;
    }

    private void FixedUpdate()
    {

        
        if(currentRainStage == RainStages.thunderStorm)
        {
            currentTimer -= Time.fixedDeltaTime;
            if(currentTimer <= 0)
            {
                PlayThunderFeedbacks();
                currentTimer = Random.Range(minThunderTime, maxThunderTime);
            }
        }
        else if (currentRainStage == RainStages.rainingBlood)
        {
            if (currentTimer <= 0)
            {
                PlayThunderFeedbacks();
                currentTimer = Random.Range(minThunderBloodTime, maxThunderBloodTime);
            }

        }
    }

    private void RefreshRainStage()
    {

        if (currentRainStage != rainManager.GetCurrentRainStage()) PlayThunderFeedbacks();
        currentRainStage = rainManager.GetCurrentRainStage();


    }

    public void PlayThunderFeedbacks()
    {
        myAnimator.SetTrigger("Thunder");
        feedbacks.PlayFeedbacks();
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
