///Similar to CapsuleSpawner; consider sharing common behavior.
using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class MuzzleFlashSpawner : MonoBehaviour
{
    [SerializeField] private ShootingHandler shootingHandler;
    [SerializeField] private Aim aim;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private List<MMMiniObjectPooler> poolers;

    private MMMiniObjectPooler currentPool;
    private GunData gunData;
    private Vector2 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        var poolers = GetComponents<MMMiniObjectPooler>();
        this.poolers.AddRange(poolers);
        
    }

    private void OnEnable()
    {
        shootingHandler.OnShoot += InstantiateMuzzleFlash;
    }
    private void UpdatePool()
    {
        string pooledGameObjName = gunData.muzzleFlash.name;

        for (int i = 0; i < poolers.Count; i++)
        {
            if (poolers[i].GameObjectToPool.name == pooledGameObjName)
            {
                currentPool = poolers[i];
            }
        }
    }

    private void InstantiateMuzzleFlash()
    {
        if (gunData.hasMuzzleFlash)
        {
            GameObject newFlash = currentPool.GetPooledGameObject();
            newFlash.SetActive(true);
            var flashTransform = newFlash.transform;
            flashTransform.rotation = aim.transform.rotation;
            
            var angle = aim.GetAimAngle() *Mathf.Deg2Rad;
            var offSet = gunData.muzzleFlashSpawnOffset;
            flashTransform.position = transform.position + new Vector3(Mathf.Cos(angle)*offSet, Mathf.Sin(angle) * offSet )  ;
            
            if (aim.GetIsAimingRight())
            {
                flashTransform.localScale = originalScale;
            }
            else
            {
                flashTransform.localScale = new Vector3(-originalScale.x, originalScale.y, 1);
            }

        }

    }

    public void UpdateBulletData(GunData gunData)
    {
        this.gunData = gunData;
        UpdatePool();
    }





}
