using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Only one scene object should control global light.
/// </summary>

public class GlobalLightManager : MonoBehaviour
{

    private Light2D myLight2D;
    public float defaultIntensity { get; private set; }
    private void Awake()
    {
        myLight2D= GetComponent<Light2D>();
        defaultIntensity= myLight2D.intensity;
    }

    public void SetLightIntensity(float intensity)
    {
        myLight2D.intensity= intensity;
    }

    public void SetDefaultIntensity(float newIntensity)
    {
        defaultIntensity = newIntensity;
    }

    public void ReturnToDefaultIntensity()
    {
        myLight2D.intensity= defaultIntensity;
    }

    public void SetGlobalLightColor(Color newColor)
    {
        myLight2D.color = newColor;
    }

    public Color GetGlobalLightColor()
    {
        return myLight2D.color;
    }
    public float GetGlobalLightIntensity()
    {
        return myLight2D.intensity;
    }
}
