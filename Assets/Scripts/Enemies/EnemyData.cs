using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

//[CreateAssetMenu(fileName = "new enemy", menuName = "enemy")]
public class EnemyData : GenericData
{
    public string EnemyName;
    public EnemyType enemyType;
    public bool isFlier = false;
    public bool bossOrElite = false;
    /// <summary>
    /// Spawn cost in points, effectively the enemy tier.
    /// </summary>
    [Min(1)] public int spawnPoints =1;
     public bool giveXp = true;
    [Min(0)] public int xp = 5;
     public bool giveBloodPoints = true;

    [Space (15)]
    [Header("   ENEMY STATS ")]
    [Space(5)]
    public float health;
    public EnemyElement enemyElement = EnemyElement.Flesh;
    public bool ignoreMassWhenMoving = false;

    [Space(15)]
    [Header("   Health Update ")]

    public int updateHpOnlyAfterWave = 0;
    [Tooltip("doNotIncreaseHealth - não aumenta a vida nunca. \nincreaseEveryWaveWithCurve - usa um coeffiente para multiplicar a wave e hp baseado na curva, \nincreaseEveryWaveWithHordeManager - usa o coeficiente do HordeManager, \nincreaseEveryWaveWithCustomCoefficient - usa um coeficiente proprio, \nsetHpToWaveValue - usa o valor atual da curva naquela onda pra ser a vida")]
    public EnemyHealthUpdateMode healthUpdateMode = EnemyHealthUpdateMode.increaseEveryWaveWithHordeManager;
    [Tooltip("S� vai ser usado se healthUpdateMode for increaseEveryWaveWithCustomCoefficient")]
    public float healthIncreaseCustomCoefficient = 0.2f;
    [Tooltip("S� vai ser usado se healthUpdateMode for increaseEveryWaveWithCurve")]
    public AnimationCurve healthIncreaseCurve;

    public float hpUntillwave5Multiplier = 0.75f;
    public float hpUntillwave10Multiplier = 0.75f, hpUntillwave15Multiplier = 1; 

    [Space(10)]
    [Header("   SPEED")]
    [SerializeField] private float speed;
    [SerializeField] private bool increaseSpeedEveryWave = true;
    [SerializeField] private float maxSpeed = 1000;
    [Tooltip("Vai atenuar o empurrao que as balas dao nele. \n1 = valor padrão. nada muda. \n 0 qualquer empurrao é anulado")]
    [Range(0, 1)] public float pushDamping = 1;

    [Space(10)]

    public bool healthIncreasesEveryWave = true;
    public float minDistanceToIncreaseSpeed = 15f, speedMultiplierWhenNotVisible = 2f;
    public float updatePathInterval;
    public bool increaseHealthOnRainWaves = true;

    public bool addWaveNumberToHealth = false; //Give selected enemies, such as the spider skeleton, extra health in later waves.
    [HideInInspector]
    public float speed_easyMode, health_easyMode;

    [Header("   BOSS ")]
    public float secondFormSpeedMultiplier = 1;


    [Header("   STATUS ")]
    public bool immuneToStatus = false;
    public bool immuneToFire, immuneToIce, immuneToSlash, immuneToImpact, immuneToPoison, immuneToExplosion;
    public bool immuneToInstaKill = false;

    [Header("   WEAKENESS ")]
    public bool weakenessToIce = false;

    [Header("   MELEE ATTACK")]
    public float attackDamage;
    public float attackRate, minDistanceToAttack, attackForce;
    public float timeAfterRespawnToAttack = 2f; //Prevent attacks while the spawn animation is still playing.

    [Header("   RANGED ATTACK")]
    public GameObject projectile;
    public float  maxDistanceToShoot, ProjectileSpeed; //Interval between shots.
    [SerializeField] private float fireRate = 4f;
    public bool useRandomFireRate = false;
    public RandomFloat fireRateInterval;
    [SerializeField] private bool increaseFireRateEveryWave = true;
    [SerializeField] private float minFireRate = 0.5f;
    [SerializeField] private int bulletsPerShot = 1;
    [Tooltip("Se for 5 por exemplo aumenta em 1 a quantidade de balas disparadas a cada 5 ondas")]
    [SerializeField] private int increaseBulletsPerShotEveryXWaves = 15; //Add one bullet per shot every 15 waves.
    [Range(0, 50)] public float multipleBulletAngleOffset = 0;


    [Header("   Other")]
    [SerializeField] protected RunData runData;


    public virtual float GetSpeed( bool secondForm =false)
    {
        if(maxSpeed < speed)
        {
            Debug.LogWarning("Max speed � menor que speed. Reveja os valores em " + name);
            maxSpeed = speed;
        }

        float newSpeed = speed;



        if(secondForm == true)
        {
            newSpeed *= secondFormSpeedMultiplier;
        }

        if(increaseSpeedEveryWave)
        {
            //Increase speed linearly so it doubles at wave 70.
            newSpeed = newSpeed + (newSpeed * runData.currentWaveNumber) /70f; //Increase speed as waves progress.
        }

        if (newSpeed > maxSpeed) newSpeed = maxSpeed;

        return newSpeed;


    }

    public float GetFireRate()
    {
        float newFireRate;

        if (useRandomFireRate) newFireRate = fireRateInterval.GetValue();
        else newFireRate = fireRate;
        

                
        if (increaseFireRateEveryWave)
        {
            //Reduce the firing interval to its minimum by wave 50.
            newFireRate = fireRate - (fireRate * runData.currentWaveNumber) / 50;

            if (newFireRate < minFireRate) newFireRate = minFireRate;
        }

        return newFireRate;
    }

    public float GetHealth()
    {
        return health;
        
    }

    public int GetBulletsPerShot()
    {
        float newBulletsPerShot = bulletsPerShot;
        if (increaseBulletsPerShotEveryXWaves > 0)
        {
            newBulletsPerShot += runData.currentWaveNumber / increaseBulletsPerShotEveryXWaves;
        }
        return Mathf.FloorToInt(newBulletsPerShot);
    }

    /// <summary>
    /// Additional EnemyData checks used when exporting CSV data.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string SafeString(object value) => value?.ToString().Replace(",", "|") ?? "None";

        return $"{SafeString(EnemyName)} , {SafeString(enemyType)} , {SafeString(isFlier)} , {SafeString(bossOrElite)} , " +
               $"{SafeString(spawnPoints)} , {SafeString(xp)} , {SafeString(health)} , {SafeString(enemyElement)} , {SafeString(speed)} , " +
               $"{SafeString(speed_easyMode)} , {SafeString(health_easyMode)} , {SafeString(minDistanceToIncreaseSpeed)} , " +
               $"{SafeString(speedMultiplierWhenNotVisible)}, {SafeString(healthIncreasesEveryWave)} , {SafeString(updatePathInterval)} , " +
               $"{SafeString(increaseHealthOnRainWaves)} , {SafeString(addWaveNumberToHealth)} , " +
               $"{SafeString(secondFormSpeedMultiplier)} , {SafeString(immuneToStatus)} , " +
               $"{SafeString(immuneToFire)} , {SafeString(immuneToIce)} , {SafeString(immuneToSlash)} , " +
               $"{SafeString(immuneToImpact)} , {SafeString(immuneToPoison)} , {SafeString(immuneToExplosion)} , " +
               $"{SafeString(immuneToInstaKill)} , {SafeString(weakenessToIce)} , " +
               $"{SafeString(attackDamage)} , {SafeString(attackRate)} , {SafeString(minDistanceToAttack)} , " +
               $"{SafeString(attackForce)} , {SafeString(timeAfterRespawnToAttack)} , " +
               $"{SafeString(fireRate)} , {SafeString(maxDistanceToShoot)} , {SafeString(ProjectileSpeed)} , " +
               $"{SafeString(bulletsPerShot)} , {SafeString(multipleBulletAngleOffset)} , " +
               $"{SafeString(projectile != null ? projectile.name : "None")}";
    }

}

public enum EnemyType
{
    demoninho, coxinha, eye, esqueletoAranha, soulEater, gromsch, larvae, brainSpider,
    hand, morcegao, fireball, demoninhoFire, octohead, cthulhu, alkarion, bomber,jumpingHandManager,gorfo,skeletonArcher,
    necromancer,egg,spearBell,healerBell,shieldBell,swordBell,lucifer,undead,bloodEggSpawner
}

/// <summary>
/// doNotIncreaseHealth: health never scales.
/// increaseEveryWaveWithCurve: scale wave and health using the curve coefficient.
/// increaseEveryWaveWithHordeManager: use the HordeManager coefficient.
/// increaseEveryWaveWithCustomCoefficient: use a custom coefficient.
/// setHpToWaveValue: use the curve value at the current wave as health.
/// </summary>
public enum EnemyHealthUpdateMode
{
    doNotIncreaseHealth, increaseEveryWaveWithCurve, increaseEveryWaveWithHordeManager,
    increaseEveryWaveWithCustomCoefficient, setHpToWaveValue
}
