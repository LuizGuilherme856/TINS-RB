using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToPoolOnCollision : MonoBehaviour
{
    [SerializeField] private bool useOnTrigger = false;
    [SerializeField] private bool destroyOnTouchingEnemy = true;

    private BulletDeathHandler bulletLifeTimeHandler;

    private MMMiniPoolableObject miniPoolableObject;
    private int collisionCounter = 0;
    [Tooltip("Para o caso do player nao precisa inicializar essa variavel. O script UpdateBulletGunData ja faz isso automatico")]
    [SerializeField] private WeaponData gunData;

    private void Awake()
    {
        miniPoolableObject = GetComponent<MMMiniPoolableObject>();
        if(gunData == null) gunData = GetComponent<BulletStats>().GetGunData();
        bulletLifeTimeHandler = GetComponent<BulletDeathHandler>();
    }

    private void OnEnable()
    {
        collisionCounter = 0;// Reset state when reusing a pooled object.
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Enemy") && destroyOnTouchingEnemy)
        {
            ReturnToPool();
            return;
        }

        collisionCounter++;
        if(collisionCounter >= gunData.maxCollisions)
        {
            ReturnToPool();

        }

    }

    private void ReturnToPool()
    {
        if (bulletLifeTimeHandler != null)
        {
            //If a bullet lifetime handler exists, let it return the bullet to the pool instead.
            bulletLifeTimeHandler.BulletKilledByTime();
            return;
        }

        miniPoolableObject.Destroy();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (useOnTrigger && collision.CompareTag("Melee")) //Allow melee hits.
        {
            collisionCounter++;
            if (collisionCounter >= gunData.maxCollisions)
            {
                ReturnToPool();
            }

        }
    }
}
