using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class SetKillCountToTxt : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private TextMeshProUGUI[] texts;

    void Start()
    {
        foreach (var txt in texts)
        {
            txt.text = playerData.GetTotalKills().ToString();
        }
        
    }


}
