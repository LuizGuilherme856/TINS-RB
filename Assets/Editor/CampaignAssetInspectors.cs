using System;
using System.Collections.Generic;
using System.Linq;
using TINS.Campaign;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

[CustomEditor(typeof(CampaignMissionEnemies))]
public sealed class CampaignMissionEnemiesInspector : Editor
{
    public override void OnInspectorGUI()
    {
        var catalog = Resources.Load<CampaignCatalog>("CampaignCatalog");
        var planetNames = catalog?.planets?.Where(planet => planet != null && planet.enemySettings == target)
            .Select(planet => planet.displayName).ToArray();
        if (planetNames is { Length: > 0 })
            EditorGUILayout.LabelField("Elenco de " + string.Join(", ", planetNames), EditorStyles.boldLabel);
        DrawDefaultInspector();
    }
}

[CustomPropertyDrawer(typeof(CampaignPlanet))]
public sealed class CampaignPlanetDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty planet)
    {
        SerializedProperty name = planet.FindPropertyRelative("displayName");
        var root = new Foldout { text = name.stringValue, value = planet.isExpanded };
        root.RegisterValueChangedCallback(change =>
        {
            if (change.target == root) planet.isExpanded = change.newValue;
        });
        root.TrackPropertyValue(name, value => root.text = value.stringValue);
        AddFields(root, planet, "displayName", "description", "missionsToPurify");

        var light = AddGroup(root, "Iluminação e terreno");
        AddFields(light, planet, "initialLightRadius", "missionTargetRadius", "initialPenumbraRadius",
            "finalPenumbraRadius", "lightGrowthPerSoul", "floorTiles", "purifiedGrassRadiusRatio");
        var combat = AddGroup(root, "Inimigos e recompensas");
        AddFields(combat, planet, "enemySettings");
        var fallbackSpeed = new PropertyField(planet.FindPropertyRelative("enemySpeedMultiplier"),
            "Multiplicador de velocidade sem elenco");
        combat.Add(fallbackSpeed);
        SerializedProperty roster = planet.FindPropertyRelative("enemySettings");
        void ShowFallbackSpeed(SerializedProperty value) => fallbackSpeed.style.display =
            value.objectReferenceValue == null ? DisplayStyle.Flex : DisplayStyle.None;
        ShowFallbackSpeed(roster);
        root.TrackPropertyValue(roster, ShowFallbackSpeed);
        AddFields(combat, planet, "soulDropChance", "rewardResourceName",
            "rewardResourceId", "rewardPerMission", "victorySouls");
        var caches = AddGroup(root, "Caches de recursos");
        AddFields(caches, planet, "resourceCacheCount", "resourcePerCache", "resourceCacheSprite");
        var map = AddGroup(root, "Mapa e rotas");
        AddFields(map, planet, "mapX", "mapY", "id");
        map.Add(CreateRouteList(planet));
        return root;
    }

    private static VisualElement CreateRouteList(SerializedProperty planet)
    {
        var root = new VisualElement();
        SerializedObject asset = planet.serializedObject;
        string path = planet.FindPropertyRelative("prerequisitePlanetIds").propertyPath;
        SerializedProperty selectedId = planet.FindPropertyRelative("id");
        var catalog = (CampaignCatalog)asset.targetObject;
        var list = new VisualElement();
        root.Add(new Label("Planetas exigidos para liberar a rota"));
        root.Add(list);

        bool CanRequire(CampaignPlanet option) => option != null && !string.IsNullOrWhiteSpace(option.id) &&
            !DependsOn(catalog, option, selectedId.stringValue, new HashSet<string>());

        void Rebuild(SerializedProperty requirements)
        {
            list.Clear();
            var available = catalog.planets.Where(CanRequire).ToArray();
            var assigned = Enumerable.Range(0, requirements.arraySize)
                .Select(i => requirements.GetArrayElementAtIndex(i).stringValue).ToArray();
            if (assigned.Length == 0) list.Add(new Label("Disponível no início da campanha."));

            for (int i = 0; i < assigned.Length; i++)
            {
                int index = i;
                string value = assigned[index];
                var options = available.Where(option => option.id == value || !assigned.Contains(option.id)).ToArray();
                var captions = options.Select(option => $"{option.displayName} ({option.id})").ToList();
                int current = Array.FindIndex(options, option => option.id == value);
                if (current < 0)
                {
                    current = captions.Count;
                    captions.Add($"Referência inválida ({value})");
                }
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                var picker = new DropdownField(captions, current);
                picker.style.flexGrow = 1;
                picker.RegisterValueChangedCallback(_ =>
                {
                    if (picker.index >= 0 && picker.index < options.Length && CanRequire(options[picker.index]))
                        Modify(array => array.GetArrayElementAtIndex(index).stringValue = options[picker.index].id);
                    else
                        Rebuild(asset.FindProperty(path));
                });
                row.Add(picker);
                row.Add(new Button(() => Modify(array => array.DeleteArrayElementAtIndex(index))) { text = "Remover" });
                list.Add(row);
            }

            CampaignPlanet next = available.FirstOrDefault(option => !assigned.Contains(option.id));
            var add = new Button(() => Modify(array =>
            {
                if (!CanRequire(next)) return;
                int index = array.arraySize++;
                array.GetArrayElementAtIndex(index).stringValue = next.id;
            })) { text = "Adicionar pré-requisito" };
            add.SetEnabled(next != null);
            list.Add(add);
        }

        void Modify(Action<SerializedProperty> change)
        {
            asset.UpdateIfRequiredOrScript();
            SerializedProperty requirements = asset.FindProperty(path);
            change(requirements);
            asset.ApplyModifiedProperties();
            Rebuild(asset.FindProperty(path));
        }

        SerializedProperty initial = asset.FindProperty(path);
        Rebuild(initial);
        root.TrackPropertyValue(initial, Rebuild);
        return root;
    }

    internal static bool DependsOn(CampaignCatalog catalog, CampaignPlanet planet, string id, HashSet<string> visited)
    {
        if (planet.id == id) return true;
        if (!visited.Add(planet.id) || planet.prerequisitePlanetIds == null) return false;
        foreach (string source in planet.prerequisitePlanetIds)
        {
            CampaignPlanet prerequisite = catalog.GetPlanet(source);
            if (prerequisite != null && DependsOn(catalog, prerequisite, id, visited)) return true;
        }
        return false;
    }

    internal static Foldout AddGroup(VisualElement root, string name)
    {
        var group = new Foldout { text = name, value = false };
        root.Add(group);
        return group;
    }

    private static void AddFields(VisualElement parent, SerializedProperty owner, params string[] fields)
    {
        foreach (string field in fields) parent.Add(new PropertyField(owner.FindPropertyRelative(field)));
    }
}

[CustomEditor(typeof(CampaignSkill))]
public sealed class CampaignSkillInspector : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        AddFields(root, "displayName", "description", "icon", "maximumRank");

        var prices = CampaignPlanetDrawer.AddGroup(root, "Preços");
        prices.value = true;
        AddFields(prices, "soulCost", "unlockCost", "soulCostGrowth", "soulCostsByRank");
        SerializedProperty resource = serializedObject.FindProperty("requiredResourceId");
        prices.Add(CreateResourcePicker(resource));
        var materialPrices = new VisualElement();
        AddFields(materialPrices, "requiredResourceAmount", "resourceStartsAtRank", "resourceCostGrowth", "resourceCostsByRank");
        prices.Add(materialPrices);
        void ShowMaterialPrices(SerializedProperty value) => materialPrices.style.display =
            string.IsNullOrWhiteSpace(value.stringValue) ? DisplayStyle.None : DisplayStyle.Flex;
        ShowMaterialPrices(resource);
        root.TrackPropertyValue(resource, ShowMaterialPrices);

        var effects = CampaignPlanetDrawer.AddGroup(root, "Efeito");
        AddFields(effects, "effectType");
        var configuration = new VisualElement();
        effects.Add(configuration);
        void ShowEffectFields(SerializedProperty value)
        {
            configuration.Clear();
            switch ((CampaignSkillEffectType)value.enumValueIndex)
            {
                case CampaignSkillEffectType.None:
                    break;
                case CampaignSkillEffectType.WeaponUnlock:
                    AddFields(configuration, "unlockedWeapon");
                    break;
                case CampaignSkillEffectType.FlashDuration:
                case CampaignSkillEffectType.FlashArea:
                case CampaignSkillEffectType.FlashCapacity:
                case CampaignSkillEffectType.FlashRecharge:
                case CampaignSkillEffectType.FlashRepulsion:
                case CampaignSkillEffectType.EliteFlashRecovery:
                case CampaignSkillEffectType.BladeTurret:
                    var flash = new ObjectField("Dados compartilhados do Flash")
                        { objectType = typeof(FlashData), value = FlashData.Shared, allowSceneObjects = false };
                    flash.SetEnabled(false);
                    configuration.Add(flash);
                    var skill = (CampaignSkill)target;
                    Vector2 bonus = skill.EffectPerRank;
                    configuration.Add(new Label(bonus.y == 0f
                        ? $"Valor por nível aplicado pelo jogo: {bonus.x:0.###}"
                        : $"Valores por nível aplicados pelo jogo: {bonus.x:0.###} / {bonus.y:0.###}"));
                    AddFields(configuration, "effectStartsAtRank");
                    if ((CampaignSkillEffectType)value.enumValueIndex == CampaignSkillEffectType.BladeTurret)
                        AddFields(configuration, "effectPrefab");
                    break;
                default:
                    AddFields(configuration, "effectPerRank", "secondaryEffectPerRank", "effectStartsAtRank");
                    if ((CampaignSkillEffectType)value.enumValueIndex == CampaignSkillEffectType.MonkeyDecoy)
                        AddFields(configuration, "effectPrefab");
                    if ((CampaignSkillEffectType)value.enumValueIndex == CampaignSkillEffectType.GuardianBlades)
                        AddFields(configuration, "orbitalEffect");
                    break;
            }
        }
        SerializedProperty effect = serializedObject.FindProperty("effectType");
        ShowEffectFields(effect);
        root.TrackPropertyValue(effect, ShowEffectFields);

        var tree = CampaignPlanetDrawer.AddGroup(root, "Posição e pré-requisitos");
        AddFields(tree, "id", "editorPosition", "visibleAtStart", "requiresFlash", "requireAllPrerequisites",
            "prerequisites", "requireAllRevealPrerequisites", "revealPrerequisites", "exclusiveWith");
        return root;
    }

    private VisualElement CreateResourcePicker(SerializedProperty resource)
    {
        var catalog = Resources.Load<CampaignCatalog>("CampaignCatalog");
        if (catalog == null) return new PropertyField(resource);

        var options = catalog.planets.Where(planet => planet != null && !string.IsNullOrWhiteSpace(planet.rewardResourceId))
            .GroupBy(planet => planet.rewardResourceId).Select(group => group.First()).ToArray();
        var ids = new List<string> { string.Empty };
        ids.AddRange(options.Select(planet => planet.rewardResourceId));
        var names = new List<string> { "Somente almas" };
        names.AddRange(options.Select(planet => planet.rewardResourceName));
        if (!ids.Contains(resource.stringValue))
        {
            ids.Add(resource.stringValue);
            names.Add($"Material desconhecido ({resource.stringValue})");
        }

        var dropdown = new DropdownField("Material exigido", names, ids.IndexOf(resource.stringValue));
        dropdown.AddToClassList(BaseField<string>.alignedFieldUssClassName);
        dropdown.RegisterValueChangedCallback(_ =>
        {
            if (dropdown.index < 0) return;
            serializedObject.UpdateIfRequiredOrScript();
            resource.stringValue = ids[dropdown.index];
            serializedObject.ApplyModifiedProperties();
        });
        dropdown.TrackPropertyValue(resource, value =>
        {
            int index = ids.IndexOf(value.stringValue);
            if (index >= 0) dropdown.SetValueWithoutNotify(names[index]);
        });
        return dropdown;
    }

    private void AddFields(VisualElement parent, params string[] fields)
    {
        foreach (string field in fields) parent.Add(new PropertyField(serializedObject.FindProperty(field)));
    }
}
