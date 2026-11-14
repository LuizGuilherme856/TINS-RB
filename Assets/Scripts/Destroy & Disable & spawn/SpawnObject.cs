using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private Transform pos;
    [SerializeField] private bool useTimer = true, rotate = true;
    [SerializeField] private float timer = 1.5f;
    

    
    void OnEnable()
    {

        if(useTimer)
        {
            Invoke(nameof(SpawnObjectNow), timer);
            return;
        }

    }

  
    public void SpawnObjectNow()
    {
        Debug.Log("Trying to spawn");
        GameObject obj;
        if (pos) //Use this transform when no explicit position is supplied.
        {
             obj = Instantiate(objectToSpawn, pos.position, pos.rotation);
        }
        else
        {
            if(rotate) obj = Instantiate(objectToSpawn, transform.position, transform.rotation);
            else obj = Instantiate(objectToSpawn, transform.position, Quaternion.identity);
        }
        obj.SetActive(true);
        Debug.Log("spawn: "+objectToSpawn.name);

        //gameObject.SetActive(false);
    }
}
