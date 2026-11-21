using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TechBooh;
using UnityEngine;
using UnityEngine.UIElements;

public class InteractWithPlayer : MonoBehaviour
{
    protected GameObject player;
    protected PlayerInputHandler playerInputHandler;

    protected bool isTouchingPlayer = false;
    protected readonly HashSet<Collider2D> touchingPlayerColliders_TechBooh = new();
    private float timeSinceLastInteraction = 0, minTimeBetweenInteractions = 0.3f;

    [SerializeField] protected ButtonsData buttons;
    [SerializeField] protected LanguageData languageData;
    [SerializeField] protected MMF_Player buyFeedback, cantBuyFeedback;
    [SerializeField] protected bool changeShaderOnInteraction = false;
    [SerializeField] protected SpriteRenderer stationSpriteRenderer;


    //private GameObject playerGameObject;
    protected GunInventory gunInventory;

    [SerializeField] protected GeneralTextBoxUI generalTextBoxUI;
    protected PointManager pointManager;

    protected string interactionButton;

    protected NotEnoughPointsFeedbacks notEnoughPointsFeedbacks;
    /// <summary>
    /// Number of successful interactions initiated by the player.
    /// </summary>
    protected int interactionsActionsCount = 0;

    protected virtual void Awake()
    {
        try
        {
            CheckScriptableObjects();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao checar ScriptableObjects: {e.Message}");
        }

        try
        {
            //De Niro's ability can create a second instance in the scene.
            var inputHandlers = FindObjectsByType<PlayerInputHandler>(FindObjectsSortMode.None);
            playerInputHandler = inputHandlers.Where(x => x.GetIsClone() == false).FirstOrDefault();

            if (playerInputHandler == null)
            {
                Debug.LogError("Nao conseguiu pegar o player input Handler");
            }
            else
            {
                try
                {
                    BindPlayerInputHandler_TechBooh(playerInputHandler);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Erro ao atualizar botão de interação: {e.Message}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao buscar PlayerInputHandler: {e.Message}");
        }

        try
        {
            gunInventory = Try.FindObjectOfType<GunInventory>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao buscar GunInventory: {e.Message}");
        }

        try
        {
            generalTextBoxUI = Try.FindObjectOfType<GeneralTextBoxUI>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao buscar GeneralTextBoxUI: {e.Message}");
        }

        try
        {
            pointManager = Try.FindObjectOfType<PointManager>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao buscar PointManager: {e.Message}");
        }

        notEnoughPointsFeedbacks = FindFirstObjectByType<NotEnoughPointsFeedbacks>();

    }

    private void CheckScriptableObjects()
    {
        if (languageData == null)
        {
            Debug.Log($"Language Data esta nulo em {gameObject.name}. Procurando pelo assetFinder");
            languageData = AssetFinder.GetLanguageData();
        }
        if (buttons == null)
        {
            Debug.Log($"Buttons Data esta nulo em {gameObject.name}. Procurando pelo assetFinder");
            buttons = AssetFinder.GetButtonsData();
        }
    }

    private void OnEnable()
    {
        try
        {
            CheckScriptableObjects();
        }
        catch (System.Exception exception)
        {
            Debug.LogError("InteractWithPlayer falhou ao recuperar os dados em " + gameObject.name + ". Erro: " + exception.Message, this);
        }

        if (generalTextBoxUI == null)
        {
            GameObject interactionTextBox = GameObject.FindGameObjectWithTag("UI/InteractionTextBox");
            if (interactionTextBox != null)
            {
                generalTextBoxUI = interactionTextBox.GetComponent<GeneralTextBoxUI>();
            }

            if ( generalTextBoxUI == null )
            {
                Debug.LogError("GeneralTextBoxUI não encontrado no OnEnable de " + gameObject.name);
            }
        }

        // TechBooh: script recompilation in Play Mode triggers OnDisable/OnEnable.
        // Restore subscriptions so interaction and A/F prompts remain current.
        BindPlayerInputHandler_TechBooh(FindMainPlayerInputHandler_TechBooh());

        if (stationSpriteRenderer == null && changeShaderOnInteraction)
        {
            stationSpriteRenderer = GetComponent<SpriteRenderer>();
            if (stationSpriteRenderer != null)
            {
                stationSpriteRenderer.material.SetFloat("_InnerOutlineFade", 0);
            }
            else
            {
                Debug.LogWarning("InteractWithPlayer nao encontrou SpriteRenderer para o outline em " + gameObject.name, this);
            }
        }

        if (languageData != null)
        {
            languageData.OnLanguageChange += RefreshInteractionText;
        }
    }

    private void OnDisable()
    {
        if (languageData != null)
        {
            languageData.OnLanguageChange -= RefreshInteractionText;
        }

        if (playerInputHandler != null)
        {
            playerInputHandler.OnControllerChange -= RefreshInteractionText;
            playerInputHandler.OnInteract -= TryToInteract;
        }

        touchingPlayerColliders_TechBooh.Clear();
        isTouchingPlayer = false;
    }


    private void Update()
    {
        if (timeSinceLastInteraction >= 0)
        {
            timeSinceLastInteraction -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsPlayerCollision(collision))
        {
            PlayerInputHandler contactInputHandler_TechBooh = collision.GetComponentInParent<PlayerInputHandler>();
            if (contactInputHandler_TechBooh != null && !contactInputHandler_TechBooh.GetIsClone())
            {
                BindPlayerInputHandler_TechBooh(contactInputHandler_TechBooh);
            }
            else
            {
                BindPlayerInputHandler_TechBooh(playerInputHandler);
            }

            bool wasTouchingPlayer = touchingPlayerColliders_TechBooh.Count > 0;
            touchingPlayerColliders_TechBooh.Add(collision);

            if (player == null)
            {
                try
                {
                    PlayerInputHandler collisionPlayerInput = collision.GetComponentInParent<PlayerInputHandler>();
                    if (collisionPlayerInput != null)
                    {
                        player = collisionPlayerInput.gameObject;
                    }
                    else
                    {
                        player = collision.gameObject.transform.parent.gameObject; //Find the parent because collision occurs on the obstacle-hit child, not the player root.
                    }
                }
                catch (System.Exception e) //Workaround for interaction loss when Hanba's intangibility changes layers.
                {
                    Debug.Log("nao conseguiu pegar o gameObject do player parent" + e);

                    player = Object.FindFirstObjectByType<PlayerInputHandler>().gameObject;
                }
            }

            isTouchingPlayer = true;

            // TechBooh:
            // Multiple player colliders can overlap an interaction area.
            // Begin interaction only when transitioning
            // from no player colliders inside to at least one inside.
            if (!wasTouchingPlayer)
            {
                if (playerInputHandler == null)
                {
                    Debug.LogError("Nao foi possivel iniciar a interacao porque o PlayerInputHandler nao foi encontrado em " + gameObject.name, this);
                    return;
                }

                playerInputHandler.OnInteract -= TryToInteract;
                playerInputHandler.OnInteract += TryToInteract;
                UpdateInteractionButton();
                StartInteraction();
            }
        }

    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // TechBooh: recover contact after recompilation or a temporary layer change
        // without requiring the player to leave and re-enter the door area.
        if (IsPlayerCollision(collision) && !touchingPlayerColliders_TechBooh.Contains(collision))
        {
            OnTriggerEnter2D(collision);
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (IsPlayerCollision(collision))
        {
            touchingPlayerColliders_TechBooh.Remove(collision);

            if (touchingPlayerColliders_TechBooh.Count <= 0)
            {
                touchingPlayerColliders_TechBooh.Clear();
                if (playerInputHandler != null)
                {
                    playerInputHandler.OnInteract -= TryToInteract;
                }
                isTouchingPlayer = false;

                EndInteraction();
            }
        }
    }

    private void TryToInteract()
    {
        if (Time.timeScale == 0)
        {
            Debug.LogWarning("Tentou interagir com a timescale em zero");
            return;
        }

        if (isTouchingPlayer && timeSinceLastInteraction <= 0)
        {
            InteractionAction();
            timeSinceLastInteraction = minTimeBetweenInteractions;
        }
    }

    private void UpdateInteractionButton()
    {
        InputPromptDeviceType deviceType = playerInputHandler != null
            ? playerInputHandler.CurrentPromptDevice
            : InputPromptDeviceType.KeyboardMouse;

        if (buttons == null)
        {
            string fallbackButton = deviceType switch
            {
                InputPromptDeviceType.KeyboardMouse => "F",
                InputPromptDeviceType.NintendoSwitch => "B",
                _ => "A"
            };
            interactionButton = "[<color=#FF0040>" + fallbackButton + "</color>]";
            return;
        }

        interactionButton = "[<color=#FF0040>" + buttons.GetInteractionButton(deviceType) + "</color>]";
    }

    private void RefreshInteractionText()
    {
        UpdateInteractionButton();

        if (!isTouchingPlayer) return;

        StartInteraction();
    }

    private PlayerInputHandler FindMainPlayerInputHandler_TechBooh()
    {
        var inputHandlers = FindObjectsByType<PlayerInputHandler>(FindObjectsSortMode.None);
        return inputHandlers.FirstOrDefault(inputHandler => inputHandler != null && !inputHandler.GetIsClone());
    }

    private void BindPlayerInputHandler_TechBooh(PlayerInputHandler inputHandler)
    {
        if (inputHandler == null)
        {
            inputHandler = FindMainPlayerInputHandler_TechBooh();
        }

        if (inputHandler == null)
        {
            return;
        }

        if (playerInputHandler != null && playerInputHandler != inputHandler)
        {
            playerInputHandler.OnControllerChange -= RefreshInteractionText;
            playerInputHandler.OnInteract -= TryToInteract;
        }

        playerInputHandler = inputHandler;
        playerInputHandler.OnControllerChange -= RefreshInteractionText;
        playerInputHandler.OnControllerChange += RefreshInteractionText;
        UpdateInteractionButton();
    }

    //this methods are overrided in the classes 
    protected virtual void StartInteraction() //When the player enters the trigger.
    {
        SetOutline(1);
    }

    protected virtual void EndInteraction() //When the player leaves the trigger.
    {
        generalTextBoxUI.RemoveText();
        SetOutline(0);
    }

    protected virtual void InteractionAction() //When the player presses interact inside the trigger.
    {
        interactionsActionsCount++;

    }


    /// <summary>
    /// Highlight outline shown while the player touches the object.
    /// </summary>
    /// <param name="amount">Value between 0 and 1.</param>
    public void SetOutline(float amount)
    {
        if (enabled == false) return;

        if(stationSpriteRenderer == null)
        {
            stationSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        try
        {
            if (changeShaderOnInteraction)
                stationSpriteRenderer.material.SetFloat("_InnerOutlineFade", amount);
        }
        catch (System.Exception)
        {
            Debug.LogError("Erro ao setar o outline do shader no objeto " + gameObject.name + ". Verifique se o sprite renderer tem o material correto.");
        }



    }

    public void ResetText()
    {
        generalTextBoxUI.SetText("");
    }

    public void DisableShaderOnInteraction()
    {
        SetOutline(0);
        changeShaderOnInteraction = false;
    }

    private bool IsPlayerCollision(Collider2D collision)
    {
        if (collision == null) return false;
        if (collision.CompareTag("Player")) return true;
        if (collision.GetComponentInParent<PlayerInputHandler>() != null) return true;

        Transform root = collision.transform.root;
        return root != null && root.CompareTag("Player");
    }


}
