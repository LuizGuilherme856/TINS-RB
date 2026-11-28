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
        gunInventory.BeginCampaignLoadout(startingPistol);
        if (catalog != null && catalog.skills != null && profile != null)
        {
            foreach (CampaignSkill skill in catalog.skills)
            {
                if (skill == null) continue;
                if (skill.effectType != CampaignSkillEffectType.WeaponUnlock || profile.Rank(skill.id) <= 0) continue;
                GunData weapon = skill.GetUnlockedWeapon();
                if (weapon == null)
                {
                    Debug.LogWarning($"Configure a arma desbloqueada por {skill.displayName}.", skill);
                    continue;
                }
                if (gunInventory.HasThisGun(weapon)) continue;

                gunInventory.GetGuns().Add(weapon);
            }
        }
        gunInventory.EnsureStartingGunEquipped(startingPistol);
    }
}
