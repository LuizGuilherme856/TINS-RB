using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveParentOnEnable : MonoBehaviour
{

    private Transform parentTransform;
    private void Awake()
    {
        parentTransform = transform.parent;
        gameObject.SetActive(true); // ??? 
    }

    private void OnEnable()
    {
        transform.position = parentTransform.position;
        transform.parent = null;
    }
}
