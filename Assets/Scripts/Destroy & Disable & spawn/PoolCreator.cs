using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Pooling through Unity's built-in pool API.
/// </summary>
public class PoolCreator : MonoBehaviour
{
    private IObjectPool<PoolCreator> objectPool;
    public bool HasPool => objectPool != null;

    [SerializeField] private bool showDebug;
    

    public void SetPool(IObjectPool<PoolCreator> pool)
    {
        objectPool = pool;
    }

    public void ReleaseToPool()
    {
        try
        {
            objectPool.Release(this);

        }
        catch (System.Exception)
        {
            //Debug.Log("Erro na pool");
        }

    }

}
