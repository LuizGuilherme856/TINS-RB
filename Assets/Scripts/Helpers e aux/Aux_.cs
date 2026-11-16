//Centralized control over debug output.
//Toggle the flag to disable all these messages.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using Random = UnityEngine.Random;

/// <summary>
/// General-purpose utility methods.
/// </summary>
public static class Aux
{

    public static void Print(string msg, bool showDebug)
    {
        if (showDebug)
        {
            Debug.Log(msg);
        }
    }

    /// <summary>
    /// Randomly choose a positive or negative sign.
    /// </summary>
    /// <param name="number"></param>
    /// <returns>The value with a randomly selected sign.</returns>
    public static float RandomizeFloatSign(float number)
    {
        float newNumber = Mathf.Abs(number);
        bool positive = (Random.value > 0.5f);
        if (positive) return newNumber;
        else return -newNumber;
    }

    /// <summary>
    /// Format fractional values with a decimal place, omitting it for whole numbers.
    /// </summary>
    public static string FormatNumber(float number, int decimalPlaces = 1)
    {
        // Build the format from the requested decimal precision.
        string formatString = "F" + decimalPlaces;

        // Check whether the value has a fractional part.
        if (number % 1 != 0)
        {
            // Format using the requested number of decimal places.
            return number.ToString(formatString, CultureInfo.InvariantCulture);
        }
        else
        {
            // Format without decimal places.
            return number.ToString("F0", CultureInfo.InvariantCulture);
        }

    }

    #region return extensions

    /// <summary>
    /// Clamp negative values to zero.
    /// </summary>
    public static float ReturnZeroIfNegative(float value)
    {
        if (value < 0)
        {
            Debug.LogWarning($"O valor informado {value} é menor que zero. Retornando zero");
            return 0;
        }

        return value;
    }
    /// <summary>
    /// Clamp negative values to zero.
    /// </summary>
    public static int ReturnZeroIfNegative(int value)
    {
        if (value < 0) return 0;
        return value;
    }
    /// <summary>
    /// Clamp values below 1 to 1.
    /// </summary>
    public static int ReturnOneIfLessThanOne(int value)
    {
        if (value < 1) return 1;
        return value;
    }

    public static float MaxValueToReturn(float value, float maxValue)
    {
        if (value > maxValue) return maxValue;
        return value;
    }

    public static int MaxValueToReturn(int value, int maxValue)
    {
        if (value > maxValue) return maxValue;
        return value;
    }

    /// <summary>
    /// Return minValue when valueToCheck is smaller.
    /// </summary>
    public static float MinValueToReturn(float valueToCheck, float minValue)
    {
        if (valueToCheck < minValue) return minValue;
        return valueToCheck;
    }

    #endregion


    /// <summary>
    /// Check for component T on the supplied object.
    /// Add it if missing.
    /// </summary>
    public static void EnsureComponent<T>(GameObject obj) where T : Component
    {
        if (obj.GetComponent<T>() == null)
        {
            obj.AddComponent<T>();
            Debug.Log($"O objeto \"{obj.name}\" não possuia o componente necessário. Componente foi adicionado manualmente");
        }
    }

    #region animator extension methods

    /// <summary>
    /// Reset the supplied Animator's triggers.
    /// Useful for long animations such as the bomber's.
    /// </summary>
    /// <param name="animator"></param>
    public static void ResetAllAnimationTriggers(Animator animator)
    {
        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(param.name);
            }
        }
    }
    /// <summary>
    /// Reset Animator triggers except the directional defaults Horizontal, Up, and Down.
    /// Useful for long animations such as the bomber's.
    /// </summary>
    /// <param name="animator"></param>
    public static void ResetAllAnimationTriggersButDirectional(Animator animator)
    {
        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                if (param.name == "Horizontal" || param.name == "Up" || param.name == "Down")
                {

                }
                else
                {
                    animator.ResetTrigger(param.name);
                }

            }
        }
    }

    public static void ResetOnlyDirectionalTriggers(Animator animator)
    {
        foreach (var param in animator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                if (param.name == "Horizontal" || param.name == "Up" || param.name == "Down")
                {
                    animator.ResetTrigger(param.name);
                }
                else
                {

                }

            }
        }
    }

    #endregion



    /// <summary>
    /// Generate a random value in [minValue, maxValue],
    /// weighted toward either end of the interval.
    /// Larger weightFactor values favor minValue.
    /// Positive weightFactor values below 1 favor maxValue.
    /// </summary>
    /// <param name="minValue">Minimum interval value.</param>
    /// <param name="maxValue">Maximum interval value.</param>
    /// <param name="weightFactor">
    /// Distribution weight:
    /// - Values above 1 favor minValue.
    /// - Positive values below 1 favor maxValue.
    /// - A value of 1 produces a uniform distribution.
    /// </param>
    /// <returns>A weighted random value in [minValue, maxValue].</returns>
    /// 
    public static float RandomNumberWeighted(float minValue, float maxValue, float weightFactor = 2f)
    {
        // Generate a random number between 0 and 1.
        float randomValue = Random.value;

        // Apply the weight; larger factors favor lower values.
        float weightedValue = Mathf.Pow(randomValue, weightFactor);

        // Map the weighted value to [min, max].
        return Mathf.Lerp(minValue, maxValue, weightedValue);
    }

    /// <summary>
    /// Test RandomNumberWeighted's distribution.
    /// </summary>
    public static void RandomNumberWeighted_RunTest(float min, float max, float weightFactor)
    {
        int numberOfBuckets = 10;
        int numberOfSamples = 100000;

        int[] buckets = new int[numberOfBuckets];
        float bucketSize = (max - min) / numberOfBuckets;

        // Draw the samples.
        for (int i = 0; i < numberOfSamples; i++)
        {
            float randomValue = RandomNumberWeighted(min, max, weightFactor);

            // Find the interval containing the sampled value.
            int bucketIndex = Mathf.Clamp((int)((randomValue - min) / bucketSize), 0, numberOfBuckets - 1);
            buckets[bucketIndex]++;
        }

        // Print the results.
        Debug.Log("Distribuição dos valores sorteados:");
        for (int i = 0; i < numberOfBuckets; i++)
        {
            float bucketStart = min + i * bucketSize;
            float bucketEnd = bucketStart + bucketSize;
            float percentage = (buckets[i] / (float)numberOfSamples) * 100f;

            Debug.Log($"Intervalo {bucketStart:F2} - {bucketEnd:F2}: {percentage:F2}%");
        }
    }

    /// <summary>
    /// Converts an integer to its Roman numeral representation (supports 1 to 3999).
    /// </summary>
    public static string ToRoman(int number)
    {
        if (number < 1 || number > 3999)
            return string.Empty;

        var romanNumerals = new[]
        {
        new { Value = 1000, Symbol = "M" },
        new { Value = 900, Symbol = "CM" },
        new { Value = 500, Symbol = "D" },
        new { Value = 400, Symbol = "CD" },
        new { Value = 100, Symbol = "C" },
        new { Value = 90, Symbol = "XC" },
        new { Value = 50, Symbol = "L" },
        new { Value = 40, Symbol = "XL" },
        new { Value = 10, Symbol = "X" },
        new { Value = 9, Symbol = "IX" },
        new { Value = 5, Symbol = "V" },
        new { Value = 4, Symbol = "IV" },
        new { Value = 1, Symbol = "I" }
    };

        var result = string.Empty;
        foreach (var item in romanNumerals)
        {
            while (number >= item.Value)
            {
                result += item.Symbol;
                number -= item.Value;
            }
        }
        return result;
    }


    /// <summary>
    /// Format upgrade/rune text with positive effects in green and negative effects in red.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string FormatUpgradeText(string input)
    {
        // Split on semicolons.
        string[] parts = input.Split(';');
        string result = "";

        foreach (string part in parts)
        {
            string trimmed = part.Trim();

            // Replace (value) with green text, removing the parentheses.
            trimmed = Regex.Replace(trimmed, @"\((\+?[^\)]+)\)", "<color=green>$1</color>");

            // Replace {value} with red text, removing the braces.
            trimmed = Regex.Replace(trimmed, @"\{(\-?[^\}]+)\}", "<color=red>$1</color>");

            // Add a bullet point and line break.
            result += $"• {trimmed}\n";
        }

        return result.TrimEnd(); // Remove the final line break.
    }




}

[Serializable]
public class RandomFloat
{
    public float min;// { get; private set; }
    public float max;// { get; private set; }
    public RandomFloat(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public float GetValue()
    {
        return UnityEngine.Random.Range(min, max);
    }

}