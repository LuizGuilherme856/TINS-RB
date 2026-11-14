using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowCharacterInfo_LoadoutScreen : MonoBehaviour
{
    [SerializeField] private GameObject[] charInfo;

    private void Start()
    {
        SetCharacterInfoVisible(0);
    }

    public void SetCharacterInfoVisible(int index) //Enable this panel and disable the others.
    {
        for (int i = 0; i < charInfo.Length; i++)
        {
            if (i == index) charInfo[i].SetActive(true);
            else charInfo[i].SetActive(false);
        }
    }
}
