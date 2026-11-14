//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(GlobalStats))]
//public class GlobalStatsEditor : Editor
//{
//    private bool showPrices = true;
//    private bool showStartRun = true;
//    private bool showMisc = true;
//    private bool showPoints = true;
//    private bool showStatus = true;
//    private bool showDrop = true;
//    private bool showPlayer = true;
//    private bool showEnemies = true;
//    private bool showOrbitalUpgrades = true;
//    private bool showMelee = true;
//    private bool showSpecialAbility = true;
//    private bool showDash = true;
//    private bool showGlobalDamage = true;
//    private bool showGunDamage = true;
//    private bool showElementalDamage = true;
//    private bool showGunAmmo = true;
//    private bool showMeleeDamage = true;
//    private bool showThornDamage = true;
//    private bool showMultipleBullets = true;

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();

//        showPrices = EditorGUILayout.Foldout(showPrices, "Prices");
//        if (showPrices)
//        {

//            EditorGUILayout.PropertyField(serializedObject.FindProperty("doorPriceMultiplier"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("meleeWeaponPriceMultiplier"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("gunUpgradePriceMultiplier"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("runePriceMultiplier"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("gunPriceMultiplier"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("globalPriceMultiplier"));
//        }

//        showStartRun = EditorGUILayout.Foldout(showStartRun, "Start Run");
//        if (showStartRun)
//        {
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("numberOfRandomRunesToStartWith"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("numberOfRandomGunsToStartWith"));
//        }

//        showMisc = EditorGUILayout.Foldout(showMisc, "Misc");
//        if (showMisc)
//        {
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("RefilAmmoOnOpenDoor"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("FreeRevives"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("walkTrailDamageType"));
//        }

//        showPoints = EditorGUILayout.Foldout(showPoints, "Points");
//        if (showPoints)
//        {
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("extraPointsPerKill"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("extraInitialPoints"));
//        }

//        showStatus = EditorGUILayout.Foldout(showStatus, "Status");
//        if (showStatus)
//        {
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxChainReactions"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("statusChanceMultiplier"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("statusDamageMultiplier"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("statusDurationMultiplier"));
//        }

//        showDrop = EditorGUILayout.Foldout(showDrop, "Drop");
//        if (showDrop)
//        {
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("AddTodropChance"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("AddToHpHealedByHearts"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("powerUpGoTowardsPlayerMaxDistanceMultiplier"));
//        }

//        showPlayer = EditorGUILayout.Foldout(showPlayer, "Player");
//        if (showPlayer)
//        {
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxIgnoreDamageChance"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("increaseWeaponCapacityBy"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("playerWalkSpeedMultiplier"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("AddToPlayerInitialHealth"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("IncreasePlayerMaxHealthBy"));
//        }

//        showEnemies = EditorGUILayout.Foldout(showEnemies, "Enemies");
//        if (showEnemies)
//        {
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyHealthMultiplier"));
//        }

//        showOrbitalUpgrades = EditorGUILayout.Foldout(showOrbitalUpgrades, "Orbital Upgrades");
//        if (showOrbitalUpgrades)
//        {
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("IncreaseOrbitalSpeedBy"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("OrbitalSpeed_Multiplier"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("OrbitalBladeInstaKillChance"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("OrbitalBladeDamage_Multiplier"));
//        }

//        showMelee = EditorGUILayout.Foldout(showMelee, "Melee");
//        if (showMelee)
//        {
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxMeleeActionQuantity"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("MeleeAttackRateMultiplier"));
//        }

//        showSpecialAbility = EditorGUILayout.Foldout(showSpecialAbility, "Special Ability");
//        if (showSpecialAbility)
//        {
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxSpecialActionQuantity"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("SpecialAbilityCooldownMultiplier"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("resetSpecialAbilityOnEnemyDeathChance"));
//        }

//        showDash = EditorGUILayout.Foldout(showDash, "Dash");
//        if (showDash)
//        {
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxDashActionQuantity"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("DashAbilityCooldownMultiplier"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashDamageMultiplier"));
//        }

//        showGlobalDamage = EditorGUILayout.Foldout(showGlobalDamage, "Global Damage");
//        if (showGlobalDamage)
//        {
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("GlobalDamage_Multiplier"));
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("IncreaseGlobalDamageBy"));
//        }

//        serializedObject.ApplyModifiedProperties();
//    }
//}
