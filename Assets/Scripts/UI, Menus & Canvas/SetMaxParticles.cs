using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetMaxParticles : MonoBehaviour
{
    [SerializeField] private ParticleType particleType;
    [SerializeField] private SettingsData settingsData;

    [SerializeField] private bool observeSlider = false;
    [SerializeField] private Slider particleSlider;
    [SerializeField] private RunData runData;
    private float defaultMaxParticles;
   
    private ParticleSystem myParticleSystem;

    private void Awake()
    {
        myParticleSystem = GetComponent<ParticleSystem>();
        defaultMaxParticles = myParticleSystem.main.maxParticles;
        RefreshMaxParticles();

    }

    private void OnEnable()
    {
        RefreshMaxParticles();
    }

    public void RefreshMaxParticles()
    {
        if (myParticleSystem == null)
        {
            myParticleSystem = GetComponent<ParticleSystem>();
            if (myParticleSystem == null) return;
        }

        var psMain = myParticleSystem.main;

        switch (particleType)
        {
            case ParticleType.bloodParticle:
                int newMaxParticles = 10;
                if (runData != null)
                {
                     newMaxParticles = Mathf.RoundToInt(defaultMaxParticles * settingsData.GetBloodParticles() - runData.currentWaveNumber * 3);
                }
                else
                {
                    Debug.Log($"Nao tem o RunData no gameObject: " + name);
                }


                if (newMaxParticles < 5) newMaxParticles = 5;
                psMain.maxParticles = newMaxParticles;
                break;
            case ParticleType.regularParticle:
                psMain.maxParticles = Mathf.RoundToInt(defaultMaxParticles * settingsData.GetParticles());
                break;
            default:
                break;
        }
    }

    void Start()
    {
        if(observeSlider)
        {
            //Adds a listener to the main slider and invokes a method when the value changes.
            particleSlider.onValueChanged.AddListener(delegate { RefreshMaxParticles(); });
        }
    }


    private enum ParticleType { bloodParticle, regularParticle }
}
