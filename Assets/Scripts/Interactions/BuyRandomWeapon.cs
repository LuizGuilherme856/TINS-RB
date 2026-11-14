using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BuyWeapon;
using Random = UnityEngine.Random;

public class BuyRandomWeapon : InteractWithPlayer
{
    [SerializeField] private int price = 500;
    [SerializeField] private GunData[] guns;
    [SerializeField] private float timeToDisappear = 5f;
    private List<Sprite> gunSprites = new();

    // [Range(0,2)] public float chanceToGetBadWeapons = 1.2f;

    [Header("Rarity Chances\nTem que somar 1 \n(1 - somatorio das chances = chance comum)")]
    [Range(0f, 1f)] public float chanceLegendary = 0.05f;
    [Range(0f, 1f)] public float chanceEpic = 0.10f;
    [Range(0f, 1f)] public float chanceRare = 0.20f;
    [Range(0f, 1f)] public float chanceUncommon = 0.25f;
    // The remainder is Common.

    public BuyRandomGunState currentState;
    private ItemRoulette itemRoulette;
    public event Action OnBuyRandomWeapon, OnRemoveWeapon;

    public GlobalStats globalStats;

    private int choosenIndex;
    // private float minDPS = 9999f, maxDPS = -1;

    private Queue<int> lastIndexes = new Queue<int>(2);

    protected override void Awake()
    {
        base.Awake();
        itemRoulette = GetComponent<ItemRoulette>();

        foreach (var gunItem in guns)
        {
            //Get all weapon sprites.
            gunSprites.Add(gunItem.buyIcon);
        }
        itemRoulette.itemSprites = gunSprites;

        itemRoulette.OnFinishRoulette += WeaponReady;
        itemRoulette.OnStatusChange += UpdateUiText;

        currentState = BuyRandomGunState.waiting;

#if UNITY_EDITOR
        // PrintGunsByRarity();
        // SimulateRandomDraws();
        // SimulateRandomDraws();
        // SimulateRandomDraws();
#endif

    }





    protected override void StartInteraction()
    {
        base.StartInteraction();

        UpdateUiText();
    }

    public void UpdateUiText()
    {
        string statusText = null;

        switch (currentState)
        {
            case BuyRandomGunState.raffling:
                statusText = SetWaitingText();
                break;
            case BuyRandomGunState.weaponReady:
                statusText = SetGetWeaponText();
                break;
            case BuyRandomGunState.waiting:
                statusText = BuyRandomWeaponText();
                break;
            default:
                break;
        }

        generalTextBoxUI.SetText(statusText);
    }



    protected override void InteractionAction()
    {

        switch (currentState)
        {
            case BuyRandomGunState.waiting:
                CheckIfPlayerCanPay();
                break;
            case BuyRandomGunState.raffling:
                cantBuyFeedback.PlayFeedbacks();
                break;
            case BuyRandomGunState.weaponReady:

                if (gunInventory.HasThisGun(guns[choosenIndex]))
                {
                    cantBuyFeedback.PlayFeedbacks();
                }
                else
                {
                    gunInventory.AddGunToInventory(guns[choosenIndex]);
                    buyFeedback.PlayFeedbacks();
                    DisappearGun();
                }

                break;
            default:
                break;
        }
        UpdateUiText();

    }

    private void DisappearGun()
    {
        if (currentState == BuyRandomGunState.weaponReady)
        {
            currentState = BuyRandomGunState.waiting;
            itemRoulette.ActivateRouletteItem();
            OnRemoveWeapon?.Invoke();
            CancelInvoke();

        }
    }


    private void CheckIfPlayerCanPay()
    {
        if (pointManager.currentPoints < GetPrice())
        {
            notEnoughPointsFeedbacks?.TriggerNotEnoughPointsFeedbacks();
        }
        else
        {
            RandomizeGun();

            currentState = BuyRandomGunState.raffling;
            itemRoulette.StartRoulette(choosenIndex);
            pointManager.RemovePoints(GetPrice());
            buyFeedback.PlayFeedbacks();
            OnBuyRandomWeapon?.Invoke();


        }
    }

    private void RandomizeGun()
    {
        const int maxTries = 20;
        int tries = 0;
        int[] forbiddenIndexes = lastIndexes.ToArray();

        // Check whether the last two weapons were Common.
        bool lastTwoWereCommon = false;
        if (lastIndexes.Count == 2)
        {
            var lastTwo = lastIndexes.ToArray();
            if (guns[lastTwo[0]].rarity == Rarity.Common && guns[lastTwo[1]].rarity == Rarity.Common)
                lastTwoWereCommon = true;
        }

        while (tries < maxTries)
        {
            // Choose a rarity.
            float roll = Random.value;
            Rarity chosenRarity;
            if (roll < chanceLegendary)
                chosenRarity = Rarity.Legendary;
            else if (roll < chanceLegendary + chanceEpic)
                chosenRarity = Rarity.Epic;
            else if (roll < chanceLegendary + chanceEpic + chanceRare)
                chosenRarity = Rarity.Rare;
            else if (roll < chanceLegendary + chanceEpic + chanceRare + chanceUncommon)
                chosenRarity = Rarity.Uncommon;
            else
                chosenRarity = Rarity.Common;

            // Force Rare after two consecutive Common weapons.
            if (lastTwoWereCommon && chosenRarity == Rarity.Common)
            {
                chosenRarity = Rarity.Rare;
            }

            // Filter by rarity and exclude the last two selections.
            var availableGuns = guns
                .Select((g, idx) => new { gun = g, idx })
                .Where(x => x.gun.rarity == chosenRarity && !forbiddenIndexes.Contains(x.idx))
                .ToArray();

            if (availableGuns.Length > 0)
            {
                var chosen = availableGuns[Random.Range(0, availableGuns.Length)];
                choosenIndex = chosen.idx;

                // Update the recent-weapon queue.
                lastIndexes.Enqueue(choosenIndex);
                if (lastIndexes.Count > 2)
                    lastIndexes.Dequeue();

                return;
            }

            tries++;
        }

        // If no valid weapon exists, choose any weapon except the last two.
        var fallbackGuns = guns
            .Select((g, idx) => new { gun = g, idx })
            .Where(x => !forbiddenIndexes.Contains(x.idx))
            .ToArray();

        if (fallbackGuns.Length > 0)
        {
            var chosen = fallbackGuns[Random.Range(0, fallbackGuns.Length)];
            choosenIndex = chosen.idx;
            lastIndexes.Enqueue(choosenIndex);
            if (lastIndexes.Count > 2)
                lastIndexes.Dequeue();
        }
        else
        {
            // If only the last two remain, allow any weapon.
            choosenIndex = Random.Range(0, guns.Length);
            lastIndexes.Enqueue(choosenIndex);
            if (lastIndexes.Count > 2)
                lastIndexes.Dequeue();
        }
    }

    private void WeaponReady()
    {
        currentState = BuyRandomGunState.weaponReady;
        Invoke(nameof(DisappearGun), timeToDisappear);
        Invoke(nameof(StartBlinking), timeToDisappear * 0.5f);

    }

    /// <summary>
    /// Wrapper for Invoke.
    /// </summary>
    private void StartBlinking()
    {
        StartCoroutine(itemRoulette.BlinkSpriteRenderer(timeToDisappear * 0.5f));
    }

    #region setText
    private string SetWaitingText()
    {
        string statusText = "";
        switch (languageData.currentLanguage)
        {
            case Language.english:
                statusText = ColorHelper.FormatTextWithColor("Waiting", "FFC100");
                break;
            case Language.chinese:
                statusText = ColorHelper.FormatTextWithColor("等待", "FFC100");
                break;
            case Language.portuguese:
                statusText = ColorHelper.FormatTextWithColor("Aguardando", "FFC100");
                break;
            case Language.spanish:
                statusText = ColorHelper.FormatTextWithColor("Esperando", "FFC100");
                break;
            default:
                break;
        }

        return statusText;
    }

    private string SetGetWeaponText()
    {
        string statusText = "";
        string replacing = "";
        switch (languageData.currentLanguage)
        {
            case Language.english:
                if (gunInventory.IsInventoryFull()) replacing = ColorHelper.FormatTextWithColor("REPLACING", "FFC100") + " \n";
                statusText = ColorHelper.FormatTextWithColor(replacing + interactionButton + " Get Weapon", "ffffff");
                break;
            case Language.chinese:
                if (gunInventory.IsInventoryFull()) replacing = ColorHelper.FormatTextWithColor("替换", "FFC100") + " \n";
                statusText = ColorHelper.FormatTextWithColor(replacing + interactionButton + " 拿武器", "ffffff");
                break;
            case Language.portuguese:
                if (gunInventory.IsInventoryFull()) replacing = ColorHelper.FormatTextWithColor("Substituindo", "FFC100") + " \n";
                statusText = ColorHelper.FormatTextWithColor(replacing + interactionButton + " Pegar arma", "ffffff");
                break;
            case Language.spanish:
                if (gunInventory.IsInventoryFull()) replacing = ColorHelper.FormatTextWithColor("Reemplazando", "FFC100") + " \n";
                statusText = ColorHelper.FormatTextWithColor(replacing + interactionButton + " Obtener arma", "ffffff");
                break;
            default:
                break;
        }

        return statusText;
    }

    private string BuyRandomWeaponText()
    {
        string coloredPrice;
        if (pointManager.currentPoints >= GetPrice())
        {
            coloredPrice = UiColors.green_ + GetPrice() + UiColors.endColor_;
        }
        else
        {
            coloredPrice = UiColors.red_ + GetPrice() + UiColors.endColor_;
        }

        string replacing = "";
        switch (languageData.currentLanguage)
        {
            case Language.english:
                //if (gunInventory.IsInventoryFull()) replacing = ColorHelper.FormatTextWithColor("REPLACING", "FFC100") + " \n";
                return replacing + interactionButton + " Buy Random Weapon" + " (Cost: " + coloredPrice + ")";

            case Language.chinese:
                //if (gunInventory.IsInventoryFull()) replacing = ColorHelper.FormatTextWithColor("替换", "FFC100") + " \n";
                return replacing + interactionButton + " 购买随机武器 " + " (消耗: " + coloredPrice + ")";

            case Language.portuguese:
                //if (gunInventory.IsInventoryFull()) replacing = ColorHelper.FormatTextWithColor("Substituindo", "FFC100") + " \n";
                return replacing + interactionButton + " Comprar arma aleatória" + " (preço: " + coloredPrice + ")";

            case Language.spanish:
                //if (gunInventory.IsInventoryFull()) replacing = ColorHelper.FormatTextWithColor("Reemplazando", "FFC100") + " \n";
                return replacing + interactionButton + " Comprar arma aleatoria" + " (precio: " + coloredPrice + ")";
            default:

                break;
        }

        return "";

    }
    #endregion

    public int GetPrice()
    {
        return (int)(price * globalStats.globalPriceMultiplier);
    }
    


    #if UNITY_EDITOR
public void SimulateRandomDraws(int draws = 100)
{
    Dictionary<Rarity, int> counts = new Dictionary<Rarity, int>
    {
        { Rarity.Common, 0 },
        { Rarity.Uncommon, 0 },
        { Rarity.Rare, 0 },
        { Rarity.Epic, 0 },
        { Rarity.Legendary, 0 }
    };

    // Clear history so it does not influence the test.
    lastIndexes.Clear();

    for (int i = 0; i < draws; i++)
    {
        RandomizeGun();
        var rarity = guns[choosenIndex].rarity;
        counts[rarity]++;
    }

    Debug.Log($"Simulação de {draws} sorteios:");
    foreach (var kv in counts)
    {
        Debug.Log($"{kv.Key}: {kv.Value}");
    }
}


    public void PrintGunsByRarity()
    {
        if (guns == null || guns.Length == 0)
        {
            Debug.Log("Nenhuma arma cadastrada.");
            return;
        }

        var rarityGroups = guns.GroupBy(g => g.rarity)
            .OrderBy(g => g.Key);

        foreach (var group in rarityGroups)
        {
            Debug.Log($"Rarity: {group.Key} - Count: {group.Count()}");
            foreach (var gun in group)
            {
                Debug.Log($"    {gun.name}");
            }
        }
    }
#endif
}

     public enum BuyRandomGunState { waiting, raffling, weaponReady };