using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothVibration : MonoBehaviour
{
    public float duration = 0.5f;      // Vibration duration in seconds.
    public float magnitude = 0.1f;     // Maximum vibration intensity.
    public bool startOnStart = false; // Whether vibration starts automatically.

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.localPosition;

        if (startOnStart)
        {
            StartSmoothVibration();
        }
    }

    public void StartSmoothVibration()
    {
        StopAllCoroutines();
        StartCoroutine(VibrateCoroutine());
    }

    private IEnumerator VibrateCoroutine()
    {
        float elapsed = 0f;
        float seed = Random.value * 100f;

        while (elapsed < duration)
        {
            float x = (Mathf.PerlinNoise(seed, Time.time * 10f) - 0.5f) * 2f * magnitude;
            float y = (Mathf.PerlinNoise(seed + 1f, Time.time * 10f) - 0.5f) * 2f * magnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}
