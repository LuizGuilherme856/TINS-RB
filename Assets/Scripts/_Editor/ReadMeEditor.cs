#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

[CustomEditor(typeof(ReadMe))]
public class ReadMeEditor : Editor
{
    //private Texture2D defaultIcon; // Store the icon in the editor

    private void OnEnable()
    {
        // Load the icon (Replace with your desired icon path)
        //defaultIcon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Editor/Icons/warning.png", typeof(Texture2D));
    }

    public override void OnInspectorGUI()
    {
        ReadMe readMe = (ReadMe)target;

        //// Display the icon and the text field in a horizontal layout
        //GUILayout.BeginHorizontal();
        //if (defaultIcon != null)
        //{
        //    GUILayout.Label(selectedIcon, GUILayout.Width(64), GUILayout.Height(64));
        //}

        //// Display the note field next to the icon
        //EditorGUILayout.BeginVertical(); // Begin a vertical block to stack the label and text area
        //EditorGUILayout.LabelField("Note", EditorStyles.boldLabel);
        //readMe.note = EditorGUILayout.TextArea(readMe.note, GUILayout.Height(75));
        //EditorGUILayout.EndVertical();
        //GUILayout.EndHorizontal();

        //GUILayout.Space(10); // Add some space between elements if needed
        readMe.note = EditorGUILayout.TextArea(readMe.note, GUILayout.Height(75));
    }
}

#endif