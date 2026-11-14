using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Launch any object in a specified direction.
/// </summary>
public class ObjectShooter : MonoBehaviour
{
    public bool targetsPlayer = false;
    public GameObject objectPrefab;
    public Transform target; 
    public Transform origin; //Optional firing origin.
    public float launchForce = 10f;
    public int poolSize = 10;

    private List<GameObject> objectPool;

    void Awake()
    {
        // Initialize the object pool.
        objectPool = new List<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            AddOneBulletToPool();
        }

        if (targetsPlayer == true && target == null)
        {
            try
            {
                target = GameObject.FindGameObjectWithTag("Player").transform;
            }
            catch (System.Exception)
            {
                Debug.LogWarning("Não achou o player na scene");
            }

        }
    }



    public void LaunchObject()
    {
        GameObject obj = GetPooledObject();

        //The pool is empty.
        if(obj == null)
        {
            obj = AddOneBulletToPool();
        }

        obj.transform.position = origin != null ? origin.position : transform.position; // Use the assigned origin if available.

        // Calculate direction in the XY plane.
        Vector3 direction = target.position - obj.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        obj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        obj.SetActive(true);
        obj.transform.parent = null;

        // Apply force to the object.
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(direction.normalized * launchForce, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogError("Objeto não possui Rigidbody");
        }
        
    }

    private GameObject GetPooledObject()
    {
        foreach (GameObject obj in objectPool)
        {
            if (obj.activeInHierarchy == false)
            {
                return obj;
            }
        }
        return null;
    }

    /// <summary>
    /// Add another bullet to the pool and return it.
    /// </summary>
    /// <returns></returns>
    private GameObject AddOneBulletToPool()
    {
        GameObject obj = Instantiate(objectPrefab);
        obj.SetActive(false);
        obj.transform.parent = transform;
        objectPool.Add(obj);
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.parent = transform;
    }
}


