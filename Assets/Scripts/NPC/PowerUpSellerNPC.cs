using System;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSellerNPC : InteractWithPlayer
{
    [Header("Object to Spawn")]
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private Transform spawnPoint; // Optional, can be null

    [Header("Purchase")]
    [SerializeField] private int price = 750;
    public DialogueLine buyText;
    public bool canBuy = true;

        [Header("Sale Limit")]
    [SerializeField] private int maxSales = 5;
    private int currentSales = 0;

    public GlobalStats globalStats;

    private bool loggedMissingGlobalStats_TechBooh;


    public event Action OnPurchase;

    protected override void Awake()
    {
        base.Awake();
        TryResolveGlobalStats_TechBooh();
    }

    protected override void StartInteraction()
    {
        base.StartInteraction();
        SetBuyText();
    }

    private void SetBuyText()
    {
        int currentPrice = GetPrice();
        string coloredPrice = pointManager.currentPoints >= currentPrice
            ? UiColors.green_ + currentPrice + UiColors.endColor_
            : UiColors.red_ + currentPrice + UiColors.endColor_;


        if (currentSales >= maxSales)
        {
            generalTextBoxUI.SetText(string.Empty);
        }
        else
        {
            string limitText = currentSales >= maxSales
            ? ""
            : $" ({currentSales}/{maxSales})";

        generalTextBoxUI.SetText(interactionButton + buyText.GetLine(languageData.currentLanguage) + coloredPrice + ")  " + limitText);

        }
    }

    protected override void InteractionAction()
    {
        if (!canBuy || currentSales >= maxSales)
        {
            cantBuyFeedback.PlayFeedbacks();
            return;
        }

        // TechBooh: calculate once so affordability checking and charging
        // use exactly the price shown to the player.
        int currentPrice = GetPrice();
        if (pointManager.currentPoints >= currentPrice)
        {
            pointManager.RemovePoints(currentPrice);
            buyFeedback.PlayFeedbacks();

            // Spawn the object
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            Instantiate(objectToSpawn, pos, Quaternion.identity);

            currentSales++;
            OnPurchase?.Invoke();
        }
        else
        {
            notEnoughPointsFeedbacks?.TriggerNotEnoughPointsFeedbacks();
        }

        SetBuyText();
    }

    public int GetPrice()
    {
        if (globalStats == null)
        {
            TryResolveGlobalStats_TechBooh();
        }

        if (globalStats == null)
        {
            if (!loggedMissingGlobalStats_TechBooh)
            {
                Debug.LogError("PowerUpSellerNPC nao encontrou GlobalStats em " + gameObject.name + ". A venda continuara usando o preco base de " + price + ".", this);
                loggedMissingGlobalStats_TechBooh = true;
            }

            return price;
        }

        return (int)(price * globalStats.ForCurrentRun.globalPriceMultiplier);
    }

    private void TryResolveGlobalStats_TechBooh()
    {
        try
        {
            globalStats = AssetFinder.GetGlobalStats();
        }
        catch (Exception exception)
        {
            if (!loggedMissingGlobalStats_TechBooh)
            {
                Debug.LogError("PowerUpSellerNPC falhou ao buscar GlobalStats em " + gameObject.name + ". A venda continuara pelo preco normal. Erro: " + exception.Message, this);
                loggedMissingGlobalStats_TechBooh = true;
            }
        }
    }
}
