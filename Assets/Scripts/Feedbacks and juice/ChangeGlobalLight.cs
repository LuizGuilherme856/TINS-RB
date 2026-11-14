using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TechBooh;
using MoreMountains.Feedbacks;

/// <summary>
/// Helper for changing Global Light values.
/// </summary>
public class ChangeGlobalLight : MonoBehaviour
{


    [Header("Blink Config")]
    public AutoBlink autoBlinkMode = AutoBlink.No;
    public int blinkCount = 1;
    public float blinkOffTime = 0.1f;
    public float blinkFadeTime = 0.1f;
    public MMF_Player blinkFeedback; // Light-flashing feedback when using MoreMountains.Feedbacks.
    public enum AutoBlink { No, OnAwake, OnStart, OnEnable,  }

    private GlobalLightManager controlGlobalLight;

    private void Awake()
    {
        controlGlobalLight = Try.FindObjectOfType<GlobalLightManager>();
    }


    private void OnEnable()
    {
        if (autoBlinkMode == AutoBlink.OnEnable)
        {
            StartCoroutine(BlinkMultipleTimes_Routine(blinkCount, blinkOffTime, blinkFadeTime));
        }
    }

    private IEnumerator BlinkMultipleTimes_Routine(int count, float offTime, float fadeTime)
    {
        for (int i = 0; i < count; i++)
        {
            yield return BlinkGlobalLight_Routine(offTime, fadeTime);
        }
    }

    public void ChangeGlobalLightIntensity(float intensity)
    {
        controlGlobalLight.SetLightIntensity(intensity);
    }

    public void SetDefaultLightIntensity()
    {
        controlGlobalLight.ReturnToDefaultIntensity();
    }


    public void ChangeGlobalLightIntensityWithFade(float targetIntensity, float fadeTime)
    {
        StartCoroutine(FadeLightIntensity_Routine(targetIntensity, fadeTime));
    }

    public void SetDefaultLightIntensityWithFade( float fadeTime)
    {
        StartCoroutine(FadeLightIntensity_Routine(controlGlobalLight.defaultIntensity, fadeTime));
    }


    private IEnumerator FadeLightIntensity_Routine(float targetIntensity, float fadeTime)
    {
        float startIntensity = controlGlobalLight.GetGlobalLightIntensity();
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float newIntensity = Mathf.Lerp(startIntensity, targetIntensity, elapsed / fadeTime);
            controlGlobalLight.SetLightIntensity(newIntensity);
            yield return null;
        }

        controlGlobalLight.SetLightIntensity(targetIntensity);
    }

    public void BlinkGlobalLight()
    {
        StartCoroutine(BlinkMultipleTimes_Routine(blinkCount, blinkOffTime, blinkFadeTime));
    }

    public void BlinkGlobalLightOnce(float offTime, float fadeTime)
    {
        StartCoroutine(BlinkGlobalLight_Routine(offTime, fadeTime));
    }

    private IEnumerator BlinkGlobalLight_Routine(float offTime, float fadeTime)
    {
        float defaultIntensity = controlGlobalLight.defaultIntensity;

        if (blinkFeedback != null)
        {
            blinkFeedback.PlayFeedbacks();
        }
        // Fade to zero.
        yield return FadeLightIntensity_Routine(0f, fadeTime);

        // Remain off for the configured duration.
        yield return new WaitForSeconds(offTime);

        // Fade back to the default value.
        yield return FadeLightIntensity_Routine(defaultIntensity, fadeTime);
    }


}
