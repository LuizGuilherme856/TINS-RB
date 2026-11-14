using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHideMap : MonoBehaviour
{
    [SerializeField] private PlayerHolder playerHolder;
     private PlayerInputHandler playerInput;
    [SerializeField] private GameObject mapGameObject, blackBackground;
    [SerializeField] private PauseMenuManager pauseMenuManager;

    private void Awake()
    {
        playerInput = playerHolder.GetPlayerInputHandler();
        if (playerInput == null) playerInput = FindFirstObjectByType<PlayerInputHandler>();
    }

    private void Start()
    {
        //playerInput.OnMap += ShowMap_Toggle;
        pauseMenuManager.OnPauseMenuIsVisible += HideMap;
        pauseMenuManager.OnInventoryIsVisible += HideMap;

        pauseMenuManager.OnPauseMenuIsNotVisible += HideMap;
        pauseMenuManager.OnInventoryIsNotVisible += HideMap;

        HideMap();
    }

    public void ShowMap_Toggle()
    {
        if (mapGameObject.activeSelf)
        {
            mapGameObject.SetActive(false);
            blackBackground.SetActive(false);
            //pauseMenuManager.ChangeTimeScale(1);
        }
        else
        {
            mapGameObject.SetActive(true);
            if(pauseMenuManager.IsPauseMenuActive())
            {
                blackBackground.SetActive(true);
            }

           // pauseMenuManager.ChangeTimeScale(0);
        }

    }

    private void HideMap()
    {
        mapGameObject.SetActive(false);
        blackBackground.SetActive(false);
    }

    private void ActivateBlackbackground()
    {
         if (mapGameObject.activeSelf) blackBackground.SetActive(true); 
    }

    private void OnDisable()
    {
        //playerInput.OnMap -= ShowMap_Toggle;
        pauseMenuManager.OnPauseMenuIsVisible -= HideMap;
        pauseMenuManager.OnInventoryIsVisible -= HideMap;

        pauseMenuManager.OnPauseMenuIsNotVisible -= HideMap;
        pauseMenuManager.OnInventoryIsNotVisible -= HideMap;
    }

}
