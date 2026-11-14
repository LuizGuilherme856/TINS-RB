using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToggleEnemySpawn_TestScene : InteractWithPlayer
{
    [SerializeField] EnemyWaveManager hordeManager;
    [SerializeField] private TextMeshProUGUI text;

    private void Start()
    {
        UpdateText();
    }

    protected override void StartInteraction()
    {
        base.StartInteraction();

        if(hordeManager.canEnemySpawn == false)
        {
            generalTextBoxUI.SetText( interactionButton + " Enable enemy spawn");
        }
        else
        {
            generalTextBoxUI.SetText(interactionButton + " Disable enemy spawn");
        }

        
    }

    protected override void InteractionAction()
    {
        hordeManager.ToggleEnemySpawn();
        buyFeedback.PlayFeedbacks();
        UpdateText();
    }

    private void UpdateText()
    {
        if (hordeManager.canEnemySpawn == false)
        {
            text.text = "Spawn Disabled";
        }
        else
        {
            text.text = "Spawn Enabled";
        }
    }
}
