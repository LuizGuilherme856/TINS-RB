using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSpriteAlpha : MonoBehaviour
{
    public enum CallMode
    {
        OnEnable,
        Awake,
        Start,
        Manual
    }

    [Header("Sprite Renderers")]
    public List<SpriteRenderer> spriteRenderers;

    [Header("Alpha Settings")]
    [Range(0f, 1f)] public float initialAlpha = 1f;
    [Range(0f, 1f)] public float targetAlpha = 0f;
    public float duration = 1f;
    public float delay = 0f;

    [Header("When to Execute")]
    public CallMode callMode = CallMode.Manual;

    private Coroutine alphaCoroutine;

    void Awake()
    {
        if (callMode == CallMode.Awake)
            StartAlphaChange();
    }

    void OnEnable()
    {
        if (callMode == CallMode.OnEnable)
            StartAlphaChange();
    }

    void Start()
    {
        if (callMode == CallMode.Start)
            StartAlphaChange();
    }

    public void StartAlphaChange()
    {
        if (alphaCoroutine != null)
            StopCoroutine(alphaCoroutine);

        alphaCoroutine = StartCoroutine(ChangeAlphaCoroutine());
    }

    private IEnumerator ChangeAlphaCoroutine()
    {
        // Set initial alpha
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                var color = sr.color;
                color.a = initialAlpha;
                sr.color = color;
            }
        }

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float time = 0f;
        while (time < duration)
        {
            float t = time / duration;
            float currentAlpha = Mathf.Lerp(initialAlpha, targetAlpha, t);

            foreach (var sr in spriteRenderers)
            {
                if (sr != null)
                {
                    var color = sr.color;
                    color.a = currentAlpha;
                    sr.color = color;
                }
            }

            time += Time.deltaTime;
            yield return null;
        }

        // Ensure the final alpha is set
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                var color = sr.color;
                color.a = targetAlpha;
                sr.color = color;
            }
        }
    }
}
