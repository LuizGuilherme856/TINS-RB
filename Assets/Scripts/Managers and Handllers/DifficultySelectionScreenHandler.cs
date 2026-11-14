using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("This script is deprecated. ")]
public class DifficultySelectionScreenHandler : MonoBehaviour
{
    [SerializeField] private GameObject rootButtons, difficultyButtons;
    [SerializeField] private GameObject startButton, classicButton; //Default selected buttons.

    [SerializeField] private RunData runData;

    private SelectEventSystemButton selectEventSystemButton;

    private void Awake()
    {
        selectEventSystemButton = FindObjectOfType<SelectEventSystemButton>();
    }

    public void ShowRootButtons()
    {
        rootButtons.SetActive(true);
        difficultyButtons.SetActive(false);
        selectEventSystemButton.SetButtonToSelect(startButton);
    }

    public void ShowDifficultyButtons()
    {
        difficultyButtons.SetActive(true);
        rootButtons.SetActive(false);
        selectEventSystemButton.SetButtonToSelect(classicButton);
    }

    public void SetDifficulty(bool isClassic)
    {
        //runData.classicDifficulty = isClassic;
    }

}
