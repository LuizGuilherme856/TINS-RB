using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offSet = new Vector3(0,0,0);
    [SerializeField] bool onEnableFollow = false;
    [SerializeField] bool followRotation = false;
    [SerializeField] bool followScaleOrientation = false;

    private void OnEnable()
    {
        if (onEnableFollow)
        {
            if (followRotation)
            {
                transform.rotation = target.rotation;
            }
            if (followScaleOrientation)
            {
                //Match scale signs so both objects face the same direction.
                if ((target.transform.localScale.x * this.gameObject.transform.localScale.x) < 0)
                {
                    this.gameObject.transform.localScale = new Vector3(this.gameObject.transform.localScale.x * -1, this.gameObject.transform.localScale.y, this.gameObject.transform.localScale.z);
                }

            }
            transform.position = target.position + offSet;
        }
    }

    private void Start()
    {
        
        if (followRotation)
        {
            transform.rotation = target.rotation;
        }
        if (followScaleOrientation)
        {
            //Match scale signs so both objects face the same direction.
            if ((target.transform.localScale.x * this.gameObject.transform.localScale.x) < 0)
            {
                this.gameObject.transform.localScale = new Vector3(this.gameObject.transform.localScale.x * -1, this.gameObject.transform.localScale.y, this.gameObject.transform.localScale.z);
            }

        }
        transform.position = target.position + offSet;
    }

    private void Update()
    {
        if (followRotation)
        {
            transform.rotation = target.rotation;
        }
        if (followScaleOrientation)
        {
            //Match scale signs so both objects face the same direction.
            if ((target.transform.localScale.x * this.gameObject.transform.localScale.x) < 0)
            {
                this.gameObject.transform.localScale = new Vector3 (this.gameObject.transform.localScale.x * -1, this.gameObject.transform.localScale.y, this.gameObject.transform.localScale.z);
            }
 
        }
        transform.position = target.position+ offSet;
    }
}
