using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Duck_Behaviour : NPC_Behaviour
{
    [SerializeField] BuyRandomWeapon buyRandomWeapon;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (buyRandomWeapon != null)
        {
            buyRandomWeapon.OnBuyRandomWeapon += HandleBuyRandomWeapon;
        }
    }

    protected override void OnDisable()
    {
        if (buyRandomWeapon != null)
        {
            buyRandomWeapon.OnBuyRandomWeapon -= HandleBuyRandomWeapon;
        }

        base.OnDisable();
    }

    private void HandleBuyRandomWeapon()
    {
        RemoveMessage(-30f); //Wait 15 seconds before showing another message.
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") == false) return;
       
        switch (buyRandomWeapon.currentState)
        {
            case BuyRandomGunState.waiting:
                if (useAnimator) animator.SetTrigger("playerIsClose");
                playerIsCloseFeedback.PlayFeedbacks();
                playerIsClose = true;
                //buyRandomWeapon.UpdateUiText();
                break;
            case BuyRandomGunState.raffling:
                break;
            case BuyRandomGunState.weaponReady:
                break;
            default:
                break;
        }

    }

    public override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") == false) return;
        
        switch (buyRandomWeapon.currentState)
        {
            case BuyRandomGunState.waiting:
                playerIsClose = false;
                if (useAnimator) animator.SetTrigger("playerIsNotCloseAnymore");
                playerIsNotCloseAnymoreFeedback.PlayFeedbacks();
                RemoveMessage(0);
                
                break;
            case BuyRandomGunState.raffling:
                break;
            case BuyRandomGunState.weaponReady:
                break;
            default:
                break;
        }
    }
}
