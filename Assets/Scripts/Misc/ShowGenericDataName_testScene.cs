using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;

/// <summary>
/// Display a GenericData asset's name in a text field for identification.
/// Used in the test scene.
/// </summary>
public class ShowGenericDataName_testScene : MonoBehaviour
{
    [SerializeField] private GenericData dataScriptableObj;
    [SerializeField] private LanguageData languageData;

    [Header("Caso seja Rune ou Arma")]
    [SerializeField] private RuneRitualManager ritualManager;
    [SerializeField] private BuyWeapon buyWeapon;
    
    private TextMeshProUGUI textField;
    private void Awake()
    {

        if(ritualManager != null)
        {
            dataScriptableObj = ritualManager.GetRuneRitualHolder();
        }
        else if (buyWeapon != null)
        {
            switch (buyWeapon.weaponType)
            {
                case BuyWeapon.WeaponType.gun:
                    dataScriptableObj = buyWeapon.gun;
                    break;
                case BuyWeapon.WeaponType.melee:
                    dataScriptableObj = buyWeapon.meleeWeapon;
                    break;
                default:
                    break;
            }

        }



        textField = GetComponent<TextMeshProUGUI>();

        if(languageData == null )
        {
            textField.text = dataScriptableObj.GetName(Language.english);
        }
        else
        {
            textField.text = dataScriptableObj.GetName(languageData.currentLanguage);
        }

        if(buyWeapon != null)
        {
            var gunData = (GunData)dataScriptableObj;
            //textField.text += " : "+Aux.FormatNumber( gunData.GetDPS(), 2);
        }
    }

}
