using UnityEngine;

/// <summary>Ensures the player begins with the configured Simple Pistol equipped.</summary>
[DefaultExecutionOrder(800)]
public sealed class EnsureStartingPistol : MonoBehaviour
{
    [SerializeField] private GunInventory gunInventory;
    [SerializeField] private GunData startingPistol;

    private void Start()
    {
        if (gunInventory == null) gunInventory = GetComponent<GunInventory>();
        if (gunInventory == null || startingPistol == null) return;
        // Campaign weapons are granted from the saved skill ranks after this runs.
        // Clear any random or leftover run weapons so the new campaign always starts with the Simple Pistol.
        gunInventory.ResetGuns(startingPistol);
        gunInventory.EnsureStartingGunEquipped(startingPistol);
    }
}
