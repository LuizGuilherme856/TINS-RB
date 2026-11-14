using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerHealth_ActiveEffect", menuName = "Upgrades/ActiveEffects/PlayerHealth")]

public class PlayerHealth_ActiveEffect : ActiveEffect
{
    [Header("Altera o HP do player\n" +
        "Valores negativos causam dano no player e valores positivos aumentam a vida do player")]
    [Range(-10, 10)][SerializeField] private int addToPlayerHealth = 0;
    public override void TriggerActiveEffect(UpgradeManager upgradeManager, GlobalStats globalStats)
    {
        if(addToPlayerHealth > 0)
        {
            upgradeManager.playerHolder.GetPlayerHealth().ReplenishHealth(addToPlayerHealth);
        }
        else
        {

            Debug.Log("Dano ao player: " + addToPlayerHealth);
            upgradeManager.playerHolder.GetPlayerGameObject().GetComponent<HandleAttackOnPlayer>().DamagePlayer(-addToPlayerHealth, true);
        }

    }

}
