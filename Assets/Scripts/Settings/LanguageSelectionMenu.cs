using Kamgam.UGUIComponentsForSettings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageSelectionMenu : MonoBehaviour
{
    [SerializeField] private OptionsButtonUGUI languageButton;
    [SerializeField] private LanguageData languageData;

    private void Start()
    {
        languageButton.OnValueChanged += OnLanguageChanged;
        UpdateLanguageOnUi();
    }

    private void UpdateLanguageOnUi()
    {
        switch (languageData.currentLanguage)
        {
            case Language.english:
                languageButton.SelectedIndex = 0;
                break;
            case Language.chinese:
                languageButton.SelectedIndex = 1;
                break;
            case Language.portuguese:
                languageButton.SelectedIndex = 2;
                break;
            case Language.spanish:
                languageButton.SelectedIndex = 3;
                break;
                
        }
    }

    private void OnLanguageChanged(int optionIndex)
    {
        languageData.SetLanguage(optionIndex);
    }

    public void SetLanguage(int languageId)
    {
        languageData.SetLanguage(languageId);
    }
}
