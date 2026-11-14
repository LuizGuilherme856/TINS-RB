using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Update HUD weapon icons in response to GunInventoryHUD.
/// </summary>
public class ExtraGunPanelHUD : MonoBehaviour
{
    [SerializeField] private Image gunImage, panelImage;
    [SerializeField] private MMF_Player feedbacks;
    // Start is called before the first frame update

    private void OnEnable()
    {
        SetImageNull();
    }

    public void ChangeGunIcon(Sprite gunSprite)
    {
        if (gunSprite == null)
        {
            SetImageNull();
            return;
        }

        bool sameSprite = gunSprite.Equals(gunImage.sprite);

        // Clearing a slot visually may leave its previous sprite assigned.
        // Restore colors even when the incoming sprite is unchanged,
        // or the icon remains invisible until another sprite refresh.
        gunImage.color = Color.white;
        panelImage.color = Color.white;

        if (sameSprite) return;

        gunImage.sprite = gunSprite;
        feedbacks.PlayFeedbacks();
    }

    private void SetImageNull()
    {
        gunImage.color = Color.clear;
        panelImage.color = new Color32(255, 255, 255, 120);
    }
}
