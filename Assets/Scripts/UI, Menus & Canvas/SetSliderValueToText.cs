using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetSliderValueToText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textTmp;

    public void UpdateText(float text) //Called by the slider.
    {
        textTmp.text = text.ToString();
    }


}
