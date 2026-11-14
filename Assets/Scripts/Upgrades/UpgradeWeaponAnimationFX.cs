using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Animation-event wrappers.

public class UpgradeWeaponAnimationFX : MonoBehaviour
{
    [SerializeField] private UpgradeWeaponFeedbacks upgradeWeaponFeedbacks;
    [SerializeField] private MMF_Player weaponTransformingFeedback, weaponIsReadyFeedback, startTransforminFeedback, runeExplosionFeedback;


    private Animator myAnimator;

    public event Action OnWeaponIsReady;

    private void Awake()
    {
        myAnimator= GetComponent<Animator>();
    }
    public void TriggerChangeGunSprite()
    {
        upgradeWeaponFeedbacks.ChangeWeaponSpriteToGoldenWeapon();
    }

    public void PlayWeaponTransformingFeedback()
    {
        weaponTransformingFeedback.PlayFeedbacks();
    }

    public void PlayStartTransformingFeedback()
    {
        startTransforminFeedback.PlayFeedbacks();
    }

    public void PlayWeaponIsReadyFeedback()
    {
        runeExplosionFeedback.PlayFeedbacks()
;        weaponIsReadyFeedback.PlayFeedbacks();
        OnWeaponIsReady?.Invoke();
    }

    public void PlayUpgradeAnimation()
    {
        myAnimator.SetTrigger("Upgrade");
    }
}
