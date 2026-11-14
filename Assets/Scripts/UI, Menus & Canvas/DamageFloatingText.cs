using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;

public class DamageFloatingText : MonoBehaviour
{
    [SerializeField] private float maxSizeBasedOnDamage = 3;
    [SerializeField] private float maxDamageToIncreaseSize = 100;
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private MMF_Player feedbacks;
    
    [Space(10)]
    [Header("Fading out")]
    [SerializeField] private float timeToStartFading;
    [SerializeField] private float  lifeTime;
    private float timer;
    private Image currentImage;

    [Space(20)]
    [Header("Element Icons")]
    public GameObject puncture;
    public GameObject slash;
    public GameObject impact;
    public GameObject explosion;
    public GameObject fire;
    public GameObject ice;
    public GameObject poison;
    public GameObject water;
    public GameObject electricity;

    [Space(10)]
    [Header("DamageOnStatus Icons")]
    public GameObject resistantIcon;
    public GameObject vulnerableIcon;

    //colors
    private Color hitColor = Color.white;
    private Color criticalHitColor = new(0.99609375f, 0.7734375f, 0);
    private Color superCriticalHitColor = new(0.99609375f, 0.3046875f, 0);
    private Color instaKillHitColor = new(1, 0, 0.3647f);

    //private float defaultScaleSize = 1;

    private MMF_Scale scaleFeedback;
    private void Awake()
    {
        scaleFeedback = feedbacks.GetFeedbackOfType<MMF_Scale>();
        currentImage = puncture.GetComponent<Image>();
    }


    private void Update()
    {
        if(timer < lifeTime)
        {
            timer += Time.deltaTime;
            if(timer >= timeToStartFading)
            {
                // Calculate fade duration.
                float fadeDuration = lifeTime - timeToStartFading;

                // Calculate normalized fade progress.
                float fadeProgress = (timer - timeToStartFading) / fadeDuration;

                // Invert progress to obtain alpha.
                float newAlpha = Mathf.Lerp(1f, 0f, fadeProgress);

                // Update the Image color.
                var color = currentImage.color;
                currentImage.color = new Color(color.r, color.g, color.b, newAlpha);

                //Update text alpha.
                color = textMesh.color;
                textMesh.color = new Color(color.r, color.g, color.b, newAlpha);

            }
        }
    }

    public void SetDamageText(float damage, CriticalDamage criticalDamage)
    {
        timer = 0;


        textMesh.text = Aux.FormatNumber(damage, 1);

        GetComponent<RectTransform>().localScale = Vector3.one;
        float newScale = 1;
        if (damage < maxDamageToIncreaseSize) newScale = maxSizeBasedOnDamage * damage / maxDamageToIncreaseSize + 1;
        else newScale = maxSizeBasedOnDamage;

        scaleFeedback.RemapCurveZero = newScale;
        scaleFeedback.RemapCurveOne = newScale + 1;

        feedbacks.PlayFeedbacks();


        if (criticalDamage == CriticalDamage.critical)
        {
            textMesh.color = criticalHitColor;
        }
        else if (criticalDamage == CriticalDamage.superCritical)
        {
            textMesh.color = superCriticalHitColor;
        }
        else if (criticalDamage == CriticalDamage.instaKill)
        {
            textMesh.color = instaKillHitColor;
            textMesh.text = GetInstaKillText();
            DeactivateAllDamageIcons();
        }
        else
        {
            textMesh.color = hitColor; ;
        }
    }

    public void ActivateDamageIcon(DamageType damageType)
    {
        timer = 0;
       
        // Deactivate all GameObjects
        DeactivateAllDamageIcons();

        // Activate the corresponding GameObject
        switch (damageType)
        {
            case DamageType.Puncture:
                puncture.SetActive(true);
                currentImage = puncture.GetComponent<Image>();
                break;
            case DamageType.Slash:
                slash.SetActive(true);
                currentImage = slash.GetComponent<Image>();
                break;
            case DamageType.Impact:
                impact.SetActive(true);
                currentImage = impact.GetComponent<Image>();
                break;
            case DamageType.Explosive:
                explosion.SetActive(true);
                currentImage = explosion.GetComponent<Image>();
                break;
            case DamageType.Fire:
                fire.SetActive(true);
                currentImage = fire.GetComponent<Image>();
                break;
            case DamageType.Ice:
                ice.SetActive(true);
                currentImage = ice.GetComponent<Image>();
                break;
            case DamageType.Poison:
                poison.SetActive(true);
                currentImage = poison.GetComponent<Image>();
                break;
            case DamageType.Water:
                water.SetActive(true);
                currentImage = water.GetComponent<Image>();
                break;
            case DamageType.Electricity:
                electricity.SetActive(true);
                currentImage = electricity.GetComponent<Image>();
                break;
        }

        var color = currentImage.color;
        currentImage.color = new Color(color.r, color.g, color.b, 1);
    }

    public void ActivateDamageOnStatusIcon(DamageOnStatus damageOnStatus)
    {
        switch (damageOnStatus)
        {
            case DamageOnStatus.resistant:
                resistantIcon.SetActive(true);
                vulnerableIcon.SetActive(false);
                break;
            case DamageOnStatus.vulnerable:
                resistantIcon.SetActive(false);
                vulnerableIcon.SetActive(true);
                break;
            case DamageOnStatus.none:
                resistantIcon.SetActive(false);
                vulnerableIcon.SetActive(false);
                break;
            default:
                break;
        }
    }

    private void DeactivateAllDamageIcons()
    {
        puncture.SetActive(false);
        slash.SetActive(false);
        impact.SetActive(false);
        explosion.SetActive(false);
        fire.SetActive(false);
        ice.SetActive(false);
        poison.SetActive(false);
        water.SetActive(false);
        electricity.SetActive(false);
    }

    private string GetInstaKillText()
    {
        return "Insta Kill";
    }
}


