using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Expose image-color changes through a public method.
/// </summary>
public class ChangeImageColor : MonoBehaviour
{
    [SerializeField] private Image imageToChangeColor;
    [SerializeField] private Color defaultColor, newColor;
  

    public void SetDefaultColor()
    {
        imageToChangeColor.color = defaultColor;
    }

    public void SetNewColor()
    {
        imageToChangeColor.color = newColor;
    }
}
