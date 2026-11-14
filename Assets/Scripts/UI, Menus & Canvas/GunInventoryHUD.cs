using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Show inventory weapons beside the equipped weapon in the HUD.
/// </summary>
public class GunInventoryHUD : MonoBehaviour
{
    [SerializeField] private PlayerHolder playerHolder;
    [SerializeField] private GameObject extraGunPanelPrefab;
    [SerializeField] private Transform extraGunScrollViewContent;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    private GunInventory gunInventory;
    private List<ExtraGunPanelHUD> extraGunHUDList = new List<ExtraGunPanelHUD>();
    private List<GunData> gunDataList = new List<GunData>();

    private void Awake()
    {
        gunInventory = playerHolder.GetGunInventory();
        extraGunHUDList.Clear();
        gunDataList.Clear();
    }

    private void OnEnable()
    {
        gunInventory.OnWeaponChange += UpdateGuns;

        if (gunInventory.GetMaxGuns() > extraGunHUDList.Count)
        {
            for (int i = 0; i < (gunInventory.GetMaxGuns() - extraGunHUDList.Count); i++)
            {
                var newPanel = Instantiate(extraGunPanelPrefab, extraGunScrollViewContent);
                extraGunHUDList.Add(newPanel.GetComponent<ExtraGunPanelHUD>());
            }


        }
    }

    private void UpdateGuns()
    {
        gunDataList = gunInventory.GetGuns();
        //Exclude the one equipped weapon.
        if(gunInventory.GetMaxGuns() - 1 > extraGunHUDList.Count)
        {
            //Add a panel.
            var newPanel = Instantiate(extraGunPanelPrefab, extraGunScrollViewContent);
            extraGunHUDList.Add(newPanel.GetComponent<ExtraGunPanelHUD>());
        }

        //Remove the equipped weapon from the extra-weapon list.
        var gunsListNotEquiped = gunDataList.Where(x => !x.Equals(gunInventory.GetCurrentEquipedGun())).ToList();

        for (int i = 0; i < extraGunHUDList.Count; i++)
        {

            extraGunHUDList[i].gameObject.transform.SetSiblingIndex( i);

            if(i >= gunsListNotEquiped.Count )
            {
                extraGunHUDList[i].ChangeGunIcon(null);
            }
            else
            {
                extraGunHUDList[i].ChangeGunIcon(gunsListNotEquiped[i].buyIcon);
            }

        }

        //Adjust panel spacing to the number of weapons;
        //use tighter spacing for larger inventories.
        switch (extraGunHUDList.Count)
        {
            case 2:
                gridLayoutGroup.spacing = new Vector2(45, 0);
                break;
            case 3:
                gridLayoutGroup.spacing = new Vector2(45, 0);
                break;
            case 4:
                gridLayoutGroup.spacing = new Vector2(12, 0);
                break;
            case 5:
                gridLayoutGroup.spacing = new Vector2(-15, 0);
                break;
            case 6:
                gridLayoutGroup.spacing = new Vector2(-22, 0);
                break;
            default:
                gridLayoutGroup.spacing = new Vector2(-40, 0);
                break;
        }

    }


}
