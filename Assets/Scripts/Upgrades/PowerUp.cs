using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] private PowerUpType powerUpType;
    private HandlePowerUpEffects handlePowerUpEffects;
    [SerializeField] private MMF_Player feedbacks;
    [SerializeField] private GameObject drop;
    [SerializeField] private CircleCollider2D circleCollider2D;
    public GlobalStats globalStats;
    public CircleCollider2D nearPlayerDetector;

    [SerializeField] private bool deactivateOnPickUp = false; //Used by pooled Survival pickups.

    private bool hasBeenCollected = false;

    public event Action OnPowerUpPickUp;

    private void OnEnable()
    {
        hasBeenCollected = false;
        nearPlayerDetector.radius = 5 * globalStats.ForCurrentRun.powerUpGoTowardsPlayerMaxDistanceMultiplier;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(hasBeenCollected) return;

        if (collision.CompareTag("Player"))
        {
            OnPowerUpPickUp?.Invoke();

            handlePowerUpEffects = collision.gameObject.GetComponentInParent<HandlePowerUpEffects>();
            hasBeenCollected = true;
            HandlePowerUp();
            feedbacks.PlayFeedbacks();

            if(deactivateOnPickUp)
            {
                gameObject.SetActive(false);
            }
            else
            {
                circleCollider2D.enabled= false;
                drop.SetActive(false);

            }
            //Destroy(drop);

        }
    }


    protected virtual void HandlePowerUp()
    {
        switch (powerUpType)
        {
            case PowerUpType.refillAmmo:
                handlePowerUpEffects.RefillAmmo();
                break;
            case PowerUpType.extraHP:
                handlePowerUpEffects.ExtraLife();
                break;
            case PowerUpType.points:
                handlePowerUpEffects.ExtraPoints();
                break;
            case PowerUpType.horn:
                handlePowerUpEffects.AddHorn();
                break;
            case PowerUpType.shield:
                handlePowerUpEffects.Shield();
                break;
            case PowerUpType.instaKillMelee:
                handlePowerUpEffects.InstaKillMelee();
                break;
            case PowerUpType.instaKill:
                handlePowerUpEffects.InstaKill();
                break;
            case PowerUpType.doublePoints:
                handlePowerUpEffects.DoublePoints();
                break;
            case PowerUpType.survivalDrop:
                handlePowerUpEffects.SurvivalXpDrop();
                break;
            default:
                break;
        }
    }

    public enum PowerUpType { refillAmmo, extraHP, points, horn, shield, instaKillMelee, instaKill, doublePoints, survivalDrop }
}


