using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCloneManager : MonoBehaviour
{
    [SerializeField] private float lifeTime = 20f;
    [SerializeField] private GunData defaultGun;
    [SerializeField] private GunRecoil gunRecoil;
    [SerializeField] private MMF_Player spawnFeedbacks;
    [SerializeField] private GameObject deathFx;

    private float lifetimeTimer = 0;

    private void OnEnable()
    {
        gunRecoil.SetCurrentGunData(defaultGun);
    }

    private void Update()
    {
        lifetimeTimer += Time.deltaTime;
        if(lifetimeTimer >= lifeTime)
        {
            Instantiate(deathFx, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }


}
