//using MoreMountains.Feedbacks;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;

//public class BuyBlessings : MonoBehaviour
//{
//    [SerializeField] private PlayerData playerData;
//    [SerializeField] private TextMeshProUGUI priceTxt;


   
//    [SerializeField] private MMF_Player buyFeedback, cantBuyNotSelected, cantBuyMaxQuantity, cantBuyNotEnoughPoints;
//    private BlessingType selectedBlessingType = BlessingType.none;
//    private int price, maxQuantity;

//    public event Action OnBuyBlessing;

//    private void Start()
//    {
        
//    }

//    public void BuyBlessing()
//    {
//        if(selectedBlessingType == BlessingType.none)
//        {
//            cantBuyNotSelected.PlayFeedbacks();
//            return;
//        }
//        else if( playerData.GetNumberOfBlessings(selectedBlessingType) >= maxQuantity)
//        {
//            cantBuyMaxQuantity.PlayFeedbacks();
//            return;
//        }
 

//        if(playerData.horns >= price)
//        {
//            playerData.AddHorns(-price);
//            playerData.AddBlessing(selectedBlessingType);
//            buyFeedback.PlayFeedbacks();
//            OnBuyBlessing?.Invoke();

//        }
//        else
//        {
//            cantBuyNotEnoughPoints.PlayFeedbacks();
//        }


//    }

//    public void SelectBlessing(int price, BlessingType blessingType, int maxQuantity)
//    {
//        this.price = price;
//        selectedBlessingType= blessingType;
//        this.maxQuantity = maxQuantity;
//    }

//    public BlessingType GetSelectedBlessing()
//    {
//        return selectedBlessingType;
//    }

//}
