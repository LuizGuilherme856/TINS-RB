using System.Collections;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine;
using System;
//[CreateAssetMenu(fileName = "characterData", menuName = "CharacterData")]
///Stores character-specific data, such as Azaus's configuration.
public class CharacterData : GenericData
{
    [Space(30)]
    public string shortName = "player";
    public Sprite smallIcon;
    public Sprite bigPortrait, dialoguePortrait;
    public Character character;
    [field: SerializeField] public GunData defaultGun { get; private set; }
    [field: SerializeField] public MeleeData defaultMelee { get; private set; }
    [field: SerializeField] public MeleeData defaultSpecialAbility { get; private set; }
    [field: SerializeField] public MeleeData defaultDash { get; private set; }
    [field: SerializeField] public float defaultHp { get; private set; }
    [field: SerializeField] public int initialPoints { get; private set; }

    [field: SerializeField] public float chanceToIgnoreDamage { get; private set; } = 0;

    [field: SerializeField] public int numberOfGunsItCanCarry { get; private set; }

    [field: SerializeField] public RuntimeAnimatorController animatorController { get; private set; }
    public GameObject corpse;
    public AudioClip hitSfx;


}
