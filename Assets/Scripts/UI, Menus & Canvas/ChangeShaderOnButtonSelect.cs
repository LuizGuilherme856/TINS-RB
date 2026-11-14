using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangeShaderOnButtonSelect : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image image;
    [SerializeField] private ChangeOnSelect changeOnSelect = ChangeOnSelect.shaderOnly;
    [SerializeField] private Material selectedMaterial, notSelectedMaterial;
    [SerializeField] private Sprite selectedImage, notSelectedImage;
    //private const string COLOR_REPLACE_FADE_NAME = "_ColorReplaceFade", RECOLOR_FADE_NAME = "_RecolorFade";

    /// <summary>
    /// Change the button shader/material or image according to changeOnSelect.
    /// </summary>
    /// <param name="isSelected"></param>
    private void SelectButtonShader(bool isSelected)
    {
        if (isSelected)
        {
            switch (changeOnSelect)
            {
                case ChangeOnSelect.shaderOnly:
                    image.material = selectedMaterial;
                    break;
                case ChangeOnSelect.imageOnly:
                    image.sprite = selectedImage;
                    break;
                case ChangeOnSelect.imageAndShader:
                    image.sprite = selectedImage;
                    image.material = selectedMaterial;
                    break;
                default:
                    break;
            }

        }
        else
        {

            switch (changeOnSelect)
            {
                case ChangeOnSelect.shaderOnly:
                    image.material = notSelectedMaterial;
                    break;
                case ChangeOnSelect.imageOnly:
                    image.sprite = notSelectedImage;
                    break;
                case ChangeOnSelect.imageAndShader:
                    image.sprite = notSelectedImage;
                    image.material = notSelectedMaterial;
                    break;
                default:
                    break;
            }
            
        }
    }

    private void OnDisable()
    {
        SelectButtonShader(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnSelect(BaseEventData eventData)
    {
        SelectButtonShader(true);
        // Debug.Log(this.gameObject.name + " was selected");
    }

    public void OnDeselect(BaseEventData eventData)
    {
        SelectButtonShader(false);
    }

    private enum ChangeOnSelect { shaderOnly, imageOnly, imageAndShader }
}
