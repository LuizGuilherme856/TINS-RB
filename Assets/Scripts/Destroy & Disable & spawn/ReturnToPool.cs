using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToPool : MonoBehaviour
{
    [SerializeField] PoolCreator poolCreator;
    [SerializeField] private float maxLifeSpanTime = 3f, shrinkRate =1f;
    [SerializeField] private GameObject objectToSpawn;

    [SerializeField] private bool shrink = false;

    private float timer;
    private void OnEnable()
    {
        timer = maxLifeSpanTime;
       Invoke(nameof(ReturnToPoolNow), maxLifeSpanTime);
        
    }

    private void FixedUpdate()
    {
        if (shrink)
        {
            timer -= Time.deltaTime;
            if (timer < maxLifeSpanTime / 4)
            {
                transform.localScale = new Vector3(transform.localScale.x * shrinkRate, transform.localScale.y * shrinkRate, transform.localScale.z * shrinkRate);
            }

        }
    }

    public void ReturnToPoolNow()
    {
        if (objectToSpawn) Instantiate(objectToSpawn, transform.position, Quaternion.identity);
        poolCreator.ReleaseToPool();
    }
}
