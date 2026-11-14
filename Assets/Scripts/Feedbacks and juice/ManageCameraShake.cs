using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Change shake strength at its source.
/// </summary>
[RequireComponent(typeof(CinemachineImpulseListener))]

public class ManageCameraShake : MonoBehaviour
{
    private CinemachineImpulseListener impulseListener;
    [SerializeField] private SettingsData settingsData;
    [SerializeField] private PlayerHolder playerHolder;

    public Slider cameraShakeSlider;

    private void Awake()
    {
        impulseListener = GetComponent<CinemachineImpulseListener>();
        UpdateCameraShake();
    }


    public void UpdateCameraShake()
    {
        impulseListener.Gain = settingsData.GetCameraShakeIntensity();
    }


    public void Start()
    {
        //Adds a listener to the main slider and invokes a method when the value changes.
        cameraShakeSlider.onValueChanged.AddListener(delegate { UpdateCameraShake(); });
    }


}
