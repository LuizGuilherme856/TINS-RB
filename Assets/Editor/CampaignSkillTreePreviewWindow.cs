#if UNITY_EDITOR
using System.Collections.Generic;
using TINS.Campaign;
using UnityEditor;
using UnityEngine;

public sealed class CampaignSkillTreePreviewWindow : EditorWindow
{
    private const string CatalogPath = "Assets/TINS - New Mechanics/Campaign/Resources/CampaignCatalog.asset";
    private CampaignCatalog catalog;
    private CampaignProfile simulation = new CampaignProfile { souls = 100 };
    private Vector2 scroll;

    [MenuItem("Tools/There Is No Sun/Preview Skill Tree Progression")]
    private static void Open()
    {
        CampaignSkillTreePreviewWindow window = GetWindow<CampaignSkillTreePreviewWindow>("Skill Tree Preview");
        window.minSize = new Vector2(520f, 420f);
        if (window.catalog == null) window.catalog = AssetDatabase.LoadAssetAtPath<CampaignCatalog>(CatalogPath);
    }

    private void OnGUI()
    {
        CampaignCatalog selected = (CampaignCatalog)EditorGUILayout.ObjectField("Campaign Catalog", catalog, typeof(CampaignCatalog), false);
        if (selected != catalog)
        {
            catalog = selected;
            ResetSimulation();
        }
        if (catalog == null)
        {
            EditorGUILayout.HelpBox("Selecione o CampaignCatalog para simular a revelação e a compra dos nós.", MessageType.Info);
            return;
        }
        if (catalog.skills == null)
        {
            EditorGUILayout.HelpBox("O catálogo não tem habilidades cadastradas.", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space(4f);
        using (new EditorGUILayout.HorizontalScope())
        {
            simulation.souls = Mathf.Max(0, EditorGUILayout.IntField("Almas simuladas", simulation.souls));
            if (GUILayout.Button("Reiniciar simulação", GUILayout.Width(150f))) ResetSimulation();
        }

        HashSet<string> resourceIds = new HashSet<string>();
        foreach (CampaignSkill skill in catalog.skills)
            if (skill != null && !string.IsNullOrWhiteSpace(skill.requiredResourceId)) resourceIds.Add(skill.requiredResourceId);
        foreach (string resourceId in resourceIds)
        {
            int current = simulation.Resource(resourceId);
            int updated = Mathf.Max(0, EditorGUILayout.IntField("Material · " + resourceId, current));
            if (updated > current) simulation.AddResource(resourceId, updated - current);
            if (updated < current) simulation.TrySpend(resourceId, current - updated);
        }

        EditorGUILayout.Space(6f);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (CampaignSkill skill in catalog.skills)
        {
            if (skill == null) continue;
            DrawSkill(skill);
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawSkill(CampaignSkill skill)
    {
        int rank = simulation.Rank(skill.id);
        bool visible = simulation.IsSkillVisible(skill);
        bool requirementsMet = simulation.MeetsSkillRequirements(skill);
        int soulCost = skill.CostForRank(rank);
        int materialCost = skill.ResourceCostForRank(rank);
        bool maxed = rank >= skill.maximumRank;
        bool canPurchase = simulation.CanBuy(skill);

        string state = !visible ? "OCULTA" : maxed ? "MAXIMIZADA" :
            !requirementsMet ? "REQUISITO PENDENTE" :
            !canPurchase ? "SEM RECURSOS" : "DISPONÍVEL";

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(skill.displayName, EditorStyles.boldLabel);
                GUILayout.Label(state, GUILayout.Width(205f));
            }
            EditorGUILayout.LabelField($"ID {skill.id} · Nível {rank}/{skill.maximumRank} · Posição {skill.editorPosition}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField(skill.description, EditorStyles.wordWrappedMiniLabel);
            if (!maxed)
            {
                string price = $"{soulCost} almas";
                if (materialCost > 0) price += $" + {materialCost} {skill.requiredResourceId}";
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Próxima compra: " + price);
                    using (new EditorGUI.DisabledScope(!canPurchase))
                        if (GUILayout.Button("Simular compra", GUILayout.Width(120f)) && simulation.TryBuy(skill)) Repaint();
                }
            }
        }
    }

    private void ResetSimulation()
    {
        simulation = new CampaignProfile { souls = 100 };
        Repaint();
    }
}
#endif
