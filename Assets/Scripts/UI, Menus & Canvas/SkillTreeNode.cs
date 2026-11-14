using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Feedbacks;

public class SkillTreeNode : MonoBehaviour
{
    [SerializeField] private SkillUpgrade skillUpgradeData;
    [Space(10)]

    [SerializeField] private List<SkillTreeNode> previousNodes;

    [SerializeField] private List<GameObject> nodesConnections;

    [SerializeField] private List<SkillTreeNode> groupNodes;

    [Space(25)]
    [SerializeField] private int skillPointCost = 1;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private SkillTreeManager skillTreeManager;
    [SerializeField] private GameObject buyButton;
    [SerializeField] private GameObject removeButton;

    [Space(25)]
    [Header("Image/Icons")]
    [SerializeField] private Image IconImage;
    [SerializeField] private Sprite deactivatedSprite, ActivatedSprite;
    [SerializeField] private Image buttonImage;

    [Space(20)]
    [Header("Feedbacks")]
    [SerializeField] private MMF_Player buyFeedback, cantBuyFeedback, removeFeedback;

    [Space(10)]
    [SerializeField] private bool isFirstNode = false;

    public bool isActivated = false;

    public bool subNode = false;
    [Space(20)]
    [Header("Debug")]
    public GameObject availableIcon_Debug, canRemoveIcon_debug;

    private void Awake()
    {
        if (skillTreeManager == null)
        {
            skillTreeManager = FindFirstObjectByType<SkillTreeManager>();
        }

        var icon = skillUpgradeData.upgradeData.icon;

        if (subNode == false)
        {
            if (icon != null)
            {
                IconImage.sprite = icon;
            }
            else
            {
                Debug.LogError($"O icone para a skill {skillUpgradeData.ID} esta nulo no upgradeData");
            }
        }
    }

    private void OnEnable()
    {
        buyButton.SetActive(false);
        CheckAutoEnable();
        Invoke(nameof(CheckAutoEnable), 0.1f);
    }

    private void CheckAutoEnable()
    {
        foreach (var skillUpgradeItem in playerData.skillUpgrades)
        {
            if (skillUpgradeItem.ID == skillUpgradeData.ID)
            {
                ActivateNode();
                break;
            }
        }
    }

    private void DebugCheck()
    {
        if (CanBuyNode() && isActivated == false)
        {
            availableIcon_Debug.SetActive(true);
        }
        else
        {
            availableIcon_Debug.SetActive(false);
        }

        if (CanRemoveNode() && isActivated)
        {
            canRemoveIcon_debug.SetActive(true);
        }
        else
        {
            canRemoveIcon_debug.SetActive(false);
        }
    }

    public void TryToBuyNode()
    {
        if (isActivated)
        {
            if (CanRemoveNode())
            {
                RemoveNode();
            }
            else
            {
                cantBuyFeedback.PlayFeedbacks();
            }
        }
        else if (CanBuyNode())
        {
            BuyNode();
        }
        else
        {
            Debug.Log("Nao foi possivel ativar o nodo!");
        }
    }

    public void ShowBuyButton()
    {
        if (isActivated == false)
        {
            buyButton.SetActive(true);
            removeButton.SetActive(false);
        }
        else
        {
            buyButton.SetActive(false);
            removeButton.SetActive(true);
        }
    }

    private bool CanBuyNode()
    {
        if (isFirstNode && playerData.GetSkillPoints() >= skillPointCost) return true;

        if (groupNodes.Count > 0)
        {
            foreach (var node in groupNodes)
            {
                if (node.isActivated)
                {
                    cantBuyFeedback.PlayFeedbacks();
                    return false;
                }
            }
        }

        if (isActivated)
        {
            cantBuyFeedback.PlayFeedbacks();
            return false;
        }

        if (!IsAnyPreviousNodeActivated())
        {
            cantBuyFeedback.PlayFeedbacks();
            return false;
        }

        if (playerData.GetSkillPoints() < skillPointCost)
        {
            cantBuyFeedback.PlayFeedbacks();
            return false;
        }

        return true;
    }

    private bool CanRemoveNode()
    {
        if (!isActivated || skillTreeManager == null)
        {
            return false;
        }

        foreach (var node in skillTreeManager.GetNodes())
        {
            if (node == null || node == this || !node.isActivated)
            {
                continue;
            }

            if (!node.WouldRemainSupportedIfNodeRemoved(this, new HashSet<SkillTreeNode>()))
            {
                return false;
            }
        }

        return true;
    }

    public void RemoveNode(bool force = false)
    {
        if (CanRemoveNode() || force)
        {
            DeactivateNode();
            removeFeedback.PlayFeedbacks();
            playerData.AddSkillPoints(skillPointCost);
            skillTreeManager.RemoveSkill(skillUpgradeData);
        }
        else
        {
            Debug.Log("Nao foi possivel remover o nodo!");
        }
    }

    private void DeactivateNode()
    {
        isActivated = false;
        buttonImage.sprite = deactivatedSprite;
        IconImage.color = Color.gray;

        buyButton.SetActive(false);
        removeButton.SetActive(false);

        Debug.Log("Nodo desativado com sucesso!");

        for (int i = 0; i < nodesConnections.Count; i++)
        {
            nodesConnections[i].GetComponent<SkillTreeNodeConection>().StopFilling();
        }
    }

    private bool IsAnyPreviousNodeActivated()
    {
        if (previousNodes.Count == 0)
        {
            return true;
        }

        foreach (var node in previousNodes)
        {
            if (node.isActivated)
            {
                return true;
            }
        }
        return false;
    }

    private bool WouldRemainSupportedIfNodeRemoved(SkillTreeNode nodeToRemove, HashSet<SkillTreeNode> visiting)
    {
        if (this == nodeToRemove || !isActivated)
        {
            return false;
        }

        if (isFirstNode || previousNodes.Count == 0)
        {
            return true;
        }

        if (!visiting.Add(this))
        {
            return false;
        }

        foreach (var previousNode in previousNodes)
        {
            if (previousNode == null || previousNode == nodeToRemove || !previousNode.isActivated)
            {
                continue;
            }

            if (previousNode.WouldRemainSupportedIfNodeRemoved(nodeToRemove, visiting))
            {
                visiting.Remove(this);
                return true;
            }
        }

        visiting.Remove(this);
        return false;
    }

    private void BuyNode()
    {
        ActivateNode();
        buyFeedback.PlayFeedbacks();
        playerData.RemoveSkillPoints(skillPointCost);
        skillTreeManager.AddSkill(skillUpgradeData);
    }

    private void ActivateNode()
    {
        buyButton.SetActive(false);
        removeButton.SetActive(false);
        isActivated = true;
        buttonImage.sprite = ActivatedSprite;
        IconImage.color = Color.white;

        Debug.Log("Nodo ativado com sucesso!");

        for (int i = 0; i < nodesConnections.Count; i++)
        {
            nodesConnections[i].GetComponent<SkillTreeNodeConection>().StartFilling();
        }
    }

    public SkillUpgrade GetUpgradeData()
    {
        return skillUpgradeData;
    }

    [ContextMenu(nameof(UpdateId))]
    public void UpdateId()
    {
        skillUpgradeData.ID = transform.parent.parent.name + transform.parent.name + name;
    }
}
