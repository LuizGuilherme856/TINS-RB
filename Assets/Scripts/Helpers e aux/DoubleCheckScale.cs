using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleCheckScale : MonoBehaviour
{

    public float delayAfterEnable = 1f;
    public Vector3 targetScale = Vector3.one;

    private void OnEnable()
    {
        StartCoroutine(CheckScaleAfterEnable());
    }
    private IEnumerator CheckScaleAfterEnable()
    {
        yield return new WaitForSeconds(delayAfterEnable);
        if (transform.localScale != targetScale)
        {
            transform.localScale = targetScale;
        }
    }

}
