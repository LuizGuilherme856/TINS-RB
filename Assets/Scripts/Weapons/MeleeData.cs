using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "new Melee", menuName = "Melee")]
public class MeleeData : WeaponData
{
    [Header("Three-hit slash")]
    public bool useSlashCombo;
    public SlashStep openingSlash = new SlashStep(0.035f, 0.09f, 0.085f, 1.65f, 100f, 10f, 0.8f, 0.08f, 0.03f);
    public SlashStep returnSlash = new SlashStep(0.025f, 0.08f, 0.075f, 1.7f, 115f, 3f, 0.85f, 0.1f, 0.02f);
    public SlashStep finishingSlash = new SlashStep(0.09f, 0.12f, 0.22f, 2.0f, 135f, 6f, 1.5f, 1.8f, 0.065f);
    [Min(0f)] public float comboInputBuffer = 0.12f;
    [Min(0f)] public float comboLinkWindow = 0.38f;

    [Header("Feedbacks")]
    [Range(0, 0.1f)] public float freezeFrameTime = 0.01f;
    [Range(-1,1)] public float lensDistortion = -0.2f;
    public bool hasTrail = false;
    public float trailTime = 0;

    [Header("OTHERS")]
    public float distOffset;

    [Tooltip("for�a que empurra o inimigo ")]
    public float camShakeMagnitude = -1f; //Negate to move forward rather than backward.

    [Tooltip("velocidade que o projetil voa ")]
    public float flySpeed=1;
    public float attackRate=1;
    public float invulnerableTimer = 0; //Invulnerability duration while attacking.
    //public int charges = 1; //Number of available actions before waiting for cooldown.

    [Tooltip("um salto que o player da pra frente quando ataca")]
    public float playerImpulse = 1f;
    [Tooltip("objeto que � spawnado. funciona como um projetil.")]
    public GameObject meleeGameObject; // 
    public bool isPlayerChild = false;
    public float specialFxDuration = 2f; //For example, Hanba's intangibility ability.
    public bool instantiateGameObjectOnAttack = true; //Used by most abilities except Hanba's.
    [Tooltip("Qual tipo de player action ele �, dividido em dash, special e melee")]
    public PlayerAction playerAction = PlayerAction.special;

    [Header("AUDIO")]
    [SerializeField] private AudioClip attackSFX;
    [SerializeField] private AudioClip[] randomAttackSFXs = null;
    [Range(0, 1)] public float volume = 1f;


    public AudioClip GetAttackSfx()
    {
        if (randomAttackSFXs == null || randomAttackSFXs.Length == 0)
        {
            return attackSFX;
        }
        else
        {
            return randomAttackSFXs[Random.Range(0, randomAttackSFXs.Length)];
        }
    }
    public override float GetDamage(bool getDefaultDamage = false)
    {
        if (getDefaultDamage) return damage;
        return statsCalculator.CalculateNewMeleeDamage(damage, damageType);
    }

    public override int GetPrice()
    {
        if (globalStats == null) globalStats = AssetFinder.GetGlobalStats();

        return (int)(base.GetPrice() * globalStats.ForCurrentRun.meleeWeaponPriceMultiplier * globalStats.ForCurrentRun.globalPriceMultiplier);
    }


}
