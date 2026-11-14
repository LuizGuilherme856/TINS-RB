///Display general interaction text, such as weapon and upgrade purchase prompts.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GeneralTextBoxUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textBox;
    

    public void SetText(string text)
    {
        textBox.text = text;
    }

    public void RemoveText()
    {
        textBox.text = null;
    }
}
