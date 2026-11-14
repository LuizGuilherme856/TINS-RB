using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLogo : MonoBehaviour
{
    [SerializeField] private GameObject logoEnglish, logoChinese;

    [SerializeField] private LanguageData languageData;


    private void Start()
    {
        ChangeLogoMethod();
        Invoke(nameof(ChangeLogoMethod), 0.1f);
        languageData.OnLanguageChange += ChangeLogoMethod;

        //InvokeRepeating(nameof(ChangeLogoMethod), 0.1f, 1);
    }

    private void ChangeLogoMethod()
    {
        try
        {
            if (languageData.currentLanguage == Language.chinese)
            {
                logoEnglish.SetActive(false);
                logoChinese.SetActive(true);
            }
            else
            {
                logoEnglish.SetActive(true);
                logoChinese.SetActive(false);
            }

        }
        catch (System.Exception e)
        {

            Debug.Log("Erro ao mudar o logo: "+e);
        }

    }

    private void OnDisable()
    {
        languageData.OnLanguageChange -= ChangeLogoMethod;
    }
}
