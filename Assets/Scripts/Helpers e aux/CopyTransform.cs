using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Copy another transform's values to this one.
/// </summary>
public class CopyTransform : MonoBehaviour
{
    public Transform otherTransform;
    public bool copyPosition = true;
    public bool copyRotation = true;
    public bool copyScale = true;

    private void Awake()
    {
        if(copyPosition)
            transform.position = otherTransform.position;
        if (copyRotation)
            transform.rotation = otherTransform.rotation;
        if (copyScale)
            transform.localScale = otherTransform.localScale;

    }


}
