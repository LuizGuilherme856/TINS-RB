///Development tool for upgrading a weapon while recording video.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Development-only behavior: this component has no build guard. Verify scene/prefab activation
// and exclude or guard it explicitly before shipping, even when the main cheat action is disabled.
public class AutoUpgradeGun : MonoBehaviour
{
    private GunData gunData;
    [SerializeField] GunInventory gunInventory;

    //private PlayerInputHandler playerInputHandler;

    private void Awake()
    {
        //playerInputHandler = GetComponent<PlayerInputHandler>();
        gunData = gunInventory.GetCurrentEquipedGun();
        gunData.projectileSize = 1;
        gunData.bulletSpeed = 20f;

    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            gunData = gunInventory.GetCurrentEquipedGun();
            UpgradeGun();
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            gunData = gunInventory.GetCurrentEquipedGun();
            DowngradeGun();
        }
    }
    private void UpgradeGun()
    {
        gunData.projectileSize *= 1.02f;
       // gunData.GetDamage() *= 1.2f;
        gunData.bulletSpeed +=1;
       // gunData.bulletsPerShot += 2;
        Debug.Log("Upgrade");
    }

    private void DowngradeGun()
    {
        gunData.projectileSize *= 0.98f;
      //  gunData.damage *= 0.833f;
        gunData.bulletSpeed -= 1;
      //  gunData.bulletsPerShot -= 2;
        Debug.Log("Downgrade");
    }

}
