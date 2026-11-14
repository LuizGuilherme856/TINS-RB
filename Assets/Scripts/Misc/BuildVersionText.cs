using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuildVersionText : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] TextMeshProUGUI text_tmp;

    private void OnEnable()
    {
        text_tmp.text = playerData.version;
    }

}
