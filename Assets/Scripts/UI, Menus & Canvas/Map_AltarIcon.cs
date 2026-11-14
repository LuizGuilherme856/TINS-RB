using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Map_AltarIcon : MonoBehaviour
{
    [SerializeField] ManageSacrifice sacrificeToObserve;
    [SerializeField] private Image sacrificeImage;
    [SerializeField] private Sprite sacrificeCompleteIcon;

    private void Start()
    {
        sacrificeToObserve.OnSacrificeComplete += ChangeImage;
    }
    private void OnEnable()
    {
        if (sacrificeToObserve.IsCompleted())
        {
            ChangeImage();
        }
    }

    private void ChangeImage()
    {
        sacrificeImage.sprite = sacrificeCompleteIcon;
    }
}
