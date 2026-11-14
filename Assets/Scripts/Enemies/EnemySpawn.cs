
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
/// <summary>
/// Use a pool to instantiate enemies.
/// Each GameObject spawns one enemy type.
/// </summary>
public class EnemySpawn : MonoBehaviour, IPoolImplementation
{
    private IObjectPool<PoolCreator> enemyPool;

    [SerializeField] private GameObject enemyObject;
    [SerializeField] private EnemyType enemyType;
    [SerializeField] private int poolsize = 100;
    private Transform spawnTransform;

    private void Awake()
    {
        enemyPool = new ObjectPool<PoolCreator>(CreatePoolObject, OnGet, OnRelease, OnDestroyPoolObject, maxSize: poolsize);
    }


    public void Spawn(Transform transform)
    {
        spawnTransform = transform;
        var newEnemy = enemyPool.Get();
    }

    /// <summary>
    /// Spawn and return the enemy GameObject; callers may ignore the return value
    /// unless they need a reference to the spawned instance.
    /// </summary>
    public GameObject SpawnAndGet(Transform transform)
    {
        spawnTransform = transform;
        var newEnemy = enemyPool.Get();
        return newEnemy.gameObject;


    }

    public EnemyType GetEnemyType()
    {
        return enemyType;
    }


    #region POOL METHODS

    public PoolCreator CreatePoolObject()
    {
        PoolCreator enemy = Instantiate(enemyObject.GetComponent<PoolCreator>());
        enemy.SetPool(enemyPool);
        return enemy;
    }

    public void OnGet(PoolCreator obj)
    {
        try
        {
            obj.GetComponent<EnemyHealth>().ReplenishHealth();
        }
        catch (System.Exception)
        {
            obj.GetComponentInChildren<EnemyHealth>().ReplenishHealth();
        }
        obj.gameObject.SetActive(true);
        obj.transform.position = spawnTransform.position;
    }

    public void OnRelease(PoolCreator obj)
    {
        obj.gameObject.SetActive(false);
    }

    public void OnDestroyPoolObject(PoolCreator obj)
    {
        Destroy(obj.gameObject);
    }

    #endregion

}
