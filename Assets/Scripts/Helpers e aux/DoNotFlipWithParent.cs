using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotFlipWithParent : MonoBehaviour
{
    [SerializeField] private Transform parentTransf;
    private void Update()
    {

        if (parentTransf.localScale.x == -1)
        {

            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {

            transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
