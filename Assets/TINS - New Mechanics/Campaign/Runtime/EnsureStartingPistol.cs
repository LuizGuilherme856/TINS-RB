using UnityEngine;
using TINS.Campaign;

/// <summary>Ensures the player begins with the configured Simple Pistol equipped.</summary>
public sealed class EnsureStartingPistol : MonoBehaviour
{
    [SerializeField] private GunInventory gunInventory;
    [SerializeField] private GunData startingPistol;

    public void Equip(CampaignCatalog catalog = null, CampaignProfile profile = null)
    {
        if (gunInventory == null) gunInventory = GetComponent<GunInventory>();
        if (gunInventory == null || startingPistol == null) return;
        gunInventory.ResetGuns(startingPistol);
        if (catalog != null && catalog.skills != null && profile != null)
        {
            foreach (CampaignSkill skill in catalog.skills)
            {
                if (skill == null) continue;
                if (string.IsNullOrWhiteSpace(skill.unlockedWeaponResourcePath) || profile.Rank(skill.id) <= 0) continue;
                GunData weapon = Resources.Load<GunData>(skill.unlockedWeaponResourcePath);
                if (weapon == null)
                {
                    Debug.LogWarning($"Arma da campanha ausente: {skill.unlockedWeaponResourcePath}", this);
                    continue;
                }
                if (gunInventory.HasThisGun(weapon)) continue;

                weapon.SetFullAmmo();
                gunInventory.GetGuns().Add(weapon);
            }
        }
        gunInventory.EnsureStartingGunEquipped(startingPistol);
    }
}
