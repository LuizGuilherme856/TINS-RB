
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GunData))]
public class GunDataEditor : Editor
{

    
    private bool showGunStats = true;
    private bool showDamage = true;
    private bool showStatus = true;
    private bool showAmmo = true;
    private bool showBullet = true;
    private bool showRecoil = true;
    private bool showCamera = true;
    private bool showShellsAndClip = true;
    private bool showMuzzleFlash = true;
    private bool showSounds = true;
    private bool showOthers = true;
    private bool showNameAndDescription = true; // New category.
    private bool showGunScore = true;

    private void OnEnable()
    {
        SetAllFoldouts(false);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Buttons to expand or collapse all sections.
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Expandir tudo"))
        {
            SetAllFoldouts(true);
        }
        if (GUILayout.Button("Colapsar tudo"))
        {
            SetAllFoldouts(false);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Custom Inspector sections.
        DrawFoldoutSection(" # -- Name and Description", ref showNameAndDescription, new[]
        {
            "nameEnglish", "descriptionEnglish",
            "nameChinese", "descriptionChinese",
            "namePortuguese", "descriptionPortuguese",
            "nameSpanish", "weaponDescriptionSpanish"
        });

        //Parameter update section.

        //string[] propertyNames = new[] {
        //    "damage", "criticalChance", "criticalMultiplier" , "statusChance", "statusDuration", "statusDamagePerSec",
        //  "fireRate", "reloadTime",  "scatteredBulletsPerShot", "burstSize",  "maxAmmoOnClip", "maxSpareAmmo", "price"  , "ammoPrice"};


        //foreach (string propertyName in propertyNames)
        //{
        //    var property = serializedObject.FindProperty(propertyName);
        //    if (property != null)
        //    {
        //        EditorGUILayout.PropertyField(property);
        //    }
        //    else
        //    {
        //        Debug.LogWarning($"Property '{propertyName}' not found in GunData.");
        //    }
        //}


        DrawFoldoutSection(" # -- Gun Scores", ref showGunScore, new[] { "DPS",  "PricePerDps", "rarity"});
        DrawFoldoutSection(" # -- Gun Stats", ref showGunStats, new[] { "price", "ammoPrice", "sprite" });
        DrawFoldoutSection(" # -- Damage", ref showDamage, new[] { "criticalChance", "criticalMultiplier", "damage", "damageType", "damageFalloffMultiplier", "minDamageFalloff" });
        DrawFoldoutSection(" # -- Status", ref showStatus, new[] { "statusChance", "statusEffect", "statusDuration", "statusDamagePerSec" });
        DrawFoldoutSection(" # -- Ammo", ref showAmmo, new[] { "maxAmmoOnClip", "maxSpareAmmo", "ammoConsumedPerShot", "chanceOfNotConsumingAmmo" });
        DrawFoldoutSection(" # -- Bullet", ref showBullet, new[] { "bullet", "bulletSpeed", "bulletLifeTime", "bulletDeathFX", "bulletRicochetFx", "scatteredBulletsPerShot", "multipleBulletAngleOffset", "linedUpBulletsPerShot", "speedRandomness", "maxCollisions", "knockBackForce", "projectileSize" });
        DrawFoldoutSection(" # -- Recoil", ref showRecoil, new[] { "recoilForceOnPlayer", "recoilAngle", "recoilDuration", "imprecision" });
        DrawFoldoutSection(" # -- Camera", ref showCamera, new[] { "camShakeMag", "camShakeDuration" });
        DrawFoldoutSection(" # -- Shells and Clip", ref showShellsAndClip, new[] { "bulletCapsule", "clip", "dropCapsuleOnshooting", "capsuleSpawnOffset" });
        DrawFoldoutSection(" # -- Muzzle Flash", ref showMuzzleFlash, new[] { "muzzleFlash", "hasMuzzleFlash", "muzzleFlashSpawnOffset" });
        DrawFoldoutSection(" # -- Sounds", ref showSounds, new[] { "shootingSFX", "shootingVolume", "soundType", "startShootingSFX", "stopShootingSFX", "reloadVolume", "startReloadingSFX", "stopReloadingSFX" });
        DrawFoldoutSection(" # -- Others", ref showOthers, new[] { "fireRate", "burstSize", "burstRate", "reloadTime", "gunType", "firePointOffset", "posOffset", "isGoldenWeapon", "goldenVersion", "buyIcon", "statsCalculator", "impactFxSpawnProbability", "deathFxSpawnProbability" });

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Draws a collapsible Inspector section.
    /// </summary>
    private void DrawFoldoutSection(string label, ref bool foldout, string[] propertyNames)
    {
        foldout = EditorGUILayout.Foldout(foldout, label, true);
        if (foldout)
        {
            foreach (string propertyName in propertyNames)
            {
                var property = serializedObject.FindProperty(propertyName);
                if (property != null)
                {
                    EditorGUILayout.PropertyField(property);
                }
                else
                {
                    Debug.LogWarning($"Property '{propertyName}' not found in GunData.");
                }
            }
            EditorGUILayout.Space(15);
        }
        EditorGUILayout.Space(10);
    }

    /// <summary>
    /// Sets all foldouts to expanded or collapsed.
    /// </summary>
    /// <param name="state">True expands all sections; false collapses them.</param>
    private void SetAllFoldouts(bool state)
    {
        showGunStats = state;
        showDamage = state;
        showStatus = state;
        showAmmo = state;
        showBullet = state;
        showRecoil = state;
        showCamera = state;
        showShellsAndClip = state;
        showMuzzleFlash = state;
        showSounds = state;
        showOthers = state;
        showNameAndDescription = state; // Include the new category.
        showGunScore = state;
    }
    
}


#endif
