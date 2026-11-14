using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHideCheats : MonoBehaviour
{

    [SerializeField] private PlayerData playerData;
    [SerializeField] private GameObject cheatsGameObject;
    private void Start()
    {
        if(playerData.showCheats)
        {
            cheatsGameObject.SetActive(true);
        }
        else
        {
            cheatsGameObject.SetActive(false);
        }
    }
}
