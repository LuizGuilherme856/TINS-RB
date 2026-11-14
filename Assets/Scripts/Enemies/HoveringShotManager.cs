using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Hovering shots remain in front of the player,
/// as with a flamethrower.
/// </summary>
public class HoveringShotManager : MonoBehaviour
{
    [SerializeField] private List<HoveringShot> hoveringShots;
    private HoveringShot currentHoveringShot;


    public void UpdateHoveringShot(GunData gunData)
    {
        for (int i = 0; i < hoveringShots.Count; i++)
        {
            if (string.Equals( hoveringShots[i].gameObject.name, gunData.hoveringShotName))
            {
                currentHoveringShot = hoveringShots[i];
            }
        }
    }

    public void StartShooting()
    {
        currentHoveringShot.gameObject.SetActive(true);
        currentHoveringShot.TriggerStart();
    }

    public void StopShooting()
    {
        if (currentHoveringShot == null) return;
        currentHoveringShot.TriggerFinish();
    }
}
