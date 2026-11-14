using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageChromaticAberration : MonoBehaviour
{
    [SerializeField] private SettingsData settingsData;
    [SerializeField] private MMF_Player aberrationFeedbacks;
    private MMF_ChromaticAberration_URP chromaAberration;
    private float baseIntensity = 1f;

    private void OnEnable()
    {
        chromaAberration = aberrationFeedbacks.GetFeedbackOfType<MMF_ChromaticAberration_URP>();
        if (chromaAberration == null) return;

        baseIntensity = chromaAberration.RemapIntensityOne;
        RefreshIntensity();
    }

    public void RefreshIntensity()
    {
        if (chromaAberration == null)
        {
            chromaAberration = aberrationFeedbacks.GetFeedbackOfType<MMF_ChromaticAberration_URP>();
            if (chromaAberration == null) return;
            baseIntensity = chromaAberration.RemapIntensityOne;
        }

        chromaAberration.RemapIntensityOne = baseIntensity * settingsData.GetLensDistortion();
    }
}
