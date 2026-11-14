using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Objects fade before destruction and may return from the pool transparent;
//restore their default color on enable.
public class DefaultColorOnEnable : MonoBehaviour
{
    private Color spriteColor;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteColor = spriteRenderer.color;
    }
    private void OnEnable()
    {
        spriteRenderer.color= spriteColor;
    }

}
