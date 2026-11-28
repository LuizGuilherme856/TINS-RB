using System;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Used by rune purchase stations.
/// </summary>
public class BuyRune : InteractWithPlayer
{
    [SerializeField] private SpriteRenderer iconSpriteRenderer;
    //[SerializeField] private Transform upgradeIconPos;

    [SerializeField] private RuneInventory runeInventory;
    public GlobalStats globalStats;

    public bool isReady = false;

    private RuneData runeData;
    private RitualsCounter sacrificesCounter;
    private GunManager gunManager;
    private Health playerHealth;
    public event Action OnBuyUpgrade;

    protected override void Awake()
    {
        base.Awake();
        sacrificesCounter = FindFirstObjectByType<RitualsCounter>();

        FindRuneInventory();
    }

    private void OnEnable()
    {
        if (pointManager != null)
        {
            pointManager.OnPointChange += RefreshBuyTextIfPlayerIsClose;
        }

        if (runeInventory != null)
        {
            runeInventory.OnBuyRune += RefreshBuyTextIfPlayerIsClose;
        }
    }

    private void OnDisable()
    {
        if (pointManager != null)
        {
            pointManager.OnPointChange -= RefreshBuyTextIfPlayerIsClose;
        }

        if (runeInventory != null)
        {
            runeInventory.OnBuyRune -= RefreshBuyTextIfPlayerIsClose;
        }
    }

    private void FindRuneInventory()
    {
        if (runeInventory == null)
        {
            Debug.Log($"Rune Inventory esta nulo em {gameObject.name}. Procurando pelo AssetFinder...");
            runeInventory = AssetFinder.GetRuneInventory();
        }
    }

    protected override void StartInteraction()
    {
        base.StartInteraction();


        if (gunManager == null)
        {
            gunManager = player.GetComponentInChildren<GunManager>();
            playerHealth = player.GetComponent<Health>();
        }

        if (isActiveAndEnabled == false)
        {
            return;
        }
        SetBuyUpgradeText();
    }

    private void SetBuyUpgradeText()
    {
        var language = languageData.currentLanguage;

        if (isReady == false || runeData == null) return;

        string coloredPrice;
        //Show the price in green when affordable.
        if (pointManager.currentPoints >= GetPrice())
        {
            coloredPrice = UiColors.green_ + GetPrice() + UiColors.endColor_;
        }
        else
        {
            coloredPrice = UiColors.red_ + GetPrice() + UiColors.endColor_;
        }
        switch (language)
        {
            case Language.english:
                generalTextBoxUI.SetText(interactionButton + " Buy " + runeData.GetName(language) + " (Cost: " + coloredPrice + ")");
                break;
            case Language.chinese:
                generalTextBoxUI.SetText(interactionButton + " 购买  " + runeData.GetName(language) + " (消耗: " + coloredPrice + ")");
                break;
            case Language.portuguese:
                generalTextBoxUI.SetText(interactionButton + " Comprar " + runeData.GetName(language) + " (Preço: " + coloredPrice + ")");
                break;
            case Language.spanish:
                generalTextBoxUI.SetText(interactionButton + " Comprar " + runeData.GetName(language) + " (Precio: " + coloredPrice + ")");
                break;
            default:
                break;
        }
    }

    protected override void InteractionAction()
    {
        if (isReady == false) return;
        int newPrice = GetPrice();


        if (pointManager.currentPoints >= newPrice)
        {
            BuyUpgradeMethod(newPrice);
        }
        else
        {
            notEnoughPointsFeedbacks?.TriggerNotEnoughPointsFeedbacks();
        }

    }

    private void BuyUpgradeMethod(int newPrice)
    {
        pointManager.RemovePoints(newPrice);
        buyFeedback.PlayFeedbacks();
        Upgrade();
        SetBuyUpgradeText(); //updates the price
    }

    private void RefreshBuyTextIfPlayerIsClose()
    {
        if (!isTouchingPlayer) return;
        SetBuyUpgradeText();
    }

    private int GetPrice()
    {
        float runePrice = runeData.price * globalStats.ForCurrentRun.runePriceMultiplier * globalStats.ForCurrentRun.globalPriceMultiplier; //Price after skill discounts.
        float price = runePrice + runePrice * runeData.priceMultiplier * runeInventory.GetUpgradeCount(runeData);
        if (price > runeData.maxPrice) price = runeData.maxPrice;
        return (int)price;
    }

    public void Upgrade()
    {
        runeInventory.AddUpgrade(runeData);
        OnBuyUpgrade?.Invoke();


        //Track orbital upgrade ActiveEffects so they can be reapplied after scene changes.
        //because orbitals must be recreated.
        foreach (var activeEffectsItem in runeData.activeEffects)
        {
            if (activeEffectsItem.GetType() == typeof(OrbitalUpgrade_ActiveEffect))
            {
                //The explicit cast is required despite the preceding type check.
                runeInventory.globalStats.ForCurrentRun.orbitalUpgrades.Add((OrbitalUpgrade_ActiveEffect)activeEffectsItem);
            }
        }


    }

    public void SetRuneData(RuneData upgradeData)
    {
        this.runeData = upgradeData;
        iconSpriteRenderer.sprite = upgradeData.smallerIcon;
        StartInteraction(); //Show the text immediately after unlocking.
    }

    public RuneData GetRuneData()
    {
        return runeData;
    }
}
