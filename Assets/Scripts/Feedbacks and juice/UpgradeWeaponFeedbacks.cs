using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TechBooh;
using UnityEngine;

//Helper component extracted to keep the main class smaller.
public class UpgradeWeaponFeedbacks : MonoBehaviour
{
    [SerializeField] private MMF_Player cantBuyFeedback, buyFeedback, takeWeaponFeedback;
    [SerializeField] private SpriteRenderer weaponSprite;
    [SerializeField] private UpgradeWeaponAnimationFX upgradeWeaponAnimationFX;

    public Animator lightAnimator;
    private Animator statueAnimator;
    private Sprite goldenWeaponSprite;

    void Awake()
    {
        statueAnimator = Try.GetComponent<Animator>(gameObject);
    }
    private void Start()
    {
        weaponSprite.sprite = null;
    }

    public void PlayCantBuyFeedback()
    {
        cantBuyFeedback.PlayFeedbacks();
    }

    public void PlayBuyFeedback()
    {
        statueAnimator.SetTrigger("Play");
        buyFeedback.PlayFeedbacks();
    }

    public void PlayLightAnimation_Animation()
    {
        lightAnimator.SetTrigger("Play");
    }

    public void PlayTakeWeaponFeedback()
    {
        takeWeaponFeedback.PlayFeedbacks();
    }


    public void SetWeaponSprite(Sprite weaponSprite, Sprite goldenWeaponSprite)
    {
        this.weaponSprite.sprite = weaponSprite;
        this.goldenWeaponSprite = goldenWeaponSprite;
    }

    public void ChangeWeaponSpriteToGoldenWeapon()
    {
        this.weaponSprite.sprite = goldenWeaponSprite;
    }

}
