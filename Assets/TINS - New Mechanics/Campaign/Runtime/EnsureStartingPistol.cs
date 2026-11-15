using UnityEngine;

/// <summary>Ensures the player begins with the configured Simple Pistol equipped.</summary>
public sealed class EnsureStartingPistol : MonoBehaviour
{
    [SerializeField] private GunInventory gunInventory;
    [SerializeField] private GunData startingPistol;

    public void Equip()
    {
        if (gunInventory == null) gunInventory = GetComponent<GunInventory>();
        if (gunInventory == null || startingPistol == null) return;
        gunInventory.ResetGuns(startingPistol);
        gunInventory.EnsureStartingGunEquipped(startingPistol);
        startingPistol.SetAmmoOnClip(startingPistol.GetMaxAmmoOnClip());
    }
}
