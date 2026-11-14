using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeTMPTextColorOnSelect : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private TextMeshProUGUI textToChangeColor;
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color notSelectedColor = Color.gray;

    private void Awake()
    {
        if (textToChangeColor == null)
        {
            textToChangeColor = GetComponent<TextMeshProUGUI>();
        }
    }

    private void OnEnable()
    {
        RefreshSelectedState();
    }

    private void OnDisable()
    {
        SetSelectedState(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        SetSelectedState(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        SetSelectedState(false);
    }

    private void RefreshSelectedState()
    {
        bool isSelected = EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;
        SetSelectedState(isSelected);
    }

    private void SetSelectedState(bool isSelected)
    {
        if (textToChangeColor == null)
        {
            return;
        }

        textToChangeColor.color = isSelected ? selectedColor : notSelectedColor;
    }
}
