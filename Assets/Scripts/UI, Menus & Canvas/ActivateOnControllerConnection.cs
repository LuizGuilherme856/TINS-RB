using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ActivateOnControllerConnection : MonoBehaviour
{
    public PlayerHolder playerHolder;
    public List<GameObject> objectsToActivate;
    public bool deactivateOnDisconnect = true;

    private PlayerInputHandler playerInputHandler;


    private void Awake()
    {
        playerInputHandler = playerHolder.GetPlayerInputHandler();
    }

    private void OnEnable()
    {
        if (playerInputHandler == null)
        {
            Debug.LogWarning("PlayerInputHandler não está configurado.");
            return;
        }
        playerInputHandler.OnControllerChange += CheckController;

        CheckController();
    }

    private void CheckController()
    {
        if (playerInputHandler == null)
        {
            Debug.LogWarning("PlayerInputHandler não está configurado.");
            return;
        }
        if (playerInputHandler.isUsingController)
        {
            foreach (GameObject obj in objectsToActivate)
            {
                obj.SetActive(true);
            }
        }
        else
        {
            if(deactivateOnDisconnect)
            {
                foreach (GameObject obj in objectsToActivate)
                {
                    obj.SetActive(false);
                }
            }
        }


    }

}
