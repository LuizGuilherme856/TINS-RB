using UnityEngine;
using UnityEngine.UI;

public class HandleGamePadCursor : MonoBehaviour
{
    public OnButtonSelectFeedbacks onButtonSelectFeedbacks;
    public Button buyButton, removeButton;
    public GameObject description;
    public GameObject buyButtonIcon, removeButtonIcon;

    private SkillTreeNode skillTreeNode;

    private void Awake()
    {
        skillTreeNode = GetComponent<SkillTreeNode>();
    }

    public void OnGamePadCursorHover()
    {
        onButtonSelectFeedbacks.HandlePointerEnter();
        buyButtonIcon.SetActive(true);
        removeButtonIcon.SetActive(true);
    }

    public void OnGamePadCursorExit()
    {
        buyButtonIcon.SetActive(false);

        description.SetActive(false);
        //Possibly redundant; verify before removing.
        onButtonSelectFeedbacks.HandlePointerExit();
        buyButton.gameObject.SetActive(false);
        removeButton.gameObject.SetActive(false);
        removeButtonIcon.SetActive(false);
    }

    /// <summary>
    /// Return the purchase or removal button for the node state.
    /// </summary>
    public Button GetRelevantButton()
    {
        if(skillTreeNode.isActivated)
        {
            return removeButton;
        }
        else
        {
            return buyButton;

        }
    }
}
