using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Find a specific pooler in a list.
/// </summary>

public class HandlePoolerList : MonoBehaviour
{
    [SerializeField] private GameObject bulletPoolObject;

    [SerializeField] private List<MMMiniObjectPooler> poolers;


    private void Awake()
    {
        FillPoolList();
    }


    private void FillPoolList()
    {
        var poolers = bulletPoolObject.GetComponents<MMMiniObjectPooler>();
        this.poolers.AddRange(poolers);
    }

    public MMMiniObjectPooler GetPooler(string gameObjName)
    {
        foreach (var pooler in poolers)
        {
            if (pooler.GameObjectToPool.name == gameObjName)
            {
                return pooler;
            }
        }
        Debug.LogError("nao achou a pooler " + gameObjName);
        return null;
    }
}
