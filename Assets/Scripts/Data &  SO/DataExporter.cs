using ND_VariaBULLET;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
/// <summary>
/// Read ScriptableObject assets and export them to CSV.
/// </summary>
#if UNITY_EDITOR

public class DataExporter : MonoBehaviour
{
    public List<GunData> gunDatas;
    public List<EnemyData> enemyDatas;
    public List<RuneData> runeDatas;

    void Start()
    {
        gunDatas = GetAllGunData();
        enemyDatas = GetAllEnemyData();
        runeDatas = GetAllRuneData();
        ExportGunDataToCSV("Assets/ExportedGunData.csv");
        ExportEnemyDataToCSV("Assets/ExportedEnemyData.csv");
        ExportRuneDataToCSV("Assets/ExportedRuneData.csv");
    }


    public static List<GunData> GetAllGunData()
    {
        List<GunData> gunDataList = new List<GunData>();
        string[] guids = AssetDatabase.FindAssets("t:GunData", new[] { "Assets" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GunData gunData = AssetDatabase.LoadAssetAtPath<GunData>(path);
            if (gunData != null)
            {
                gunDataList.Add(gunData);
            }
        }

        return gunDataList;
    }

    public static List<EnemyData> GetAllEnemyData()
    {
        List<EnemyData> enemyDataList = new List<EnemyData>();
        string[] guids = AssetDatabase.FindAssets("t:EnemyData", new[] { "Assets" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EnemyData enemyData = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (enemyData != null)
            {
                enemyDataList.Add(enemyData);
            }
        }

        return enemyDataList;
    }

    private void ExportGunDataToCSV(string filePath)
    {
        List<string> lines = new List<string>();

        // Add the header.
        lines.Add("Name,MaxAmmoOnClip,MaxSpareAmmo,AmmoConsumedPerShot,ChanceOfNotConsumingAmmo,BulletSpeed,BulletLifeTime,BulletDeathFX,BulletRicochetFx,BulletCollisionDeathFX,ScatteredBulletsPerShot,MultipleBulletAngleOffset,LinedUpBulletsPerShot,SpeedRandomness,RecoilForceOnPlayer,RecoilAngle,RecoilDuration,Imprecision,CamShakeMag,CamShakeDuration,BulletCapsule,Clip,DropCapsuleOnShooting,CapsuleSpawnOffset,MuzzleFlash,HasMuzzleFlash,MuzzleFlashSpawnOffset,ShootingSFX,ShootingVolume,SoundType,StartShootingSFX,StopShootingSFX,ReloadVolume,StartReloadingSFX,StopReloadingSFX,FireRate,BurstSize,BurstRate,ReloadTime,GunType,HoveringShotName,FirePointOffset,PosOffset,IsGoldenWeapon,GoldenVersion,DPS,StatusRelevance,DpsPerCost,OverallScore,ImpactFxSpawnProbability,DeathFxSpawnProbability,CurrentAmmoOnClip,CurrentSpareAmmo,Price,AmmoPrice,Sprite,CriticalChance,CriticalMultiplier,Damage,DamageType,KnockBackForce,StatusChance,StatusDuration,StatusDamagePerSec,MaxCollisions,ProjectileSize,BuyIcon,InstaKillChance,DamageFalloffMultiplier,MinDamageFalloff,NameEnglish,DescriptionEnglish,NameChinese,DescriptionChinese,NamePortuguese,DescriptionPortuguese,NameSpanish,DescriptionSpanish");

        // Add the data.
        foreach (GunData gunData in gunDatas)
        {
            try
            {
                string line = $"{CheckNull(gunData.name)},{CheckNull(gunData.GetMaxAmmoOnClip())},{CheckNull(gunData.GetMaxSpareAmmo())},{CheckNull(gunData.GetAmmoConsumedPerShot())},{CheckNull(gunData.chanceOfNotConsumingAmmo)},{CheckNull(gunData.bulletSpeed)},{CheckNull(gunData.bulletLifeTime)},{CheckNull(gunData.bulletDeathFX)},{CheckNull(gunData.bulletRicochetFx)},{CheckNull(gunData.bulletCollisionDeathFX)},{CheckNull(gunData.GetScatteredBulletsPerShot())},{CheckNull(gunData.multipleBulletAngleOffset)},{CheckNull(gunData.GetLinedUpBulletsPerShot())},{CheckNull(gunData.speedRandomness)},{CheckNull(gunData.recoilForceOnPlayer)},{CheckNull(gunData.recoilAngle)},{CheckNull(gunData.recoilDuration)},{CheckNull(gunData.imprecision)},{CheckNull(gunData.camShakeMag)},{CheckNull(gunData.camShakeDuration)},{CheckNull(gunData.bulletCapsule)},{CheckNull(gunData.clip)},{CheckNull(gunData.dropCapsuleOnshooting)},{CheckNull(gunData.capsuleSpawnOffset)},{CheckNull(gunData.muzzleFlash)},{CheckNull(gunData.hasMuzzleFlash)},{CheckNull(gunData.muzzleFlashSpawnOffset)},{CheckNull(gunData.shootingSFX)},{CheckNull(gunData.shootingVolume)},{CheckNull(gunData.soundType)},{CheckNull(gunData.startShootingSFX)},{CheckNull(gunData.stopShootingSFX)},{CheckNull(gunData.reloadVolume)},{CheckNull(gunData.startReloadingSFX)},{CheckNull(gunData.stopReloadingSFX)},{CheckNull(gunData.fireRate)},{CheckNull(gunData.burstSize)},{CheckNull(gunData.burstRate)},{CheckNull(gunData.reloadTime)},{CheckNull(gunData.gunType)},{CheckNull(gunData.hoveringShotName)},{CheckNull(gunData.firePointOffset)},{CheckNull(gunData.posOffset)},{CheckNull(gunData.isGoldenWeapon)},{CheckNull(gunData.goldenVersion)},{CheckNull(gunData.DPS)},{CheckNull(gunData.impactFxSpawnProbability)},{CheckNull(gunData.deathFxSpawnProbability)},{CheckNull(gunData.currentAmmoOnClip)},{CheckNull(gunData.currentSpareAmmo)},{CheckNull(gunData.price)},{CheckNull(gunData.ammoPrice)},{CheckNull(gunData.sprite)},{CheckNull(gunData.criticalChance)},{CheckNull(gunData.criticalMultiplier)},{CheckNull(gunData.GetDamage())},{CheckNull(gunData.damageType)},{CheckNull(gunData.knockBackForce)},{CheckNull(gunData.statusChance)},{CheckNull(gunData.statusDuration)},{CheckNull(gunData.statusDamagePerSec)},{CheckNull(gunData.maxCollisions)},{CheckNull(gunData.projectileSize)},{CheckNull(gunData.buyIcon)},{CheckNull(gunData.instaKillChance)},{CheckNull(gunData.damageFalloffMultiplier)},{CheckNull(gunData.minDamageFalloff)},{CheckNull(gunData.GetName(Language.english))},{CheckNull(gunData.GetDescription(Language.english))},{CheckNull(gunData.GetName(Language.chinese))},{CheckNull(gunData.GetDescription(Language.chinese))},{CheckNull(gunData.GetName(Language.portuguese))},{CheckNull(gunData.GetDescription(Language.portuguese))},{CheckNull(gunData.GetName(Language.spanish))},{CheckNull(gunData.GetDescription(Language.spanish))}";
                lines.Add(line);

            }
            catch (Exception)
            {
                continue;
            }
        }

        // Write the file contents.
        File.WriteAllLines(filePath, lines);
    }

    private void ExportEnemyDataToCSV(string filePath)
    {
        List<string> lines = new List<string>();

        // Add the header.
        lines.Add("EnemyName, EnemyType, IsFlier, BossOrElite, SpawnPoints, XP, Health, EnemyElement, Speed,SpeedEasyMode, HealthEasyMode, MinDistanceToIncreaseSpeed,SpeedMultiplier, HealthIncreasesEveryWave, UpdatePathInterval,IncreaseHealthOnRainWaves, AddWaveNumberToHealth,SecondFormSpeedMultiplier, ImmuneToStatus,ImmuneToFire, ImmuneToIce, ImmuneToSlash,ImmuneToImpact, ImmuneToPoison, ImmuneToExplosion,ImmuneToInstaKill, WeakenessToIce,AttackDamage, AttackRate, MinDistanceToAttack,AttackForce, TimeAfterRespawnToAttack,FireRate, MaxDistanceToShoot, ProjectileSpeed,BulletsPerShot, MultipleBulletAngleOffset,Projectile");

        // Add the data.
        foreach (EnemyData enemyDataItem in enemyDatas)
        {
            try
            {
                string line = enemyDataItem.ToString();
                lines.Add(line);

            }
            catch (Exception)
            {
                continue;
            }
        }

        // Write the file contents.
        File.WriteAllLines(filePath, lines);
    }

    public static List<RuneData> GetAllRuneData()
    {
        List<RuneData> runeDataList = new List<RuneData>();
        string[] guids = AssetDatabase.FindAssets("t:RuneData", new[] { "Assets" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            RuneData runeData = AssetDatabase.LoadAssetAtPath<RuneData>(path);
            if (runeData != null)
            {
                runeDataList.Add(runeData);
            }
        }

        return runeDataList;
    }

    private void ExportRuneDataToCSV(string filePath)
    {
        List<string> lines = new List<string>();

        // Add the header.
        lines.Add("NamePortuguese,DescriptionPortuguese,Price,MaxPrice,PriceMultiplier,Path");

        // Add the data.
        foreach (RuneData runeData in runeDatas)
        {
            try
            {
                string line = $"{CheckNull(runeData.GetName(Language.portuguese))},{CheckNull(runeData.GetDescription(Language.portuguese))},{CheckNull(runeData.price)},{CheckNull(runeData.maxPrice)},{CheckNull(runeData.priceMultiplier)},{CheckNull(runeData.path)}";
                lines.Add(line);
            }
            catch (Exception)
            {
                continue;
            }
        }

        // Write the file contents.
        File.WriteAllLines(filePath, lines);
    }

    private string CheckNull(object value)
    {
        return value == null ? "nulo" : value.ToString().Replace(",", "|");
    }
}
#endif