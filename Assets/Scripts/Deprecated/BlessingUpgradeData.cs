//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//    //[CreateAssetMenu(fileName = "blessing", menuName = "blessing")]
//public class BlessingUpgradeData : GenericData
//{


//    [SerializeField] private int maxPrice, initialPrice, priceIncrease;
//    [SerializeField] private PlayerData playerData;

//    public BlessingType blessingType;
//    public int maxQuantity = 9;

//    public int GetPrice()
//    {
//        int quantity=0;
//        switch (blessingType)
//        {
//            case BlessingType.extraHP:
//                quantity = playerData.extraInitialHP_nOfBlessings;
//                break;
//            case BlessingType.attackRate:
//                quantity = playerData.meleeAttackRateMultiplier_nOfBlessings;
//                break;
//            case BlessingType.walkSpeed:
//                quantity = playerData.walkSpeedMultiplier_nOfBlessings;
//                break;
//            case BlessingType.specialCooldown:
//                quantity = playerData.specialAbilityCooldownMultiplier_nOfBlessings;
//                break;
//            case BlessingType.extraPoints:
//                quantity = playerData.extraInitialPoints_nOfBlessings;
//                break;
//            case BlessingType.extraShotChance:
//                quantity = playerData.extraShotChance_nOfBlessings;
//                break;
//            case BlessingType.extraPointPerKill:
//                quantity = playerData.extraPointPerKill_nOfBlessings;
//                break;
//            case BlessingType.fasterReload:
//                quantity = playerData.fasterReload_nOfBlessings;
//                break;
//            default:
//                break;
//        }

//        int price = initialPrice + priceIncrease * quantity;
        
//        if (price < maxPrice) return price;
//        else return maxPrice;
//    }

//}
