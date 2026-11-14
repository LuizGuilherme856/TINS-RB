//#if UNITY_EDITOR

//using UnityEngine;
//using UnityEditor;

//[CustomEditor(typeof(MeleeData))]
//public class MeleeDataEditor : Editor
//{
//    private bool showNameAndDescription = true; // New category.
//    private bool showDamage = true;
//    private bool showFeedbacks = true;
//    private bool showOthers = true;
//    private bool showAudio = true;

//    private void OnEnable()
//    {
//        SetAllFoldouts(false);
//    }

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();

//        // Buttons to expand or collapse all sections.
//        EditorGUILayout.BeginHorizontal();
//        if (GUILayout.Button("Expandir tudo"))
//        {
//            SetAllFoldouts(true);
//        }
//        if (GUILayout.Button("Colapsar tudo"))
//        {
//            SetAllFoldouts(false);
//        }
//        EditorGUILayout.EndHorizontal();

//        EditorGUILayout.Space(10);


//    // Custom Inspector sections.
//    DrawFoldoutSection(" # -- Name and Description", ref showNameAndDescription, new[]
//        {
//            "nameEnglish", "descriptionEnglish",
//            "nameChinese", "descriptionChinese",
//            "namePortuguese", "descriptionPortuguese",
//            "nameSpanish", "weaponDescriptionSpanish"
//        });

//        DrawFoldoutSection(" # -- Damage", ref showDamage, new[] { "criticalChance", "criticalMultiplier", "damage", "instaKillChance", "damageType" , "statusDamagePerSec", "statusDuration", "statusDamage" });
//        DrawFoldoutSection(" # -- Feedbacks", ref showFeedbacks, new[] { "freezeFrameTime", "lensDistortion", "hasTrail", "trailTime" });
//        DrawFoldoutSection(" # -- Audio", ref showAudio, new[] { "attackSFX", "volume" });
//        DrawFoldoutSection(" # -- Others", ref showOthers, new[] { "distOffset", "knockBackForce", "camShakeMagnitude", "flySpeed", "attackRate", "invulnerableTimer", "playerImpulse", "meleeGameObject", "isPlayerChild", "specialFxDuration", "instantiateGameObjectOnAttack", "playerAction", "statsCalculator", "sprite", "buyIcon" });

//        serializedObject.ApplyModifiedProperties();
//    }

//    /// <summary>
//    /// Draws a collapsible Inspector section.
//    /// </summary>
//    private void DrawFoldoutSection(string label, ref bool foldout, string[] propertyNames)
//    {
//        foldout = EditorGUILayout.Foldout(foldout, label, true);
//        if (foldout)
//        {
//            foreach (string propertyName in propertyNames)
//            {
//                var property = serializedObject.FindProperty(propertyName);
//                if (property != null)
//                {
//                    EditorGUILayout.PropertyField(property);
//                }
//                else
//                {
//                    Debug.LogWarning($"Property '{propertyName}' not found in MeleeData.");
//                }
//            }
//            EditorGUILayout.Space(15);
//        }
//        EditorGUILayout.Space(10);
//    }

//    /// <summary>
//    /// Sets all foldouts to expanded or collapsed.
//    /// </summary>
//    private void SetAllFoldouts(bool state)
//    {
//        showNameAndDescription = state; // Include the new category.
//        showDamage = state;
//        showFeedbacks = state;
//        showOthers = state;
//        showAudio = state;
//    }
//}

//#endif