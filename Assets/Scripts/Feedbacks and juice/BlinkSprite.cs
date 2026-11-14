using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkSprite : MonoBehaviour
{
    [Header("Sprite to Blink")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Blink Settings")]
    [Tooltip("Total duration of the blinking effect (seconds). If 0, blinks forever.")]
    [SerializeField] private float totalDuration = 0f;
    [Tooltip("How many times per second the sprite blinks.")]
    [SerializeField] private float frequency = 2f;
    [Tooltip("Minimum alpha value during blink.")]
    [Range(0f, 1f)]
    [SerializeField] private float minAlpha = 0.2f;
    [Tooltip("Maximum alpha value during blink.")]
    [Range(0f, 1f)]
    [SerializeField] private float maxAlpha = 1f;

    private Coroutine blinkCoroutine;

    void OnEnable()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        StartBlink();
    }

    void OnDisable()
    {
        StopBlink();
    }

    public void StartBlink()
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    public void StopBlink()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        if (spriteRenderer != null)
        {
            var color = spriteRenderer.color;
            color.a = maxAlpha;
            spriteRenderer.color = color;
        }
    }

    private IEnumerator BlinkRoutine()
    {
        float elapsed = 0f;
        while (totalDuration <= 0f || elapsed < totalDuration)
        {
            float t = Mathf.PingPong(Time.time * frequency, 1f);
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

            if (spriteRenderer != null)
            {
                var color = spriteRenderer.color;
                color.a = alpha;
                spriteRenderer.color = color;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure sprite returns to max alpha at the end
        if (spriteRenderer != null)
        {
            var color = spriteRenderer.color;
            color.a = maxAlpha;
            spriteRenderer.color = color;
        }
    }
}
