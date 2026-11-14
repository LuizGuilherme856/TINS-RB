using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Shield_PowerUp : MonoBehaviour
{
    [SerializeField] private ShieldSource shieldSource; //Keep shield sources separate so simultaneous shields do not interfere.
    [SerializeField] private GameObject shield;
    [SerializeField] private Health playerHealth;
    
    [SerializeField] private MMF_Player disappearingFeedback;

    private float timer;
    private float shieldDuration;

    private void Start()
    {
        DeactivateShield();
    }

    private void FixedUpdate()
    {
        if(timer > 0)
        {
            timer -= Time.fixedDeltaTime;

            if (timer < shieldDuration * 0.25 && disappearingFeedback.IsPlaying == false) 
                disappearingFeedback.PlayFeedbacks();

            if(timer <= 0) DeactivateShield();
        }
    }

    private void SetInvulnerable(bool invulnerable)
    {
        switch (shieldSource)
        {
            case ShieldSource.powerUp:
                playerHealth.invulnerableShield = invulnerable;
                break;
            case ShieldSource.dash:
                playerHealth.invulnerable_dash = invulnerable;
                break;
                case ShieldSource.rune:
                playerHealth.invulnerableRune = invulnerable;
                break;
            default:
                break;
        }
    }

    //public void ActivateShield(float duration)
    //{
    //    timer = shieldDuration = duration;
    //    SetInvulnerable(true);
    //    shield.SetActive(true);
    //}

    public void ActivateShield(float time)
    {
        timer = time;
        shieldDuration = time;
        SetInvulnerable(true);
        shield.SetActive(true);
    }

    public void DeactivateShield()
    {
        disappearingFeedback.StopFeedbacks();
        SetInvulnerable(false);
        shield.SetActive(false);
    }

    private enum ShieldSource { powerUp, dash, rune}

}
