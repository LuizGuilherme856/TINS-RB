using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorpsePoolManager : MonoBehaviour
{
    //Populate the component list automatically from assigned GameObjects.

    [SerializeField] private MMMiniObjectPooler[] objectPoolers;

    public GameObject GetEnemyCorpseFromPool(string name )
    {
        // Debug.Log("Requested name: " + name + " stored name: " + corpsePoolers[0].);
        for (int i = 0; i < objectPoolers.Length; i++)
        {
            if (objectPoolers[i].GameObjectToPool.name == name)
            {
                var pooledGameObject = objectPoolers[i].GetPooledGameObject();
                pooledGameObject.SetActive(true);
                return pooledGameObject;
            }
        }

        Debug.LogError("Corpse not found! Make sure the corpse is set in the inspector. Maybe the name is wrong?");
        return null;
    }
}
