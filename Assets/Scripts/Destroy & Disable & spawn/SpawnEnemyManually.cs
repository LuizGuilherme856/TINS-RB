using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpawnEnemyManually : InteractWithPlayer
{
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private EnemyType enemyType;
    [SerializeField] private int amount = 1;
    [SerializeField] private TextMeshProUGUI text;

    private void Start()
    {
        text.text = enemyType.ToString();
    }
    protected override void StartInteraction()
    {
        base.StartInteraction();
        generalTextBoxUI.SetText("Spawn " + enemyType.ToString());
    }

    protected override void InteractionAction()
    {
        base.InteractionAction();

        for (int i = 0; i < amount; i++)
        {
            spawnManager.SpawnEnemy(enemyType, true);

        }
        buyFeedback.PlayFeedbacks();
    }
}
