using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonSingleUseUntilSceneChange : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private UnityEvent onAcceptedClick;

    private bool wasClicked = false;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    public void TryClick()
    {
        if (wasClicked)
            return;

        wasClicked = true;

        if (button != null)
            button.interactable = false;

        onAcceptedClick?.Invoke();
    }
}