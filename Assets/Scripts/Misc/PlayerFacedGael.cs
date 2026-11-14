using UnityEngine;

[RequireComponent(typeof(BossFightArena))]
/// This script sets a flag in PlayerData when the player starts the boss fight against Gael
public class PlayerFacedGael : MonoBehaviour
{
    [SerializeField] PlayerData playerData;
    BossFightArena bossFightArena;

    private void Awake()
    {
        bossFightArena = GetComponent<BossFightArena>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(playerData.playerFacedGael <= 1)
        {
            bossFightArena.OnStartBossFight += () => playerData.playerFacedGael = 1;
        }

    }


    
}
