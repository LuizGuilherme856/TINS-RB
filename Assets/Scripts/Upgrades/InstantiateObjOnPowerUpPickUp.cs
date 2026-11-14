using MoreMountains.Feedbacks;
using System;
using TMPro;
using UnityEngine;

public class InstantiateObjOnPowerUpPickUp : MonoBehaviour
{

    [SerializeField] private LanguageData languageData;

    [SerializeField] private string englishText, chineseText, portugueseText, spanishText;
    [SerializeField] private GameObject objToInstantiate;
    private PowerUp powerUp;
    private KeyPickUp keyPickUp;

    private void Awake()
    {
        powerUp = GetComponent<PowerUp>();
        if(powerUp == null)
        {
            keyPickUp = GetComponent<KeyPickUp>();
        }
    }


    private void OnEnable()
    {
        if (powerUp == null)
        {
            keyPickUp.OnPowerUpPickUp += InstantiateObject;
        }
        else
        {
            powerUp.OnPowerUpPickUp += InstantiateObject;

        }
    }

    private void OnDisable()
    {
        if (powerUp == null)
        {
            keyPickUp.OnPowerUpPickUp -= InstantiateObject;
        }
        else
        {
            powerUp.OnPowerUpPickUp -= InstantiateObject;

        }
    }


    private void InstantiateObject()
    {
        var obj = Instantiate(objToInstantiate, transform.position, transform.rotation);

        obj.GetComponentInChildren<TextMeshProUGUI>().text = GetText();
    }

    private string GetText()
    {
        switch (languageData.currentLanguage)
        {
            case Language.english:
                return englishText;
            case Language.chinese:
                return chineseText;
            case Language.portuguese:
                return portugueseText;
            case Language.spanish:
                return spanishText;
            default:
                Debug.LogError("Nao encontrou o idioma do power up: "+gameObject.name);
                return "";
        }


    }
}
