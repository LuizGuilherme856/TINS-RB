using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XpBar_SurvivalMode : MonoBehaviour
{
    [SerializeField] XpManager_SurvivalMode xpManager;
    [SerializeField] private Slider xpSlider;

    private void OnEnable()
    {
        xpManager.OnPickUpPoint += UpdateLevelBar;
        xpManager.OnLevelUp += UpdateLevelBar;
    }

    private void OnDisable()
    {

    }

    private void UpdateLevelBar()
   {
        float currentLevelXpPoints = xpManager.CurrentLevelMinXp();
        float nextLevelXpPoints = xpManager.NextLevelMinXp();

        Debug.Log("slider value: " + xpSlider.value);
        //Normalize to the range 0 to 1.
        xpSlider.value = (xpManager.CurrentXpPoints - currentLevelXpPoints)/(nextLevelXpPoints - currentLevelXpPoints);
        // 1200 - 1000 / 2000 - 1000
    }
}
