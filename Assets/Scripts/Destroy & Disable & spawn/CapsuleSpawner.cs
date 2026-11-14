using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class CapsuleSpawner : MonoBehaviour, IPoolImplementation
{
    [SerializeField] private ShootingHandler shootingHandler;
    [SerializeField] private Aim aim;
    [SerializeField] private RunDataManager runDataManager;

    [SerializeField] private bool playerClone= false;
    private RunData runData;
    private IObjectPool<PoolCreator> capsulePool;
     private GunData gunData;
    private Vector2 originalScale;

    private void Awake()
    {
        //Workaround for De Niro's ability clone.
        if (playerClone) return;
        capsulePool = new ObjectPool<PoolCreator>(CreatePoolObject, OnGet, OnRelease, OnDestroyPoolObject, maxSize: 20);
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (playerClone) return;
        runData = runDataManager.GetRunData();
        shootingHandler.OnShoot += InstantiateCapsule;
    }

    private void InstantiateCapsule()
    {

        if (runData.currentWaveNumber > 10) return; //Stop spawning casings after the tenth.

        if (gunData.dropCapsuleOnshooting)
        {
            GameObject newCapsule = capsulePool.Get().gameObject;
            if (aim.GetIsAimingRight())
            {
                newCapsule.transform.position = transform.position - gunData.capsuleSpawnOffset;
            }
            else
            {
                newCapsule.transform.position = transform.position + gunData.capsuleSpawnOffset;
            }
            newCapsule.transform.localScale = originalScale;
        }

    }


    public void UpdateBulletData( GunData gunData)
    {

        this.gunData = gunData;
        NewPool();
    }

    private void NewPool()
    {
        capsulePool.Clear();
        capsulePool = new ObjectPool<PoolCreator>(CreatePoolObject, OnGet, OnRelease, OnDestroyPoolObject, maxSize: gunData.GetMaxAmmoOnClip());
    }
    public PoolCreator CreatePoolObject()
    {
        PoolCreator capsule = Instantiate(gunData.bulletCapsule.GetComponent<PoolCreator>());
        capsule.SetPool(capsulePool);
        return capsule;
    }

    public void OnGet(PoolCreator obj)
    {
        obj.gameObject.SetActive(true);
    }

    public void OnRelease(PoolCreator obj)
    {
        obj.gameObject.SetActive(false);
    }
    public void OnDestroyPoolObject(PoolCreator obj)
    {
        Destroy(obj.gameObject);
    }
}
