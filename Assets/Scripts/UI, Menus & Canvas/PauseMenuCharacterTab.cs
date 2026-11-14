using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PauseMenuCharacterTab : MonoBehaviour
{

    [Tooltip("Campo de texto para exibir o nome do personagem.")]
    [SerializeField] private TextMeshProUGUI characterNameText;

    [Tooltip("Campo de texto para exibir a descri��o do personagem.")]
    [SerializeField] private TextMeshProUGUI characterDescriptionText;

    [SerializeField] private Image characterIcon;


    [SerializeField] private RunData runData;
    [SerializeField] private CharacterData[] characterDatas;
    
    [SerializeField] private LanguageData languageData;

    private void OnEnable()
    {
        // Verify RunData and LanguageData are assigned.
        if (runData == null)
        {
            Debug.LogError("RunData n�o atribu�do. Certifique-se de atribuir uma inst�ncia v�lida.");
            return;
        }
        if (languageData == null)
        {
            Debug.LogError("LanguageData n�o atribu�do. Certifique-se de atribuir uma inst�ncia v�lida.");
            return;
        }
        // Refresh character information when the tab opens.
        UpdateCharacterInfo(runData);
    }

    /// <summary>
    /// Populate text from the CharacterData referenced by RunData.
    /// </summary>
    /// <param name="runData">RunData containing the selected CharacterData.</param>
    public void UpdateCharacterInfo(RunData runData)
    {
        if (runData == null)
        {
            Debug.LogError("RunData � nulo. Certifique-se de passar uma inst�ncia v�lida.");
            return;
        }

        var characterData = runData.currentCharacter;

        // Update text fields with character information.
        characterNameText.text = characterData.GetName(languageData.currentLanguage);
        characterDescriptionText.text = characterData.GetDescription(languageData.currentLanguage);
        characterIcon.sprite = characterData.bigPortrait;

        

    }
}
