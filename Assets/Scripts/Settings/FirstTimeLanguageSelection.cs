using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Choose the game language on first launch.
/// </summary>
public class FirstTimeLanguageSelection : MonoBehaviour
{
    public LanguageData languageData; // Reference to the LanguageData scriptable object that manages language settings
    private readonly string KEY_NAME = "LanguageSelected";

    private void Awake()
    {
        if (PlayerPrefs.HasKey(KEY_NAME))
        {

            return;
            

        }
        else
        {
            PlayerPrefs.SetInt(KEY_NAME, 1);
        }

        if (Application.systemLanguage == SystemLanguage.Spanish)
        {
            languageData.SetLanguage(3); // Set language to Spanish if the system language is Spanish
        }
        else if (Application.systemLanguage == SystemLanguage.English)
        {
            languageData.SetLanguage(0); // Set language to English if the system language is English
        }
        else if(Application.systemLanguage == SystemLanguage.Chinese)
        {
            languageData.SetLanguage(1); // Set language to Chinese if the system language is Chinese
        }
        else if (Application.systemLanguage == SystemLanguage.Portuguese)
        {
            languageData.SetLanguage(2); // Set language to Portuguese if the system language is Portuguese
        }
        else
        {
            languageData.SetLanguage(0); // Default to English if no match found
        }

    }



}
