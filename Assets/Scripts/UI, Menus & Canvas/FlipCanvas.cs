using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Flip the duck's Canvas so its contents remain readable.
public class FlipCanvas : MonoBehaviour
{
    [SerializeField] private GameObject canvas;
    void Update()
    {
        if (transform.localScale.x == 1) canvas.transform.localScale = new Vector3(1,1,1);
        else if (transform.localScale.x == -1) canvas.transform.localScale = new Vector3(-1, 1, 1);
    }
}
