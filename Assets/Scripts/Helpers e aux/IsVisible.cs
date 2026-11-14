using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsVisible : MonoBehaviour
{
    bool isVisibleToTheCamera = false;
    void OnBecameInvisible()
    {
        isVisibleToTheCamera = false;
    }

    // Enable this script when the GameObject moves into the camera's view
    void OnBecameVisible()
    {
        isVisibleToTheCamera = true;
    }

    public bool GetIsVisible()
    {
        return isVisibleToTheCamera;
    }
}
