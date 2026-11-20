#if UNITY_EDITOR
using TINS.Campaign;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(CampaignSkillTreeNodeView))]
public sealed class CampaignSkillTreeNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();

        CampaignSkillTreeNodeView node = (CampaignSkillTreeNodeView)target;
        CampaignSkill skill = node.SkillAsset;
        RectTransform rect = node.transform as RectTransform;
        if (skill == null || rect == null)
        {
            EditorGUILayout.HelpBox("Associe um CampaignSkill e mantenha este nó como elemento da UI.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(5f);
        if (GUILayout.Button("Aplicar posição do nó ao asset"))
        {
            Undo.RecordObject(skill, "Update Campaign Skill Position");
            skill.editorPosition = rect.anchoredPosition;
            EditorUtility.SetDirty(skill);
            AssetDatabase.SaveAssets();
        }
        if (GUILayout.Button("Aplicar posição do asset à cena"))
        {
            Undo.RecordObject(rect, "Move Campaign Skill Node");
            rect.anchoredPosition = skill.editorPosition;
            EditorUtility.SetDirty(rect);
            EditorSceneManager.MarkSceneDirty(node.gameObject.scene);
        }
    }
}
#endif
