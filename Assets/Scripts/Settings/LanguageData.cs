using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//[CreateAssetMenu(fileName = "language", menuName = "language")]
public class LanguageData : ScriptableObject
{
    [field: SerializeField] public Language currentLanguage { get; private set; }
    [SerializeField] private TMP_FontAsset chineseFont, englishFont, simpleEnglishFont;
    [SerializeField] private TMP_FontAsset chineseFontWithTexture, englishFontWithTexture, simpleEnglishFontWithTexture;

    private GameDataAndSettings gameDataAndSettings;

    public event Action OnLanguageChange; 

    private void Awake()
    {
        gameDataAndSettings = FindFirstObjectByType<GameDataAndSettings>();
    }

    public int GetCurrentLanguageID()
    {
        return GetIdFromLanguage(currentLanguage);
    }

    private int GetIdFromLanguage(Language language)
    {
        switch (language)
        {
            case Language.english:
                return 0;
            case Language.chinese:
                return 1;
            case Language.portuguese:
                return 2;
            case Language.spanish:
                return 3;
            default:
                Debug.LogWarning("LANGUAGE NOT FOUND");
                return 0;
        }
    }

    private Language GetLanguageFromID(int languageID)
    {
        if (languageID == 0) return Language.english;
        else if (languageID == 1) return Language.chinese;
        else if (languageID == 2) return Language.portuguese;
        else if (languageID == 3) return Language.spanish;
        else
        {
            Debug.LogWarning("LANGUAGE NOT FOUND");
            return Language.english;
        }
    }

    public TMP_FontAsset GetFont(bool fontWithTexture = false, bool simpleFont = false)
    {
        if (fontWithTexture)
        {
            if (currentLanguage == Language.chinese) return chineseFontWithTexture;
            else if (simpleFont == true) return simpleEnglishFontWithTexture;
            else return englishFontWithTexture;
        }
        else
        {
            if (currentLanguage == Language.chinese) return chineseFont;
            else if (simpleFont == true) return simpleEnglishFont;
            else return englishFont;
            

        }
    }

    public void SetLanguage(int languageID, bool saveToPrefs = true) 
    {
        currentLanguage = GetLanguageFromID(languageID);
        OnLanguageChange?.Invoke();
        //PlayerPrefs.SetInt(gameDataAndSettings.LANGUAGE_NAME, languageID); //Preferred approach, but previously caused a bug.
        
        if (saveToPrefs == false) return;
        PlayerPrefs.SetInt("language", languageID);

        PlayerPrefs.Save();
    }

    public void SetDefaultLanguage()
    {
        SetLanguage(0);
    }

}

public enum Language { english, chinese, portuguese, spanish };
