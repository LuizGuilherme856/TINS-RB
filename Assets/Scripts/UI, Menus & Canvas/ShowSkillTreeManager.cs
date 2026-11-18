using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowSkillTreeManager : MonoBehaviour
{
    [SerializeField] private GameObject skillTreeUI;
    [SerializeField] private PlayerHolder playerHolder;
    [SerializeField] private PauseMenuManager pauseMenuManager;

    private PlayerInputHandler playerInputHandler;
    private SaveGame saveGame;

    public Transform SkillTreeRoot => skillTreeUI != null ? skillTreeUI.transform : null;
    
    private void Awake()
    {
        saveGame = GetComponent<SaveGame>();
        playerInputHandler = playerHolder.GetPlayerInputHandler();
    }

    private void OnEnable()
    {
        playerInputHandler.OnEscape += HideSkillTree;
    }

    public void ShowSkillTree()
    {
        var campaign = TINS.Campaign.CampaignBaseController.Active;
        if (campaign != null && campaign.IsReady)
        {
            campaign.ShowSkillTree();
            return;
        }

        playerInputHandler.SetCanMove(false);
        playerInputHandler.SetAllowPauseInputWhenCantMove(true);
        skillTreeUI.SetActive(true);

        Debug.Log("show skill tree");
    }

    public void HideSkillTree()
    {
        if(skillTreeUI.activeSelf)
        {
            pauseMenuManager.ClosePauseMenu(); //Prevent the Escape used to close the skill tree from also opening pause.
            saveGame.SaveGameToDisk();
            playerInputHandler.SetAllowPauseInputWhenCantMove(false);
            playerInputHandler.SetCanMove(true);
            skillTreeUI.SetActive(false);
        }
    }

    public bool IsSkillTreeOpen()
    {
        return skillTreeUI != null && skillTreeUI.activeSelf;
    }

}
