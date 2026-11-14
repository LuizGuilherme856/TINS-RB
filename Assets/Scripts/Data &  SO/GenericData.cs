
using System;
using UnityEditor;
using UnityEngine;

//Base class for ScriptableObjects with a name and description.
[System.Serializable]
public class GenericData : ScriptableObject
{
   // [Header("#---- NAME AND DESCRIPTION ----#")]
    //[Space(10)]
    [Header("English")]
    [SerializeField] private string nameEnglish;
    [TextArea][SerializeField] private string descriptionEnglish;
    [Header("Chinese")]
    [SerializeField] private string nameChinese;
    [TextArea][SerializeField] private string descriptionChinese;
    [Header("Portuguese")]
    [SerializeField] private string namePortuguese;
    [TextArea][SerializeField] private string descriptionPortuguese;
    [Header("Spanish")]
    [SerializeField] private string nameSpanish;
    [TextArea][SerializeField] private string weaponDescriptionSpanish;



    public string GetName(Language language)
    {
        switch (language)
        {
            case Language.english:
                return nameEnglish;
            case Language.chinese:
                return nameChinese;
            case Language.portuguese:
                return namePortuguese;
            case Language.spanish:
                return nameSpanish;
            default:
                Debug.LogWarning("language not found");
                return nameEnglish;
        }
    }

    public string GetDescription(Language language, bool useBulletPoints = false)
    {

        string text = string.Empty;
        switch (language)
        {
            case Language.english:
                text = descriptionEnglish;
                break;
            case Language.chinese:
                text = descriptionChinese;
                break;
            case Language.portuguese:
                text = descriptionPortuguese;
                break;
            case Language.spanish:
                text = weaponDescriptionSpanish;
                break;
            default:
                Debug.LogWarning("language not found");
                text = descriptionEnglish;
                break;
        }

        if(useBulletPoints) text = Aux.FormatUpgradeText(text);

        return text;
    }
}

