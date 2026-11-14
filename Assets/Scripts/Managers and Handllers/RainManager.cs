using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Controls rain appearance.
/// </summary>
public class RainManager : MonoBehaviour
{
    //[SerializeField] private Color defaultRainColor, bloodRainColor;
    //[SerializeField] private float[] particlesOverTime; //Four entries, one per rain type.
    //[SerializeField] private float[] globalLightIntensity = new float[4];
    //public Color[] globalLightColors;
    //[SerializeField] private float minEmission = 50f, maxEmission = 800f;

    [SerializeField] private ParticleSystem rainParticles;
    [SerializeField] private EnemyWaveManager hordeManager;
    [SerializeField] private GlobalLightManager controlGlobalLight_;
    [SerializeField] private AmbienceSoundManager ambienceSoundManager_;
    [SerializeField] private bool spawnParticles = true; //Disabled in the roofed Cathedral, where rain particles are unnecessary.

    [SerializeField] private bool enableRain = true;
    
    //private float currentEmission;
    private int startRainingWave, maxRainWave;
    ParticleSystem.EmissionModule ps_emission;
    ParticleSystem.MainModule ps_main;
    private RainStages currentRainStage = RainStages.noRain;

    public event Action OnChangeRainStage;

    private void Awake()
    {
        if (enableRain == false) return;

        ps_emission = rainParticles.emission;
        ps_main = rainParticles.main;

        if (ambienceSoundManager_ == null) //Defensive check for an observed failure.
        {
            Debug.Log("nao tem o ambience sfx. procurando...");
            ambienceSoundManager_ = FindFirstObjectByType<AmbienceSoundManager>();
        }

        if(controlGlobalLight_ == null)
        {
            Debug.Log("nao tem o control global light. procurando...");
            controlGlobalLight_ = FindFirstObjectByType<GlobalLightManager>();
        }
    }
    void Start()
    {
        if (enableRain == false) return;
        startRainingWave = hordeManager.startRainingWave;
        maxRainWave = hordeManager.maxRainWave;

        if (spawnParticles) ps_emission = rainParticles.emission;

        hordeManager.OnStartNewWave += UpdateRain;


    }

    private void UpdateRain()
    {
        if (enableRain == false) return;
        int currentWave = hordeManager.GetCurrentWave();

        if(spawnParticles == false)
        {
            DontSpawnParticles();
        }

        if(currentWave < startRainingWave && currentWave % 5 == 0)
        {
            SetRainStages(RainStages.lightRain);
        }
        else if(currentWave < startRainingWave)
        {
            ps_emission.rateOverTime = 0;
            ps_main.startColor = new Color(1, 1, 1, 0.6f);
            SetRainStages(RainStages.noRain);
            return;
        }



        if(currentWave <= startRainingWave + 5) //20 - 25
        {
            //Debug.Log("Set light rain");
            SetRainStages(RainStages.lightRain);
            
        }
        else if(currentWave < startRainingWave + 10) //25 -30
        {
            //Debug.Log("Set heavyRain");
            SetRainStages(RainStages.heavyRain);
        }
        else if (currentWave <= startRainingWave + 29) //30 - 49
        {
            //Debug.Log("Set thunder ");
            SetRainStages(RainStages.thunderStorm);
        }
        else //>=50
        {
            //Debug.Log("Set raining blood");
            SetRainStages(RainStages.rainingBlood);
        }





        //ps_emission.rateOverTime = CalculateEmission();
       // ps_main.startColor = CalculateColor();
    }

    private void DontSpawnParticles()
    {
        rainParticles.gameObject.SetActive(false);
        ps_emission.rateOverTime = 0;
    }

    private void SetRainStages(RainStages rainStage)
    {
        if (enableRain == false) return;
        if (currentRainStage == rainStage) return; //No update is needed.
        currentRainStage = rainStage;
        OnChangeRainStage?.Invoke();

        ambienceSoundManager_.SetRainAmbience(rainStage);

        switch (rainStage)
        {
            case RainStages.lightRain:
                ps_emission.rateOverTime = 50;
                // controlGlobalLight.SetDefaultIntensity(globalLightIntensity[0]);
                controlGlobalLight_.SetGlobalLightColor(new Color (1,1,1,1));

                break;
            case RainStages.heavyRain:
                ps_emission.rateOverTime = 150;
                controlGlobalLight_.SetGlobalLightColor(new Color(0.8f, 0.8f, 0.8f, 0.8f));
                // controlGlobalLight.SetDefaultIntensity(globalLightIntensity[1]);
                break;
            case RainStages.thunderStorm:
                ps_emission.rateOverTime = 300;
                controlGlobalLight_.SetGlobalLightColor(new Color(0.64f, 0.52f, 0.58f, 1));
                // controlGlobalLight.SetDefaultIntensity(globalLightIntensity[2]);
                break;
            case RainStages.rainingBlood:
                ps_emission.rateOverTime = 600;
                controlGlobalLight_.SetGlobalLightColor(new Color(0.66f, 0.18f, 0.48f, 1));
                // controlGlobalLight.SetDefaultIntensity(globalLightIntensity[3]);
                ps_main.startColor = new Color(1, 0, 0.274f, 0.8f);
                break;
            default:
                break;
        }
    }

    /*
    private Color CalculateColor()
    {
        if (hordeManager.GetCurrentWave() > maxRainWave) return colors[colors.Length-1]; //Return the final color.

        //Increase the fractional index with each wave, then convert it to an integer.
       // float indexPerWave_fraction  = colors.Length / (maxRainWave - startRainingWave);
        //int index = Mathf.FloorToInt( (hordeManager.GetCurrentWave() - startRainingWave) * indexPerWave_fraction);
        return colors[0];

    }
    private int CalculateEmission()
    {
        if (hordeManager.GetCurrentWave() > maxRainWave) return (int) maxEmission;

        float particlesPerWave = (maxEmission - minEmission) / (maxRainWave - startRainingWave);
        float currentParticles = minEmission + (hordeManager.GetCurrentWave() - startRainingWave) * particlesPerWave;

        return (int)currentParticles;
    }
    */

    public RainStages GetCurrentRainStage()
    {
        if (enableRain == false)  return RainStages.noRain;
        return currentRainStage;
    }

}

    public enum RainStages {noRain, lightRain, heavyRain, thunderStorm, rainingBlood}