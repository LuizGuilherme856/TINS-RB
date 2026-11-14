using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPlayerWithGunData : AttackPlayer
{
    
    [SerializeField] public List<MMMiniObjectPooler> bulletPoolers; //Assigned in the Editor.
    [SerializeField] public List<GunData> gunDatas; //Assigned in the Editor.
   

    protected override void Attack(Collision2D collision)
    {
        //Disable inherited melee damage in this specialized class.
        //base.Attack(collision);
    }

    /// <summary>
    /// Spawn a projectile using the supplied GunData and pool.
    /// </summary>
    public void InstantiateProjectile(int intTargetGun,int intTargetPool)
    {
        GunData targetGunData = gunDatas[intTargetGun];
        MMMiniObjectPooler targetMiniPool = bulletPoolers[intTargetPool];

        if (hasFeedback)
        {
            rangedAttackFeedback_shoot.PlayFeedbacks();
        }

        int _bulletsPerShot;
        float _projectileSpeed, _multipleBulletAngleOffset;


        _multipleBulletAngleOffset = targetGunData.multipleBulletAngleOffset;
        _bulletsPerShot = targetGunData.scatteredBulletsPerShot;
        _projectileSpeed = targetGunData.bulletSpeed;
        



        Projectile[] projectiles = new Projectile[_bulletsPerShot]; //Return an array because some weapons fire multiple bullets per shot.

        multipleBulletsAngleOffset = 0;

        var angle = Mathf.Atan2(player.position.y - firePoint.position.y, player.position.x - firePoint.position.x) * Mathf.Rad2Deg;

        for (int i = 0; i < _bulletsPerShot; i++)
        {
            projectiles[i] = targetMiniPool.GetPooledGameObject().GetComponent<Projectile>();
            projectiles[i].gameObject.SetActive(true);
            projectiles[i].transform.position = firePoint.position;
            // SetBulletSpeed(projectiles[i], i);
            //Refresh bullets after assigning their data; otherwise, the new values take effect only on the next shot.
            projectiles[i].SetprojectileSpeed(_projectileSpeed);


            if (_bulletsPerShot > 1)
            {
                if (i == 0)
                {
                    //Do not offset the first bullet's angle.
                }
                else if (i % 2 != 0) //For odd indices, apply a positive angle offset.
                {
                    multipleBulletsAngleOffset = Mathf.Abs(multipleBulletsAngleOffset);
                    multipleBulletsAngleOffset += _multipleBulletAngleOffset;
                }
                else //For even indices, apply a negative angle offset.
                {
                    multipleBulletsAngleOffset *= -1;
                }

            }

            projectiles[i].SetRotation(new Vector3(0, 0, angle + multipleBulletsAngleOffset));
            projectiles[i].transform.localScale = transform.localScale * targetGunData.projectileSize;
            //Keep scale positive because bullet orientation is already controlled by angle.
            if (projectiles[i].transform.localScale.x < 0) 
            {
                projectiles[i].transform.localScale = new Vector3(projectiles[i].transform.localScale.x * -1, projectiles[i].transform.localScale.y, projectiles[i].transform.localScale.z);
            }
            projectiles[i].RefreshVelocity();
        }
    }
}
