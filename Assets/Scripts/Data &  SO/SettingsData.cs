using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Allows particle settings to change during gameplay.
//Settings currently combine a singleton and ScriptableObjects; this implementation preserves that arrangement.

//[CreateAssetMenu(fileName = "settings", menuName = "settings")]
public class SettingsData : ScriptableObject
{
    [SerializeField] [Range(0,1)] private float bloodParticles = 1, particles = 1, cameraShake = 1, lensDistortion;



    public float GetBloodParticles()
    {
        return bloodParticles;
    }

    public float GetParticles() 
    {
        return particles;
    }

    public float GetCameraShakeIntensity()
    {
        return cameraShake;
    }

    public float GetLensDistortion()
    {
        return lensDistortion;
    }


    public void SetBloodParticles(float amount)
    {
        if(amount > 1) amount = 1;
        else if(amount< 0) amount = 0;
        bloodParticles= amount;
    }

    public void SetParticles(float amount)
    {
        if (amount > 1) amount = 1;
        else if (amount < 0) amount = 0;
        particles = amount;
    }

    public void SetCameraShake(float amount)
    {
        if (amount > 1) amount = 1;
        else if (amount < 0) amount = 0;
        cameraShake = amount;
    }

    public void SetlensDistorion(float amount)
    {
        if (amount > 1) amount = 1;
        else if (amount < 0) amount = 0;
        lensDistortion = amount;
    }

}
