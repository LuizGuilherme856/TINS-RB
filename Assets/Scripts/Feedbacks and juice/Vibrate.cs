using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vibrate : MonoBehaviour
{
    public float duration = 0.5f; // Vibration duration in seconds.
    public float magnitude = 0.1f; // Vibration intensity.

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.localPosition;
        StartVibration();
    }

    public void StartVibration()
    {
        StopAllCoroutines();
        StartCoroutine(VibrateCoroutine());
    }

    private IEnumerator VibrateCoroutine()
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}
