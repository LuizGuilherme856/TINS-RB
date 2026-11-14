using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Initialize audio sliders from the current audio settings.
public class SetSlidersValue : MonoBehaviour
{
    [Header("Sliders e Flags")]
    [SerializeField] private bool updateMusicSlider = true;
    [SerializeField] private Slider musicSlider;

    [SerializeField] private bool updateSfxSlider = true;
    [SerializeField] private Slider sfxSlider;

    [SerializeField] private bool updateUiSlider = true;
    [SerializeField] private Slider uiSlider;

    [SerializeField] private bool updateCamShakeSlider = true;
    [SerializeField] private Slider camShakeSlider;

    [SerializeField] private bool updateLensDistortionSlider = true;
    [SerializeField] private Slider lensDistortionSlider;

    [SerializeField] private bool updateCameraFollowMouse = true;
    [SerializeField] private Slider cameraFollowMouse;

    [SerializeField] private bool updateBloodParticles = true;
    [SerializeField] private Slider bloodParticles;

    [SerializeField] private bool updateParticles = true;
    [SerializeField] private Slider particles;

    [SerializeField] private bool updateFrameCap = true;
    [SerializeField] private Slider frameCap;

    private GameDataAndSettings gameData;
    [SerializeField] private SettingsData settingsData;

    private void Awake()
    {
        gameData = GameObject.Find("Game Session").GetComponent<GameDataAndSettings>();
    }

    private void Start()
    {
        gameData = FindFirstObjectByType<GameDataAndSettings>();

        if (updateMusicSlider)
            SetSliderValueWithoutNotify_TechBooh(musicSlider, gameData.musicVolume);

        if (updateSfxSlider)
            SetSliderValueWithoutNotify_TechBooh(sfxSlider, gameData.sfxVolume);

        if (updateUiSlider)
            SetSliderValueWithoutNotify_TechBooh(uiSlider, gameData.uiVolume);

        if (updateCamShakeSlider)
            SetSliderValueWithoutNotify_TechBooh(camShakeSlider, gameData.cameraShakeIntensity);

        if (updateLensDistortionSlider)
            SetSliderValueWithoutNotify_TechBooh(lensDistortionSlider, settingsData.GetLensDistortion());

        if (updateCameraFollowMouse)
            SetSliderValueWithoutNotify_TechBooh(cameraFollowMouse, gameData.cameraFollowMouseIntensity);

        if (updateBloodParticles)
            SetSliderValueWithoutNotify_TechBooh(bloodParticles, settingsData.GetBloodParticles());

        if (updateParticles)
            SetSliderValueWithoutNotify_TechBooh(particles, settingsData.GetParticles());

        if (updateFrameCap)
            SetSliderValueWithoutNotify_TechBooh(frameCap, gameData.frameCap);
    }

    private void SetSliderValueWithoutNotify_TechBooh(Slider slider, float value)
    {
        if (slider == null)
        {
            return;
        }

        // TechBooh:
        // On opening the menu, sliders must reflect saved values without raising
        // OnValueChanged events, avoiding feedbacks and side effects
        // that should occur only after manual input.
        slider.SetValueWithoutNotify(value);
    }
}
