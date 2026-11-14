using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideGunOnMelee : MonoBehaviour
{
     private HideGunSprite hideGunSprite;

    private void Start()
    {
        hideGunSprite = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<HideGunSprite>();
        if (hideGunSprite)
        {
            Debug.Log("Encontrou hideGunSprite");
        }
    }
    public void HideGun()
    {
        hideGunSprite.EnableGunSprite(false);
    }

    public void ShowGun()
    {
        hideGunSprite.EnableGunSprite(true);
    }
}
