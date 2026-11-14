using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Toggle the laser sight for controller input.
/// </summary>
public class LaserSight : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private GameObject laserGameObj;
    [SerializeField] private GunInventory gunInventory;

    private void OnEnable()
    {
        ActivateLaser();
        playerInputHandler.OnControllerChange += ActivateLaser;
    }

    private void OnDisable()
    {
        playerInputHandler.OnControllerChange -= ActivateLaser;
    }

    private void ActivateLaser()
    {
        if(playerInputHandler.isUsingController && gunInventory.GetGuns().Count > 0)
        {
            laserGameObj.SetActive(true);
        }
        else
        {
            laserGameObj.SetActive(false);
        }
    }
}
