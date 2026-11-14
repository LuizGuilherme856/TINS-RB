using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Reduce explosion volume when firing multiple bullets to avoid excessive loudness.

public class ManageExplosionSfxVolume : MonoBehaviour
{
    [SerializeField] private MMF_Player feedbacks;
    [SerializeField] private RuneInventory runeUpgradeManager;
    [SerializeField] private ExplosiveData explosionData;

    private MMF_Sound soundFeedback;
    private float originalVolume;

    private void Awake()
    {
        soundFeedback = feedbacks.GetComponent<MMF_Sound>();

        originalVolume = explosionData.explosionVolume;

        UpdateVolume();

    }


    void Start()
    {
        runeUpgradeManager.OnBuyRune += UpdateVolume;
    }

    private void UpdateVolume()
    {
        Debug.Log("Consertar aqui. Lógica foi perdida com a troca do sistema de upgrade");
        int nOfUpgrades = 0; // runeUpgradeManager.GetNumberOfExtraShotUpgrades();

        if (nOfUpgrades == 0)
        {
            soundFeedback.MinVolume = originalVolume;
            soundFeedback.MaxVolume = originalVolume;
        }
        else if (nOfUpgrades == 1)
        {
            soundFeedback.MinVolume = originalVolume * 0.7f;
            soundFeedback.MaxVolume = originalVolume * 0.7f;
        }
        else if (nOfUpgrades == 2)
        {
            soundFeedback.MinVolume = originalVolume * 0.5f;
            soundFeedback.MaxVolume = originalVolume * 0.5f;
        }
        else
        {
            soundFeedback.MinVolume = originalVolume * 0.4f;
            soundFeedback.MaxVolume = originalVolume * 0.4f;
        }
    }


}
