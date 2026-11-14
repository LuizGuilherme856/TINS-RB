using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Show the skill-tree purchase or removal button for the node's state.
/// </summary>
public class CheckBuyOrRemove : MonoBehaviour
{
    public SkillTreeNode skillTreeNode;
    [SerializeField] GameObject buyButton, removeButton;

    private void OnEnable()
    {
        CheckButton();
    }

    private void CheckButton()
    {
        if (gameObject.activeSelf == false) return;

        if (skillTreeNode.isActivated && removeButton.activeSelf == false)
        {
            removeButton.SetActive(true);
            buyButton.SetActive(false);
        }
        else if(skillTreeNode.isActivated == false && buyButton.activeSelf == false)
        {
            removeButton.SetActive(false);
            buyButton.SetActive(true);
        }
    }

    private void Update()
    {
        CheckButton();
    }
}
