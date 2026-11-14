using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CheatsHandler : MonoBehaviour
{

    [SerializeField] GameObject[] hudElements; 

    [SerializeField] private PlayerData playerData;
    [SerializeField] private RuneInventory runeUpgradeManager;
    [SerializeField] private PointManager pointManager;
    [SerializeField] private EnemyWaveManager hordeManager;
    [SerializeField] private PlayerHolder playerHolder;
    [SerializeField] private SaveGame saveGame;

    private Health playerHealth;
    private SpecialWeaponInventory specialWeaponInventory;

    [SerializeField] private MeleeData dash, decoy, arrow, trap, glassSword, azausDash;

    private void Awake()
    {
        // var playerGameObj = GameObject.FindGameObjectWithTag("Player");

        var playerGameObj = playerHolder.GetPlayerGameObject();

        playerHealth = playerHolder.GetPlayerHealth();
        //playerHealth = playerGameObj.GetComponent<Health>();
        specialWeaponInventory = playerGameObj.GetComponent<SpecialWeaponInventory>();
    }

    private void Start()
    {
        specialWeaponInventory ??= playerHolder.GetPlayerGameObject().GetComponent<SpecialWeaponInventory>();
    }

    /// <summary>
    /// Make the UI transparent without deactivating it.
    /// </summary>
    /// <param name="hide"></param>
    public void HideHUD(bool hide)
    {
        foreach (var item in hudElements)
        {
            // Update image alpha.
            var images = item.GetComponentsInChildren<Image>(true);
            foreach (var image in images)
            {
                var color = image.color;
                color.a = hide ? 0 : 1;
                image.color = color;
            }

            // Update text alpha.
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                float fontSize = text.fontSize;
                text.fontSize = hide ? 0 : fontSize; // Update font size.

                var color = text.color;
                color.a = hide ? 0 : 1;
                text.color = color;
            }
        }
    }

    public void SetImortal()
    {
        if (playerHealth.invulnerable == true)
            playerHealth.invulnerable = false;
        else
            playerHealth.invulnerable = true;
    }

    public void Add10Horns()
    {
        playerData.AddHorns(10);
    }

    public void SpecialAttackDash()
    {
        specialWeaponInventory.ChangeSpecialWeapon(dash);
    }

    public void SpecialAttackDecoy()
    {
        specialWeaponInventory.ChangeSpecialWeapon(decoy);
    }

    public void SpecialAttackArrow()
    {
        specialWeaponInventory.ChangeSpecialWeapon(arrow);
    }

    public void SpecialAttackTrap()
    {
        specialWeaponInventory.ChangeSpecialWeapon(trap);
    }

    public void SpecialAttackAzausDash()
    {
        specialWeaponInventory.ChangeSpecialWeapon(azausDash);
    }

    public void SpecialAttackGlassSword()
    {
        specialWeaponInventory.ChangeSpecialWeapon(glassSword);
    }

    public void AddOneHealthPoint()
    {
        playerHealth.ReplenishHealth(1);
    }

    public void InfinitePoints()
    {
        pointManager.CHEAT_AddPoint(10000);
    }

    public void ResetPoints()
    {
        pointManager.CHEAT_ResetPoints();
    }

    public void NextWave()
    {
        hordeManager.CHEAT_NextWave();
    }

    public void previousWave()
    {
        hordeManager.CHEAT_PreviousWave();
    }

    public void SetWave(int wave)
    {
        hordeManager.CHEAT_SetWave(wave);
    }


    public void Add5SkillPoints()
    {
        playerData.AddSkillPoints(5);
    }

    // Destructive development action: clears permanent upgrades and writes the changed progression
    // to the normal save slot. Do not expose this callback in a release menu.
    public void ResetSkillAndLevel()
    {
        playerData.skillUpgrades.Clear();
        playerData.level = 1;
        playerData.SetSkillPoints(0);
        saveGame.SaveGameToDisk();
    }
}
