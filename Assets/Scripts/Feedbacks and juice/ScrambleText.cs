using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScrambleText : MonoBehaviour
{
    private string SCRAMBLE_EFFECT_NAME = "_GaussianBlurFade";

    [SerializeField] protected Material material;
    [SerializeField] protected TextMeshProUGUI textMesh;

    public void UnscrambleText_Animation(float duration)
    {
        // Set the material to the unscrambled state
        material.SetFloat(SCRAMBLE_EFFECT_NAME, 1f);
        ChangeMaterialFloat(0, duration);
    }

    public void ScrambleText_Animation(float duration)
    {
        // Set the material to the scrambled state
        material.SetFloat(SCRAMBLE_EFFECT_NAME, 0f);
        ChangeMaterialFloat(1, duration);
    }


    private void ChangeMaterialFloat(float targetValue, float duration)
    {
        StartCoroutine(ChangeMaterialFloatCoroutine(targetValue, duration));
    }

    private IEnumerator ChangeMaterialFloatCoroutine(float targetValue, float duration)
    {
        float initialValue = material.GetFloat(SCRAMBLE_EFFECT_NAME);
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float currentValue = Mathf.Lerp(initialValue, targetValue, t);
            material.SetFloat(SCRAMBLE_EFFECT_NAME, currentValue);
            yield return null;
        }

        // Ensure the final value is set exactly
        material.SetFloat(SCRAMBLE_EFFECT_NAME, targetValue);
    }
}
