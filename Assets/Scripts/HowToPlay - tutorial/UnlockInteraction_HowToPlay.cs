using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockInteraction_HowToPlay : MonoBehaviour
{
    [SerializeField] private InteractionType interactionType;
    [SerializeField] private ClosedDoor closedDoor;
    [SerializeField] private BuyWeapon buyWeapon;
    [SerializeField] private UpgradeWeapon upgradeWeapon;

    public void Unlock()
    {
        switch (interactionType)
        {
            case InteractionType.openDoor:
                closedDoor.canOpen = true;
                break;
            case InteractionType.buyWeapon:
                buyWeapon.canBuy = true;
                break;
            case InteractionType.upgradeWeapon:
                upgradeWeapon.canBuy = true;
                break;
            default:
                break;
        }

    }

    private enum InteractionType { openDoor, buyWeapon, upgradeWeapon }
}


