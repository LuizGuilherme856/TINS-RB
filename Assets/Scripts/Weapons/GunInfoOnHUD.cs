using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;

/// <summary>
/// Displays weapon information and the campaign's pixel-art weapon HUD.
/// </summary>
public class GunInfoOnHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoOnclipText, spareAmmoText, gunNameText;
    [SerializeField] private UnityEngine.UI.Image gunIcon;
    [SerializeField] private Sprite emptySprite;

    [Header("Campaign pixel HUD")]
    [SerializeField] private GameObject legacyWeaponHud;
    [SerializeField] private GameObject campaignHudRoot;
    [SerializeField] private UnityEngine.UI.Image campaignWeaponImage;
    [SerializeField] private UnityEngine.UI.Image[] magazineSlots;
    [SerializeField] private Sprite[] bulletFrames;
    [SerializeField, Min(1)] private int slotsPerRow = 6;
    [SerializeField, Min(1f)] private float extraRowSpacing = 40f;
    [SerializeField, Min(0.01f)] private float bulletAnimationFrameDuration = 0.045f;

    [Header("JUICE")]
    [SerializeField] private MMF_Player changeWeaponFeedbacks;

    [Header("OTHER")]
    [SerializeField] private PlayerHolder playerHolder;
    [SerializeField] private LanguageData languageData;

    private GunInventory gunInventory;
    private GunData currentGun;
    private GunAmmo currentAmmo;
    private Sprite simplePistolSprite;
    private readonly List<UnityEngine.UI.Image> bulletSlots = new List<UnityEngine.UI.Image>();
    private readonly List<float> shotTimes = new List<float>();
    private int previousAmmo = -1;
    private int previousMaxAmmo = -1;
    private bool campaignHudActive;

    private void Awake()
    {
        if (campaignWeaponImage != null) simplePistolSprite = campaignWeaponImage.sprite;
    }

    private void OnEnable()
    {
        SetCampaignHudActive(ShouldShowCampaignHud());
        BindGunInventory();
    }

    private void Update()
    {
        if (gunInventory == null) BindGunInventory();

        bool shouldShowCampaignHud = ShouldShowCampaignHud();
        if (campaignHudActive != shouldShowCampaignHud)
            SetCampaignHudActive(shouldShowCampaignHud);

        if (campaignHudActive) DrawMagazine();
    }

    private bool ShouldShowCampaignHud() =>
        campaignHudRoot != null && magazineSlots != null && magazineSlots.Length > 0
        && bulletFrames != null && bulletFrames.Length > 0
        && TINS.Campaign.CampaignCombatRules.IsCampaignMission;

    private void OnDisable()
    {
        UnbindGunInventory();
        if (currentAmmo != null) currentAmmo.Changed -= RefreshCurrentAmmo;
        currentAmmo = null;
        currentGun = null;
        SetCampaignHudActive(false);
    }

    private void BindGunInventory()
    {
        if (gunInventory != null) return;

        if (playerHolder == null) playerHolder = FindFirstObjectByType<PlayerHolder>();
        if (playerHolder != null) gunInventory = playerHolder.GetGunInventory();

        if (gunInventory == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) gunInventory = player.GetComponent<GunInventory>();
        }

        if (gunInventory == null) return;

        gunInventory.OnWeaponChange += RefreshCurrentGun;
        gunInventory.OnWeaponChange += RefreshCurrentAmmo;
        gunInventory.OnWeaponChange += RefreshGunIconAndName;
        RefreshCurrentGun();
        RefreshCurrentAmmo();
        RefreshGunIconAndName();
    }

    private void UnbindGunInventory()
    {
        if (gunInventory == null) return;
        gunInventory.OnWeaponChange -= RefreshCurrentGun;
        gunInventory.OnWeaponChange -= RefreshCurrentAmmo;
        gunInventory.OnWeaponChange -= RefreshGunIconAndName;
        gunInventory = null;
    }

    private void SetCampaignHudActive(bool active)
    {
        campaignHudActive = active;

        if (legacyWeaponHud != null) legacyWeaponHud.SetActive(!active);
        if (campaignHudRoot != null) campaignHudRoot.SetActive(active && currentGun != null);

        RefreshGunIconAndName();
        RefreshCurrentAmmo();
    }

    private void RefreshGunIconAndName()
    {
        if (currentGun == null)
        {
            if (gunIcon != null) gunIcon.sprite = emptySprite;
            if (gunNameText != null) gunNameText.text = "";
            if (campaignHudRoot != null) campaignHudRoot.SetActive(false);
            return;
        }

        if (campaignHudActive)
        {
            if (campaignWeaponImage != null)
            {
                bool isSimplePistol = gunInventory != null && gunInventory.IsStartingGun(currentGun);
                campaignWeaponImage.sprite = isSimplePistol
                    && simplePistolSprite != null
                    ? simplePistolSprite
                    : (currentGun.buyIcon != null ? currentGun.buyIcon : currentGun.sprite);
                campaignWeaponImage.enabled = campaignWeaponImage.sprite != null;
            }
            if (campaignHudRoot != null) campaignHudRoot.SetActive(true);
            return;
        }

        if (gunIcon != null) gunIcon.sprite = currentGun.buyIcon;
        if (gunNameText != null && languageData != null) gunNameText.text = currentGun.GetName(languageData.currentLanguage);
        changeWeaponFeedbacks?.PlayFeedbacks();
    }

    private void RefreshCurrentAmmo()
    {
        if (currentAmmo == null)
        {
            if (ammoOnclipText != null) ammoOnclipText.text = "- -";
            if (spareAmmoText != null) spareAmmoText.text = "- -";
            ClearBulletSlots();
            return;
        }

        if (ammoOnclipText != null) ammoOnclipText.text = currentAmmo.Rounds.ToString();
        if (spareAmmoText != null)
            spareAmmoText.text = TINS.Campaign.CampaignCombatRules.IsCampaignMission
                ? "âˆž"
                : currentAmmo.Reserve.ToString();

        if (campaignHudActive) RefreshCampaignMagazine();
    }

    private void RefreshCampaignMagazine()
    {
        int maxAmmo = Mathf.Max(1, currentGun.GetMaxAmmoOnClip());
        int ammo = Mathf.Clamp(currentAmmo.Rounds, 0, maxAmmo);
        if (maxAmmo != previousMaxAmmo) RebuildBulletSlots(maxAmmo);

        if (previousAmmo > ammo)
        {
            for (int slot = ammo; slot < previousAmmo; slot++)
                shotTimes[slot] = Time.time;
        }
        else if (ammo > previousAmmo)
        {
            for (int slot = 0; slot < ammo; slot++)
                shotTimes[slot] = float.NegativeInfinity;
        }

        previousAmmo = ammo;
        DrawMagazine();
    }

    private void DrawMagazine()
    {
        float now = Time.time;
        float frameDuration = Mathf.Max(0.01f, bulletAnimationFrameDuration);
        int lastFrame = bulletFrames.Length - 1;
        for (int i = 0; i < bulletSlots.Count; i++)
        {
            int frame = lastFrame;
            if (i < previousAmmo)
                frame = 0;
            else if (now - shotTimes[i] < lastFrame * frameDuration)
                frame = Mathf.FloorToInt((now - shotTimes[i]) / frameDuration);

            Sprite sprite = bulletFrames[frame];
            if (bulletSlots[i].sprite != sprite) bulletSlots[i].sprite = sprite;
        }
    }

    private void RebuildBulletSlots(int count)
    {
        ClearBulletSlots();
        previousAmmo = -1;
        previousMaxAmmo = count;

        for (int i = 0; i < magazineSlots.Length; i++)
            magazineSlots[i].gameObject.SetActive(i < count);

        int columns = Mathf.Clamp(slotsPerRow, 1, magazineSlots.Length);
        int lastAuthoredRow = (magazineSlots.Length - 1) / columns;
        for (int i = 0; i < count; i++)
        {
            UnityEngine.UI.Image image;
            if (i < magazineSlots.Length)
            {
                image = magazineSlots[i];
            }
            else
            {
                int extraIndex = i - magazineSlots.Length;
                UnityEngine.UI.Image template = magazineSlots[extraIndex % columns];
                image = Instantiate(template, template.transform.parent, false);
                image.name = $"Round {i + 1:00}";
                Vector2 offset = Vector2.up * ((lastAuthoredRow + 1 + extraIndex / columns) * extraRowSpacing);
                image.rectTransform.anchoredPosition = template.rectTransform.anchoredPosition + offset;
                image.gameObject.SetActive(true);
            }

            bulletSlots.Add(image);
            shotTimes.Add(float.NegativeInfinity);
        }
    }

    private void ClearBulletSlots()
    {
        // The first slots belong to the prefab. Only additional rounds are temporary.
        int authoredCount = magazineSlots?.Length ?? 0;
        for (int i = authoredCount; i < bulletSlots.Count; i++)
        {
            if (bulletSlots[i] == null) continue;
            bulletSlots[i].gameObject.SetActive(false);
            Destroy(bulletSlots[i].gameObject);
        }

        bulletSlots.Clear();
        shotTimes.Clear();
    }

    private void RefreshCurrentGun()
    {
        if (currentAmmo != null) currentAmmo.Changed -= RefreshCurrentAmmo;
        currentGun = gunInventory != null ? gunInventory.GetCurrentEquipedGun() : null;
        currentAmmo = gunInventory != null ? gunInventory.GetAmmo(currentGun) : null;
        previousAmmo = -1;
        previousMaxAmmo = -1;

        if (currentAmmo != null) currentAmmo.Changed += RefreshCurrentAmmo;
        if (campaignHudRoot != null) campaignHudRoot.SetActive(campaignHudActive && currentGun != null);
    }
}
