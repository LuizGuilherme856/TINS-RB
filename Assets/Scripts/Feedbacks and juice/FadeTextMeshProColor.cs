using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Fade TextMesh Pro text without creating another material.
/// </summary>

public class FadeTextMeshProColor : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI textTMP;
    [SerializeField] private float speed = 1.0f;
    //[SerializeField] private float updateInterval = 0.2f;

    [SerializeField] private float maxAlphaValue = 1.0f;
    [SerializeField] private float minAlphaValue = 0f;

    private float currentAlphaValue;
    private bool increasing = true;

    void Start()
    {

       // InvokeRepeating(nameof(UpdateColor), 0, updateInterval); //Run at the configured interval.
    }

    private void FixedUpdate()
    {
        UpdateColor();
    }



    private void UpdateColor() // Increase the color value by speed on each call.
    {
        currentAlphaValue = textTMP.color.a;

        //Debug.Log(currentAlphaValue);

        if (currentAlphaValue <= minAlphaValue && increasing == false) //If the value is near its minimum and decreasing,
        {
            increasing = true; //start increasing it instead.
        }
        else if (currentAlphaValue >= maxAlphaValue && increasing == true)
        {
            increasing = false;
        }


        float newAlpha;

        if (increasing)
        {
            newAlpha = currentAlphaValue + Time.fixedDeltaTime* speed;
        }
        else
        {
            newAlpha = currentAlphaValue - Time.fixedDeltaTime * speed;
        }


        textTMP.color = new Color(255, 255, 255, newAlpha);
    }
}
