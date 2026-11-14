using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Equip the player for the introduction.
/// </summary>
public class Intro_GivePlayerEquipment : MonoBehaviour
{
    [SerializeField] GunData[] guns;
    //[SerializeField] int numberOfExtraShots = 15;
    [SerializeField] int health;
    [SerializeField] int initialWave = 14;




    [SerializeField] PlayerHolder playerHolder;
    private CheatsHandler cheatsHandler;

    private void Awake()
    {
        cheatsHandler = GetComponent<CheatsHandler>();
    }

    private void Start()
    {
        cheatsHandler.SpecialAttackGlassSword();
        cheatsHandler.SetWave(initialWave);

        //for (int i = 0; i < numberOfExtraDamage; i++) cheatsHandler.AddExtraDamage();
        //for (int i = 0;i < numberOfExtraShots; i++) cheatsHandler.AddExtraShot();
        for (int i = 0; i < health; i++) cheatsHandler.AddOneHealthPoint();




        foreach (var gun in guns)
        {
            playerHolder.GetGunInventory().AddGunToInventory(gun);
        }
    }



}
