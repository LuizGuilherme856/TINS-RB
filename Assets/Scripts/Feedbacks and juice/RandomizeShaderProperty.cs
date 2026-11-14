///Change the selected shader property.
///This behavior could not be implemented through MMF.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeShaderProperty : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private string[] propertyName;
    [SerializeField] private float [] minValue, maxValue; 

    private void OnEnable()
    {
        var material = spriteRenderer.material;

        for (int i = 0; i < propertyName.Length; i++)
        {
            float value = Random.Range(minValue[i], maxValue[i]); //Randomize generated embers.
           
            material.SetFloat(propertyName[i], value);
        }
    }
}
