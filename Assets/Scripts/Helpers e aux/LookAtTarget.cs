using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public bool lookAtPlayer;

    [SerializeField] protected Transform target;

    private void Awake()
    {
        if (lookAtPlayer)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
    private void OnEnable()
    {
        Vector2 dir = ((Vector2)target.position - (Vector2)this.gameObject.transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        this.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90);
    }
    void Start()
    {
        if (lookAtPlayer)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
        
    }

   
    protected void Update()
    {
        
        Vector2 dir = ((Vector2)target.position - (Vector2)this.gameObject.transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        this.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90);
    }
}
