using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Control lens distortion according to the configured value.
/// in SettingsData for the selected MMFeedbacks component.
/// </summary>
public class ManageLensDistortion : MonoBehaviour
{
    [SerializeField] private SettingsData settingsData;
    [SerializeField] private MMF_Player distortionFeedbacks;
    private MMF_LensDistortion_URP lensDistortion;
    private float baseIntensity = 1f;

    private void OnEnable()
    {
        lensDistortion = distortionFeedbacks.GetFeedbackOfType<MMF_LensDistortion_URP>();
        if (lensDistortion == null) return;

        baseIntensity = lensDistortion.RemapIntensityOne;
        RefreshIntensity();
    }

    public void RefreshIntensity()
    {
        if (lensDistortion == null)
        {
            lensDistortion = distortionFeedbacks.GetFeedbackOfType<MMF_LensDistortion_URP>();
            if (lensDistortion == null) return;
        }

        lensDistortion.RemapIntensityOne = baseIntensity * settingsData.GetLensDistortion();
    }
}
