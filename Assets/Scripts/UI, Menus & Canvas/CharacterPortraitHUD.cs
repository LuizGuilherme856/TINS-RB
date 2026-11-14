using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPortraitHUD : MonoBehaviour
{
    private Image image;
    [Space(15)]
    [SerializeField] RunData runData;


    private void Awake()
    {
        image = GetComponent<Image>();
    }
    private void Start()
    {
        UpdateCharacterPortrait();
    }

    private void UpdateCharacterPortrait()
    {
        image.sprite = runData.currentCharacter.dialoguePortrait;
    }




}
