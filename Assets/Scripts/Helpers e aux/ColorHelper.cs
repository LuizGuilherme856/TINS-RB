using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static color helpers shared by other classes.
/// </summary>
public class ColorHelper : MonoBehaviour
{
    public static Gradient CreateGradient(params Color[] colors)
    {
        Gradient gradient = new Gradient();

        // Create color keys based on the input colors
        GradientColorKey[] colorKeys = new GradientColorKey[colors.Length];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[colors.Length];

        for (int i = 0; i < colors.Length; i++)
        {
            float time;
            if(colors.Length == 1) //Avoid division by zero.
            {
                time = 0;
            }
            else
            {
                time = (float)i / (colors.Length - 1); // Distribute time evenly between 0 and 1
            }


            colorKeys[i] = new GradientColorKey(colors[i], time);
            alphaKeys[i] = new GradientAlphaKey(colors[i].a, time); // Use the alpha value of each color
        }

        gradient.SetKeys(colorKeys, alphaKeys);
        return gradient;
    }

    public static string FormatTextWithColor(string text, string hexColor)
    {
        return $"<color=#{hexColor}>{text}</color>";
    }

}
