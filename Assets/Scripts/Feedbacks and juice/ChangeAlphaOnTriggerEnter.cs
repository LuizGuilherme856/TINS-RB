using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeAlphaOnTriggerEnter : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.5f; // Transition duration.
    [SerializeField] private float targetAlpha = 0.5f; // Alpha on trigger entry.
    private SpriteRenderer spriteRenderer;
    private float originalAlpha;
    private Coroutine fadeCoroutine;
    private int objectsInTrigger = 0; // Number of objects inside the trigger.

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalAlpha = spriteRenderer.color.a;
        }
        else
        {
            Debug.LogError("SpriteRenderer não encontrado no GameObject.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Enemy"))
        {
            objectsInTrigger++;
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(ChangeAlpha(targetAlpha));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Enemy"))
        {
            objectsInTrigger--;
            if (objectsInTrigger <= 0)
            {
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                }
                fadeCoroutine = StartCoroutine(ChangeAlpha(originalAlpha));
            }
        }
    }

    private IEnumerator ChangeAlpha(float targetAlpha)
    {
        Color color = spriteRenderer.color;
        float startAlpha = color.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            spriteRenderer.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        spriteRenderer.color = color;
    }
}
