using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdatePlayerHornsQuantity : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI quantityTxt;
    [SerializeField] private PlayerData playerData;

    private void OnEnable()
    {
        UpdateQuantity();
        playerData.OnHornsChange += UpdateQuantity;
    }

    private void OnDisable()
    {
        playerData.OnHornsChange -= UpdateQuantity;
    }

    public void RefreshQuantity()
    {
        UpdateQuantity();
    }

    private void UpdateQuantity()
    {
        quantityTxt.text = playerData.horns.ToString();
    }

}
