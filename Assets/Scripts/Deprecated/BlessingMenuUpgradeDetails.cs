//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;

////Populate blessing name/description panels and update the selected blessing.

//public class BlessingMenuUpgradeDetails : MonoBehaviour
//{
//    [SerializeField] private TextMeshProUGUI itemNameTxt, itemDescriptionTxt, itemPriceTxt;
//    [SerializeField] private BuyBlessings buyBlessings;
//     [SerializeField] private BlessingUpgradeData blessingUpgradeData;
//    [SerializeField] private LanguageData languageData;


//    private void Start()
//    {
//        buyBlessings.OnBuyBlessing += UpdateInfo;
//    }

//    public void SetItemDetails()
//    {
//        itemNameTxt.text = blessingUpgradeData.GetName(languageData.currentLanguage);
//        itemDescriptionTxt.text = blessingUpgradeData.GetDescription(languageData.currentLanguage);

//        itemPriceTxt.text = blessingUpgradeData.GetPrice().ToString();

//    }

//    private void UpdateInfo()
//    {
//        //Refresh the blessing price, which changes after each purchase.
//        if(buyBlessings.GetSelectedBlessing() == blessingUpgradeData.blessingType)
//        {
//            SetSelectedBlessing();
//            SetItemDetails(); 
//        }
//    }

//    public void SetSelectedBlessing()
//    {
//        buyBlessings.SelectBlessing(blessingUpgradeData.GetPrice(), blessingUpgradeData.blessingType, blessingUpgradeData.maxQuantity);
//    }

//    public BlessingUpgradeData GetBlessingUpgradeData()
//    {
//        return blessingUpgradeData;
//    }
//}
