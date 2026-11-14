using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Display enemy-defeated text.
/// </summary>
public class EnemySlainText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TextMeshProUGUI;
     private BossFightArena bossFightArena;

    private void OnEnable()
    {
        bossFightArena = GetComponent<BossFightArena>();
        bossFightArena.OnEndBossFight += ShowText;
    }


    private void ShowText()
    {
        TextMeshProUGUI.text = "ENEMY SLAIN";
        TextMeshProUGUI.gameObject.GetComponent<Animator>().SetTrigger("Play");
    }
}
