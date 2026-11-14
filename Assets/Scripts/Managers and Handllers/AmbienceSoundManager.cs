using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceSoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource myAudioSource;
    [SerializeField] private AudioClip cemeteryGatesDefault;


    [Header("    Rain SFX")]
    [SerializeField] private AudioClip lightRain, heavyRain, thunderStorm, rainingBlood;

    public void SetVolume (float volume)
    {
        myAudioSource.volume = volume;
    }
  
    public void SetRainAmbience(RainStages rainStage)
    {
        switch (rainStage)
        {
            case RainStages.lightRain:
                myAudioSource.clip = lightRain;
                break;
            case RainStages.heavyRain:
                myAudioSource.clip = heavyRain;
                break;
            case RainStages.thunderStorm:
                myAudioSource.clip = thunderStorm;
                break;
            case RainStages.rainingBlood:
                myAudioSource.clip = rainingBlood;
                break;
            default:
                myAudioSource.clip = cemeteryGatesDefault;
                break;
        }

        myAudioSource.Play();
    }
}
