using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnButtonSelectFeedbacks : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private MMF_Player OnSelectFeedbacks, onHoverFeedbacks;
    [Tooltip("So necessario se for trocar a cor do botao")]
    [SerializeField] private Image buttonImage;
    [SerializeField] private PlayerHolder playerHolder;
    [SerializeField] private bool resetOnEnable = false;

    [Header("   On Hover")]
    [SerializeField] private bool changeImageColor = false;
    [SerializeField] private Color32 hoverColor;
    [SerializeField] private bool enableGameObjectOnHover = false;
    [SerializeField] private GameObject hoverGameObject;
    [SerializeField] private float onHoverScaleMultiplier = 1f;

    [Header("   On exitHover")]
    [SerializeField] private bool disableGameObjectOnExitHover = false;
    public float delayToDiselect = 1f;

    [Header("   On Selected")]
    [SerializeField] private bool enableGameObjectOnSelect = false;
    [SerializeField] private List<GameObject> selectGameObjects;

    [Header("   On deselected")]
    [SerializeField] private bool disableGameObjectOnDeselect = false;

    private Vector3 defaultScale;
    private PlayerInputHandler playerInputHandler;

    private void Awake()
    {
        defaultScale = transform.localScale;

        if (playerHolder == null)
        {
            playerHolder = FindFirstObjectByType<PlayerHolder>();
        }

        if (playerHolder != null)
        {
            playerInputHandler = playerHolder.GetPlayerInputHandler();
        }
    }

    private void OnEnable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnControllerChange += HandleControllerChange;
        }

        if (resetOnEnable)
        {
            ResetEverything();
        }
    }

    private void OnDisable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnControllerChange -= HandleControllerChange;
        }

        CancelInvoke();
        ResetEverything();
    }

    public void OnSelect(BaseEventData eventData)
    {
        OnSelectFeedbacks.PlayFeedbacks();
        if (enableGameObjectOnSelect)
        {
            foreach (var obj in selectGameObjects)
            {
                obj.SetActive(true);
            }
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (disableGameObjectOnDeselect)
        {
            DeselectGameObjects();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ShouldIgnorePointerInteraction()) return;
        HandlePointerEnter();
    }

    public void HandlePointerEnter()
    {
        if (onHoverFeedbacks != null)
        {
            onHoverFeedbacks.PlayFeedbacks();
        }

        if (enableGameObjectOnHover)
        {
            hoverGameObject.SetActive(true);
            CancelInvoke();
        }

        if (onHoverScaleMultiplier != 1f)
        {
            transform.localScale = defaultScale * onHoverScaleMultiplier;
        }

        if (changeImageColor)
        {
            buttonImage.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ShouldIgnorePointerInteraction()) return;
        HandlePointerExit();
    }

    public void HandlePointerExit()
    {
        if (onHoverScaleMultiplier != 1f)
        {
            transform.localScale = defaultScale;
        }

        if (enableGameObjectOnHover)
        {
            hoverGameObject.SetActive(false);
        }

        if (changeImageColor)
        {
            buttonImage.color = Color.white;
        }

        if (disableGameObjectOnExitHover)
        {
            Invoke(nameof(DeselectGameObjects), delayToDiselect);
        }
    }

    private void DeselectGameObjects()
    {
        foreach (var obj in selectGameObjects)
        {
            obj.SetActive(false);
        }
    }

    public void ResetEverything()
    {
        transform.localScale = defaultScale;

        if (enableGameObjectOnHover) hoverGameObject.SetActive(false);
        if (enableGameObjectOnSelect)
        {
            foreach (var obj in selectGameObjects)
            {
                obj.SetActive(false);
            }
        }

        if (buttonImage != null)
        {
            buttonImage.color = Color.white;
        }
    }

    private bool ShouldIgnorePointerInteraction()
    {
        return playerInputHandler != null && playerInputHandler.isUsingController;
    }

    private void HandleControllerChange()
    {
        if (!ShouldIgnorePointerInteraction()) return;

        HandlePointerExit();
        if (disableGameObjectOnDeselect)
        {
            DeselectGameObjects();
        }
    }
}
