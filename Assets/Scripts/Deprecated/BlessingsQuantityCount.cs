//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;

//public class BlessingsQuantityCount : MonoBehaviour
//{
//    //[SerializeField] private TextMeshProUGUI quantityTxt;
//    private BlessingType blessingType;
//    [SerializeField] private PlayerData playerData;
//    [SerializeField] private Image quantityIcon;
//    [SerializeField] private float fillSpeed = 2f;


//    private BlessingUpgradeData blessingUpgradeData;
//    private BlessingMenuUpgradeDetails blessingMenuUpgradeDetails;
//    private float currentFillAmount = 0;
//    private readonly float FILL_COEFFICIENT = 0.1111f;


//    private void Awake()
//    {
//        blessingMenuUpgradeDetails = GetComponent<BlessingMenuUpgradeDetails>();
//        blessingUpgradeData = blessingMenuUpgradeDetails.GetBlessingUpgradeData();
//    }
//    private void Start()
//    {
//        blessingType = blessingUpgradeData.blessingType;
//        UpdateQuantity();

//        playerData.OnUpdateBlessingQuantity += UpdateQuantity;
//    }

//    private void FixedUpdate()
//    {
//        FillImage();
//    }

//    public void UpdateQuantity()
//    {
//        switch (blessingType)
//        {
//            case BlessingType.extraHP:
//                currentFillAmount = playerData.extraInitialHP_nOfBlessings * FILL_COEFFICIENT;
//                break;
//            case BlessingType.attackRate:
//                currentFillAmount = playerData.meleeAttackRateMultiplier_nOfBlessings * FILL_COEFFICIENT;
//                break;
//            case BlessingType.walkSpeed:
//                currentFillAmount = playerData.walkSpeedMultiplier_nOfBlessings * FILL_COEFFICIENT;
//                break;
//            case BlessingType.specialCooldown:
//                currentFillAmount = playerData.specialAbilityCooldownMultiplier_nOfBlessings * FILL_COEFFICIENT;
//                break;
//            case BlessingType.extraPoints:
//                currentFillAmount = playerData.extraInitialPoints_nOfBlessings * FILL_COEFFICIENT;
//                break;
//            case BlessingType.extraShotChance:
//                currentFillAmount = playerData.extraShotChance_nOfBlessings * FILL_COEFFICIENT;
//                break;
//            case BlessingType.extraPointPerKill:
//                currentFillAmount = playerData.extraPointPerKill_nOfBlessings * FILL_COEFFICIENT;
//                break;
//            case BlessingType.fasterReload:
//                currentFillAmount = playerData.fasterReload_nOfBlessings * FILL_COEFFICIENT;
//                break;
//            default:
//                break;
//        }

//    }

//    private void FillImage()
//    {
//        if (quantityIcon.fillAmount < currentFillAmount)
//        {
//            quantityIcon.fillAmount += Time.fixedDeltaTime * fillSpeed;
//        }
//    }

//}

//public enum BlessingType { extraHP, attackRate, walkSpeed, specialCooldown, extraPoints, extraShotChance, extraPointPerKill, fasterReload, none }