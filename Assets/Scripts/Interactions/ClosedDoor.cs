///Doors and obstacles purchased with points.
using UnityEngine;
using System;
using System.Linq;
using TechBooh;

[RequireComponent(typeof(ScanPathFinder))]
public class ClosedDoor : InteractWithPlayer
{
    [SerializeField] protected int price = 300;
    [SerializeField] protected int opensRegion = 1; //index of the region this door unlocks 

    [SerializeField] protected bool startsOpened = false;

    [SerializeField] protected ColliderType colliderType = ColliderType.box;

    [SerializeField] protected ObstacleType obstacleType = ObstacleType.door;

    [SerializeField] protected ClosedDoor anotherDoorToOpen; //Used for paired doors.
    /// <summary>
    /// Automatically raise the door-open event.
    /// </summary>
    [SerializeField] protected bool autoInvokeEvent = true;
    [SerializeField] protected DisableCollider disableCollider = DisableCollider.onBuy;



    public GlobalStats globalStats;
    /// <summary>
    /// Raised when the door physically opens and the player can pass.
    /// </summary>
    public event Action OnOpenDoor;
    /// <summary>
    /// Raised when payment occurs; OnOpenDoor waits until the door is actually open.
    /// With a delay, OnBuyDoor and OnOpenDoor occur at different times.
    /// </summary>
    public event Action OnBuyDoor;

    public bool canOpen = true;

    #region Private Variables

    private ScanPathFinder scanPathFinder;
    private SpawnManager spawnManager;
    //Obstacle colliders physically keep the door closed.
    private BoxCollider2D obstacleColliderBox;
    private PolygonCollider2D obstacleColliderPolygon;
    private Animator animator;

    //enums
    protected enum DisableCollider { onBuy, onEventCall }
    protected enum ColliderType { box, polygon }
    protected enum ObstacleType { door, debris }

    protected bool isOpen = false;

    #endregion

    protected override void Awake()
    {
        base.Awake();

        if (globalStats == null) globalStats = AssetFinder.GetGlobalStats();

        if(GetComponents<ClosedDoor>().Count() > 1)
        {
            Debug.LogError("Atenção! essa porta tem lockedDoor e ClosedDoor scripts. Use apenas um! porta: "+gameObject.name);
        }

        //Find the collider that blocks the player.
        switch (colliderType)
        {
            case ColliderType.box:
                // TechBooh: ignore the interaction trigger and select only
                // The physical passage-blocking collider.
                obstacleColliderBox = GetComponents<BoxCollider2D>().FirstOrDefault(collider => !collider.isTrigger);
                break;
            case ColliderType.polygon:
                //Find the non-trigger collider that keeps the door closed.
                var colliders = Try.GetComponents_Plural<PolygonCollider2D>(gameObject);
                obstacleColliderPolygon = colliders.Where(x => x.isTrigger == false).FirstOrDefault();
                break;
        }

        if (spawnManager == null) spawnManager = Try.FindObjectOfType<SpawnManager>();

        animator = Try.GetComponent<Animator>(gameObject);

        scanPathFinder = Try.GetComponent<ScanPathFinder>(gameObject);
        if (startsOpened) OpenDoor();

        ValidateObstacleCollider_TechBooh();
    }

    private void Start()
    {
        if (generalTextBoxUI == null)
        {
            Debug.LogWarning("ClosedDoor nao encontrou generalTextBoxUI em " + gameObject.name);
        }
    }

    protected override void StartInteraction()
    {
        if (isOpen) return; //its already open so you cant open it again
                            //generalTextBoxUI.SetText("Press " + buttons.interaction + " To "+openText+" for " + GetPrice());

        base.StartInteraction();
        if (generalTextBoxUI == null)
        {
            Debug.LogWarning("ClosedDoor nao pode exibir o texto porque GeneralTextBoxUI esta ausente em " + gameObject.name, this);
            return;
        }

        int currentPrice = GetPrice();
        int currentPoints = pointManager != null ? pointManager.currentPoints : 0;
        string priceColored;
        //Show the price in green when affordable.
        if (currentPoints >= currentPrice)
        {
            priceColored = UiColors.green_ + currentPrice + UiColors.endColor_;
        }
        else
        {
            priceColored = UiColors.red_ + currentPrice + UiColors.endColor_;
        }

        Language currentLanguage = languageData != null ? languageData.currentLanguage : Language.english;
        if (obstacleType == ObstacleType.door)
        {
            switch (currentLanguage)
            {
                case Language.english:
                    generalTextBoxUI.SetText(interactionButton + " Open " + " (Cost: " + priceColored + ")");
                    break;
                case Language.chinese:
                    generalTextBoxUI.SetText(interactionButton + " 开启 " + "(消耗: " + priceColored + ")");
                    break;
                case Language.portuguese:
                    generalTextBoxUI.SetText(interactionButton + " Abrir " + "(Preço: " + priceColored + ")");
                    break;
                case Language.spanish:
                    generalTextBoxUI.SetText(interactionButton + " Abrir " + "(Precio: " + priceColored + ")");
                    break;
                default:
                    break;
            }
        }
        else //clear path 
        {
            switch (currentLanguage)
            {
                case Language.english:
                    generalTextBoxUI.SetText(interactionButton + " Clear path " + "(Cost: " + priceColored + ")");
                    break;
                case Language.chinese:
                    generalTextBoxUI.SetText(interactionButton + " 清除道路 " + "(消耗: " + priceColored + ")");
                    break;
                case Language.portuguese:
                    generalTextBoxUI.SetText(interactionButton + " Limpar Caminho " + "(Preço: " + priceColored + ")");

                    break;
                case Language.spanish:
                    //Clear the path; cost: 500.
                    generalTextBoxUI.SetText(interactionButton + " despejar el camino " + "(Precio: " + priceColored + ")");
                    break;
                default:
                    break;
            }

        }

    }

    protected override void InteractionAction()
    {
        if (isOpen) return; //its already open so you cant open it again

        if (pointManager == null)
        {
            pointManager = Try.FindObjectOfType<PointManager>();
            if (pointManager == null)
            {
                Debug.LogError("ClosedDoor nao pode abrir porque PointManager nao foi encontrado em " + gameObject.name, this);
                return;
            }
        }

        int currentPrice = GetPrice();
        if (pointManager.currentPoints >= currentPrice && canOpen)
        {
            pointManager.RemovePoints(currentPrice);

            // TechBooh: once points are charged, opening the door is essential.
            // Optional feedbacks and listeners must not interrupt that operation.
            OpenDoor();
            SafePlayFeedback_TechBooh(buyFeedback, "buyFeedback");

            SafeInvokeBuyDoorEvent_TechBooh(); //Raise the purchase event; the door may not be open yet.
        }
        else
        {
            if (pointManager.currentPoints < currentPrice)
            {
                try
                {
                    notEnoughPointsFeedbacks?.TriggerNotEnoughPointsFeedbacks();
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("ClosedDoor falhou ao tocar feedback de pontos insuficientes em " + gameObject.name + ". Erro: " + exception.Message, this);
                }

            }
            else
            {
                SafePlayFeedback_TechBooh(cantBuyFeedback, "cantBuyFeedback");
            }


        }
    }

    public void OpenDoor(bool scan = true)
    {
        if (isOpen) return; //Prevent repeated calls.

        // TechBooh:
        // Ensure the door opens physically even if an auxiliary
        // Animator, path scan, stats component, or event listener fails.
        // Mark it open early and handle the remaining work on a best-effort basis.
        isOpen = true;

        if (disableCollider == DisableCollider.onBuy)

        {
            SafeDisableColliderMethod_TechBooh();
        }
        else
        {
            OnOpenDoor += SafeDisableColliderMethod_TechBooh;

        }

        SafeSetRegionActive_TechBooh();
        SafeScanPathFinder_TechBooh(scan);
        SafeEndInteraction_TechBooh();
        SafePlayOpenAnimation_TechBooh();

        //Use when opening one door should also open another.
        if (anotherDoorToOpen)
        {
            SafeOpenAnotherDoor_TechBooh();
        }

        //Skill that refills ammunition when opening a door.
        SafeRefillAmmoOnOpenDoor_TechBooh();


        if (autoInvokeEvent)
        {
            SafeInvokeOpenDoorEvent_TechBooh();
        }
    }

    private void DisableColliderMethod()
    {
        ResolveObstacleCollider_TechBooh();

        //Disable the blocking collider to open the passage.
        switch (colliderType)
        {
            case ColliderType.box:
                foreach (BoxCollider2D collider in GetComponents<BoxCollider2D>().Where(collider => !collider.isTrigger))
                {
                    collider.enabled = false;
                }
                break;
            case ColliderType.polygon:
                foreach (PolygonCollider2D collider in GetComponents<PolygonCollider2D>().Where(collider => !collider.isTrigger))
                {
                    collider.enabled = false;
                }
                break;
        }
    }

    protected void SafeDisableColliderMethod_TechBooh()
    {
        try
        {
            DisableColliderMethod();
        }
        catch (Exception exception)
        {
            Debug.LogWarning("ClosedDoor falhou ao desabilitar collider em " + gameObject.name + ". A porta continuara tentando abrir. Erro: " + exception.Message, this);
        }
    }

    protected void SafeSetRegionActive_TechBooh()
    {
        try
        {
            if (spawnManager != null)
            {
                spawnManager.SetRegionActive(opensRegion);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("ClosedDoor falhou ao ativar regiao em " + gameObject.name + ". Erro: " + exception.Message, this);
        }
    }

    protected void SafeScanPathFinder_TechBooh(bool scan)
    {
        if (!scan)
        {
            return;
        }

        try
        {
            if (scanPathFinder != null)
            {
                scanPathFinder.AsyncScan();
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("ClosedDoor falhou ao atualizar pathfinding em " + gameObject.name + ". Erro: " + exception.Message, this);
        }
    }

    protected void SafeEndInteraction_TechBooh()
    {
        try
        {
            EndInteraction();
        }
        catch (Exception exception)
        {
            Debug.LogWarning("ClosedDoor falhou ao encerrar interacao em " + gameObject.name + ". Erro: " + exception.Message, this);
        }
    }

    protected void SafePlayOpenAnimation_TechBooh()
    {
        try
        {
            if (animator != null)
            {
                animator.SetTrigger("Open");
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("ClosedDoor falhou ao tocar animacao de abrir em " + gameObject.name + ". Erro: " + exception.Message, this);
        }
    }

    protected void SafeOpenAnotherDoor_TechBooh()
    {
        try
        {
            if (anotherDoorToOpen != null)
            {
                anotherDoorToOpen.OpenDoor(false);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("ClosedDoor falhou ao abrir porta secundaria em " + gameObject.name + ". Erro: " + exception.Message, this);
        }
    }

    protected void SafeRefillAmmoOnOpenDoor_TechBooh()
    {
        try
        {
            if (globalStats != null && globalStats.ForCurrentRun.RefilAmmoOnOpenDoor && player != null)
            {
                GunInventory gunInventory = player.GetComponent<GunInventory>();
                if (gunInventory != null)
                {
                    gunInventory.RefillAmmoCurrentGun(true);
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("ClosedDoor falhou ao recarregar municao bonus em " + gameObject.name + ". Erro: " + exception.Message, this);
        }
    }

    protected void SafeInvokeOpenDoorEvent_TechBooh()
    {
        if (OnOpenDoor == null) return;

        // TechBooh: invoke listeners independently so a failing callback cannot
        // prevent later callbacks, especially the one removing the blocking collider.
        foreach (Action callback in OnOpenDoor.GetInvocationList())
        {
            try
            {
                callback.Invoke();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("ClosedDoor falhou ao disparar um listener de OnOpenDoor em " + gameObject.name + ". Erro: " + exception.Message, this);
            }
        }
    }

    private void ResolveObstacleCollider_TechBooh()
    {
        if (colliderType == ColliderType.box && obstacleColliderBox == null)
        {
            obstacleColliderBox = GetComponents<BoxCollider2D>().FirstOrDefault(collider => !collider.isTrigger);
        }
        else if (colliderType == ColliderType.polygon && obstacleColliderPolygon == null)
        {
            obstacleColliderPolygon = GetComponents<PolygonCollider2D>().FirstOrDefault(collider => !collider.isTrigger);
        }
    }

    private void ValidateObstacleCollider_TechBooh()
    {
        ResolveObstacleCollider_TechBooh();
        bool hasCollider = colliderType == ColliderType.box
            ? obstacleColliderBox != null
            : obstacleColliderPolygon != null;

        if (!hasCollider)
        {
            Debug.LogError("ClosedDoor nao encontrou um " + colliderType + " collider nao-trigger em " + gameObject.name + ". A animacao abrira, mas o obstaculo fisico nao podera ser removido.", this);
        }
    }

    private void SafePlayFeedback_TechBooh(MoreMountains.Feedbacks.MMF_Player feedback, string feedbackName)
    {
        try
        {
            feedback?.PlayFeedbacks();
        }
        catch (Exception exception)
        {
            Debug.LogWarning("ClosedDoor falhou ao tocar " + feedbackName + " em " + gameObject.name + ". Erro: " + exception.Message, this);
        }
    }

    private void SafeInvokeBuyDoorEvent_TechBooh()
    {
        if (OnBuyDoor == null) return;

        foreach (Action callback in OnBuyDoor.GetInvocationList())
        {
            try
            {
                callback.Invoke();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("ClosedDoor falhou ao disparar um listener de OnBuyDoor em " + gameObject.name + ". Erro: " + exception.Message, this);
            }
        }
    }

    public void CloseDoor() //maybe it'll be useful for a door that closes automatically 
    {
        isOpen = false;
    }

    public bool GetIsOpen()
    {
        return isOpen;
    }

    public void InvokeEvent()
    {
        SafeInvokeOpenDoorEvent_TechBooh();
    }



    public int GetPrice()
    {
        if (globalStats == null)
        {
            globalStats = AssetFinder.GetGlobalStats();
        }

        if (globalStats == null)
        {
            Debug.LogWarning("ClosedDoor nao encontrou GlobalStats em " + gameObject.name + ". Usando o preco base.", this);
            return price;
        }

        return (int)(price * globalStats.ForCurrentRun.doorPriceMultiplier * globalStats.ForCurrentRun.globalPriceMultiplier);
    }

}
