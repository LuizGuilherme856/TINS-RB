using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Prevents the player from taking damage during restart.

public class SetPlayerInvulnerable : MonoBehaviour
{
    [SerializeField] private PlayerHolder playerHolder;
     private Health playerHealth;

    private void Awake()
    {
        playerHealth = playerHolder.GetPlayerHealth();
        if (playerHealth == null)
        {
            playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
            Debug.LogError("PLAYER HEALTH NAO ENCONTRADO no awake");
        }
    }

    private void OnEnable()
    {
        if (playerHealth == null)
        {
            playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
            Debug.LogError("PLAYER HEALTH NAO ENCONTRADO no enable");
        }
        

        
    }


    public void SetPlayerInvulnerableMethod()
    {
        playerHealth.SetInvulnerableTime(60);//Temporary invulnerability prevents post-death damage.
    }

}
