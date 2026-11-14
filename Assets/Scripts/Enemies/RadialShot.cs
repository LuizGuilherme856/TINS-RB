using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class RadialShot : MonoBehaviour
{
    [SerializeField] private RuneInventory runeUpgradeManager;
    [SerializeField] private GunData gunData;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float aimAngle;
    private MMMiniObjectPooler miniObjectPooler;
    [SerializeField] private Health playerHealth;

    //private void Awake()
    //{
    //    miniObjectPooler = GetComponent<MMMiniObjectPooler>();

    //    InvokeRepeating(nameof(TryToShoot), 2, 2);
    //}

    //private void TryToShoot()
    //{
    //    if(runeUpgradeManager.GetNumberOfRadialShot() > 0 && playerHealth.IsAlive())
    //    {
    //        InstantiateShot();
    //    }
    //}


    //public void InstantiateShot()
    //{
    //        //Regular discrete projectiles.
    //        int numberOfBulets = runeUpgradeManager.GetNumberOfRadialShot();
    //        Projectile[] projectiles = new Projectile[numberOfBulets]; //Return an array because some weapons fire multiple bullets per shot.

    //        for (int i = 0; i < numberOfBulets; i++)
    //        {
    //            projectiles[i] = miniObjectPooler.GetPooledGameObject().GetComponent<Projectile>();
    //            projectiles[i].gameObject.SetActive(true);
    //            //projectiles[i].transform.localScale = transform.localScale * gunData.bulletSize;

    //            projectiles[i].gameObject.GetComponent<BulletStats>().SetGunData(gunData); //Used when a bullet is fired by a weapon other than its original owner.

    //            SetBulletSpeed(projectiles[i], i);
    //            //Refresh bullets after assigning their data; otherwise, the new values take effect only on the next shot.
    //            //Aux.Print("ShootingHandler: " + gunData.GetGunName(Language.english).ToString() + " - "+gunData.bullet.name.ToString(), showDebug);
    //        }
       
    //}

    //private void SetBulletSpeed(Projectile newProjectile, int currentIndex) //Speed, inaccuracy, and angular spread for multiple bullets.
    //{

    //    Vector3 newRotation = Vector3.zero;


    //    newProjectile.SetDirection(1);

    //    float numberOfBullets = runeUpgradeManager.GetNumberOfRadialShot();

    //    float multipleBulletsAngleOffset = 360 / numberOfBullets;

    //    if (numberOfBullets > 1)
    //    {
    //        if (currentIndex == 0)
    //        {
    //            multipleBulletsAngleOffset = 0;
    //            //Do not offset the first bullet's angle.
    //        }
    //        else 
    //        {
    //            multipleBulletsAngleOffset *= currentIndex;
    //        }

    //        newRotation = new Vector3(newRotation.x, newRotation.y, newRotation.z + multipleBulletsAngleOffset);

    //    }
    //    newProjectile.SetRotation(newRotation);

    //    newProjectile.transform.position = firePoint.position;
    //    newProjectile.transform.parent = null;
    //    var speedRandomness = UnityEngine.Random.Range(-gunData.speedRandomness, gunData.speedRandomness);
    //    newProjectile.SetprojectileSpeed(gunData.bulletSpeed + speedRandomness);
    //    newProjectile.RefreshVelocity();
    //}
}
