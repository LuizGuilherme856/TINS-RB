using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleBulletImpact : MonoBehaviour
{
    
    [SerializeField] private bool isExplosive = false;

    [SerializeField] private bool hurtPlayer = false;

    private BulletRicochetEffectPooler bulletImpactSpawner;
    private Material material;
    

    private GunData gunData;
    private bool spawnedEffect = false;

    private void Awake()
    {
        //bulletImpactSpawner = FindObjectOfType<BulletRicochetEffectPooler>();
        //gunData = (GunData)GetComponent<BulletStats>().GetGunData();
    }

    private void OnEnable()
    {
        spawnedEffect = false;
    }

    public void SpawnBulletImpact( Vector3 position, Collision2D collision = null)
    {
        if (gunData == null) gunData = GetComponent<BulletStats>().GetGunData();

        if (Random.Range(0f, 1f) > gunData.impactFxSpawnProbability || spawnedEffect) return; //Handles collision impacts only; lifetime expiration is handled separately.

        var bulletRicochetFx = bulletImpactSpawner.GetPooler(gunData.bulletRicochetFx.name).GetPooledGameObject();

        SpawnEffect(collision, bulletRicochetFx);
    }



    private void SpawnEffect(Collision2D collision, GameObject effectGameObj)
    {
        spawnedEffect = true;
        effectGameObj.transform.position = transform.position;
        effectGameObj.transform.parent = null;
        if (material != null) effectGameObj.GetComponent<SpriteRenderer>().material = material;
        effectGameObj.SetActive(true);

        if (hurtPlayer && collision != null) // TODO: Improve this implementation; the current version is retained because it works.
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<HandleAttackOnPlayer>().DamagePlayer(1);
            }
        }

        if (isExplosive)
        {
            var explosion = effectGameObj.GetComponent<Explosion>();
            explosion.transform.position = transform.position;
            
            explosion.TriggerExplosion();
        }
    }

    public void ChangeBulletImpactMaterial( Material material)
    {
        this.material = material;

    }

    public void SetGunData(GunData gunData, BulletRicochetEffectPooler bulletRicochetEffectPooler)
    {
        //Debug.Log($"coloquei {gunData.name} no {gameObject.name}");
        this.gunData = gunData;
        this.bulletImpactSpawner = bulletRicochetEffectPooler;
    }
}
