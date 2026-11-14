using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Count station uses and break it after the configured limit.
/// </summary>
public class RuneStationBreakManager : MonoBehaviour
{
    [SerializeField] private RuneInventory runeInventory;
    [SerializeField] private MMF_Player breakingFeedbacks, stompFeedback;
    private Animator animator;
    private BuyRune buyUpgrade;
    private int timesBought = 0;
    private RuneData runeData;

    private void Awake()
    {
        buyUpgrade = GetComponent<BuyRune>();
        animator = GetComponent<Animator>();

        FindRuneInventory();
    }

    private void FindRuneInventory()
    {
        if (runeInventory == null)
        {
            Debug.Log($"Rune Inventory esta nulo em {gameObject.name}. Procurando pelo AssetFinder...");
            runeInventory = AssetFinder.GetRuneInventory();
        }
    }

    private void OnEnable()
    {
        buyUpgrade.OnBuyUpgrade += UpdateTimesBought;
    }

    private void UpdateTimesBought()
    {
        if(runeData == null) runeData = buyUpgrade.GetRuneData();

        timesBought++;

        if (timesBought == 2) animator.SetTrigger("damage");
        if (timesBought == 4) animator.SetTrigger("damage");


        if (timesBought >= runeData.numberOfTimesCanBeBought)
        {
            DestroyStation();
        }
    }

    private void DestroyStation()
    {
        animator.SetTrigger("break");
        buyUpgrade.DisableShaderOnInteraction();
        buyUpgrade.ResetText();
        buyUpgrade.isReady = false;
        breakingFeedbacks.PlayFeedbacks();
        foreach (Transform child in transform)
        {
            //Keep the feedback objects alive.
            if (child.name.Equals("Feedbacks")) continue;

            child.gameObject.SetActive(false);
        }
        buyUpgrade.ResetText();
    }

    /// <summary>
    /// Animation event timed to the stone hitting the ground.
    /// </summary>
    public void PlayTombStoneStomp()
    {
        stompFeedback.PlayFeedbacks();
    }

}
