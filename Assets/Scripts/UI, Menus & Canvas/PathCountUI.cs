using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Show the owned rune count for each path in the inventory.
/// </summary>
public class PathCountUI : MonoBehaviour
{
    [SerializeField] private RuneInventory runeInventory;
    [SerializeField] private PathsOfPowerNames path;
    [SerializeField] private TextMeshProUGUI quantityText;
    private void OnEnable()
    {
        int quantity = runeInventory.GetPathQuantity(path);
        quantityText.text = quantity.ToString();
    }
}
