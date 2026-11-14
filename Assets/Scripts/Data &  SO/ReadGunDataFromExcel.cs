
using System;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ReadGunDataFromExcel", menuName = "Others/ReadGunDataFromExcel")]
public class ReadGunDataFromExcel : ScriptableObject
{
    string csvFilePath; // CSV file path.
    public List<GunData> gunDataList; // GunData list assigned in the Inspector.


    // ...

    [ContextMenu("Read CSV")]
    public void ReadCSV()
    {
        csvFilePath = GetDirectoryPath();

        if (File.Exists(csvFilePath))
        {
            using (StreamReader sr = new StreamReader(csvFilePath))
            {
                string headerLine = sr.ReadLine();
                string[] headers = headerLine.Split(',');

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] values = line.Split(',');

                    string gunName = values[Array.IndexOf(headers, "Name")];
                    GunData gunData = gunDataList.Find(g => g.name == gunName);

                    if (gunData != null)
                    {
                        for (int i = 0; i < headers.Length; i++)
                        {
                            try
                            {
                                switch (headers[i].ToLower())
                            {
                                case "name":
                                    gunData.name = values[i];
                                    break;
                                case "damage":
                                    gunData.damage = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "maxammoonclip":
                                    gunData.maxAmmoOnClip = int.Parse(values[i]);
                                    break;
                                case "maxspareammo":
                                    gunData.maxSpareAmmo = int.Parse(values[i]);
                                    break;
                                case "ammoconsumedpershot":
                                    gunData.ammoConsumedPerShot = int.Parse(values[i]);
                                    break;
                                case "chanceofnotconsumingammo":
                                    gunData.chanceOfNotConsumingAmmo = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "bulletspeed":
                                    gunData.bulletSpeed = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "bulletlifetime":
                                    gunData.bulletLifeTime = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "scatteredbulletsperShot":
                                    gunData.scatteredBulletsPerShot = int.Parse(values[i]);
                                    break;
                                case "multiplebulletangleoffset":
                                    gunData.multipleBulletAngleOffset = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "linedupbulletsperShot":
                                    gunData.linedUpBulletsPerShot = int.Parse(values[i]);
                                    break;
                                case "speedrandomness":
                                    gunData.speedRandomness = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "recoilforceonplayer":
                                    gunData.recoilForceOnPlayer = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "recoilangle":
                                    gunData.recoilAngle = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "recoilduration":
                                    gunData.recoilDuration = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "imprecision":
                                    gunData.imprecision = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "camshakemag":
                                    gunData.camShakeMag = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "camshakeduration":
                                    gunData.camShakeDuration = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "shootingvolume":
                                    gunData.shootingVolume = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "reloadvolume":
                                    gunData.reloadVolume = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "firerate":
                                    gunData.fireRate = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "burstsize":
                                    gunData.burstSize = int.Parse(values[i]);
                                    break;
                                case "burstrate":
                                    gunData.burstRate = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "reloadtime":
                                    gunData.reloadTime = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "isgoldenweapon":
                                    gunData.isGoldenWeapon = bool.Parse(values[i]);
                                    break;
                                case "impactfxspawnprobability":
                                    gunData.impactFxSpawnProbability = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "deathfxspawnprobability":
                                    gunData.deathFxSpawnProbability = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "damagefalloffmultiplier":
                                    gunData.damageFalloffMultiplier = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "price":
                                    gunData.price = int.Parse(values[i]);
                                    break;
                                case "ammoprice":
                                    gunData.ammoPrice = int.Parse(values[i]);
                                    break;
                                case "criticalchance":
                                    gunData.criticalChance = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "criticalmultiplier":
                                    gunData.criticalMultiplier = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "damagetype":
                                    gunData.damageType = (DamageType)System.Enum.Parse(typeof(DamageType), values[i]);
                                    break;
                                case "knockbackforce":
                                    gunData.knockBackForce = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "statuschance":
                                    gunData.statusChance = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "statusduration":
                                    gunData.statusDuration = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "statusdamagepersec":
                                    gunData.statusDamagePerSec = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                case "maxcollisions":
                                    gunData.maxCollisions = int.Parse(values[i]);
                                    break;
                                case "dps":
                                    gunData.DPS = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break ;
                                case "priceperdps":
                                    gunData.PricePerDPS = float.Parse(values[i], CultureInfo.InvariantCulture);
                                    break;
                                // case "rarity":
                                //     gunData.rarity =  values[i];
                                    //break;
                            }

                            }
                            catch (Exception e)
                            {
                                if (values[i] != string.Empty)
                                Debug.LogWarning("Erro ao ler do CSV: " + values[i] + "N�o � um valor valido para " + headers[i] + e.Message);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Arquivo CSV n�o encontrado: " + csvFilePath);
        }
    }

#if UNITY_EDITOR

    //[ContextMenu("Get All Gun Data")]
    //public void GetAllGunDataButton()
    //{
    //    gunDataList = GetAllGunData();
    //}


    //public static List<GunData> GetAllGunData()
    //{
    //    List<GunData> gunDataList = new List<GunData>();
    //    string[] guids = AssetDatabase.FindAssets("t:GunData", new[] { "Assets" });

    //    foreach (string guid in guids)
    //    {
    //        string path = AssetDatabase.GUIDToAssetPath(guid);
    //        GunData gunData = AssetDatabase.LoadAssetAtPath<GunData>(path);
    //        if (gunData != null)
    //        {
    //            gunDataList.Add(gunData);
    //        }
    //    }

    //    return gunDataList;
    //}

#endif

    /// <summary>
    /// Return the Windows Documents path or Unity's persistent data path.
    /// </summary>
    /// <returns></returns>
    private string GetDirectoryPath()
    {
        string path;
        path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("\\", "/");
        path += "/Raining Blood Hellfire/Planilha - Armas balanceamento.csv";

        return path;
    }
}

