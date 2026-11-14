#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RuneRitualHolder))]
public class RuneRitualHolderEditor : Editor
{
    private bool showNameAndDescription = true; // New category for names.

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
            "nameEnglish", "nameChinese", "namePortuguese", "nameSpanish"
        });

        // Display these fields directly without a foldout.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("runes"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));

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
                    EditorGUILayout.PropertyField(property, true);
                }
                else
                {
                    Debug.LogWarning($"Property '{propertyName}' not found in RuneRitualHolder.");
                }
            }
            EditorGUILayout.Space(15);
        }
        EditorGUILayout.Space(10);
    }

    /// <summary>
    /// Sets all foldouts to expanded or collapsed.
    /// </summary>
    private void SetAllFoldouts(bool state)
    {
        showNameAndDescription = state; // Include the new category.
    }
}


#endif