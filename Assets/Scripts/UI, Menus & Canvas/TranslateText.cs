using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TranslateText : MonoBehaviour
{
    [SerializeField] private LanguageData languageData;
    private TextMeshProUGUI textMesh;

    [SerializeField] private bool changeFontOnly = false, fontWithTexture = false, simpleFont = false,
        changeOnlyChineseFont = false;

    [TextArea]
    [SerializeField] private string englishText, chineseText, portugueseText, spanishText;


    private void Awake()
    {
        textMesh= GetComponent<TextMeshProUGUI>();
        if(languageData == null)
        {
            Debug.Log($"Language Data esta nulo em {gameObject.name}. Procurando pelo assetFinder");
            languageData = AssetFinder.GetLanguageData();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        languageData.OnLanguageChange += SetCurrentLanguageText;
    }

    private void OnEnable()
    {
        SetCurrentLanguageText();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetCurrentLanguageText()
    {
        SetTranslatedText(languageData.currentLanguage);
        if(changeOnlyChineseFont && languageData.currentLanguage != Language.chinese)
        {
            return;
        }

        textMesh.font = languageData.GetFont(fontWithTexture, simpleFont);
    }

    private void SetTranslatedText(Language language)
    {
        if (changeFontOnly) return;

        switch (language)
        {
            case Language.english:
                textMesh.text = englishText;
                break;
            case Language.chinese:
                textMesh.text = chineseText;
                break;
            case Language.portuguese:
                textMesh.text = portugueseText;
                break;
            case Language.spanish:
                textMesh.text = spanishText;
                break;
            default:
                Debug.LogWarning("LANGUAGE NOT FOUND");
                textMesh.text = englishText;
                break;
        }
    }
}
