using System;
using System.Collections.Generic;
using System.Linq;
using TINS.Campaign;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CampaignAuthoringValidation
{
    private const string CatalogPath = "Assets/TINS - New Mechanics/Campaign/Resources/CampaignCatalog.asset";
    private const string BasePath = "Assets/Scenes/Campaign/CampaignBase.unity";
    private const string MissionPath = "Assets/Scenes/HR Floor one.unity";

    [MenuItem("Tools/TINS/Validate Campaign Content")]
    private static void ValidateFromMenu()
    {
        List<string> issues = CollectIssues();
        if (issues.Count == 0) Debug.Log("Conteúdo da campanha validado.");
        foreach (string issue in issues) Debug.LogError(issue);
    }

    public static List<string> CollectIssues()
    {
        var issues = new List<string>();
        var catalog = AssetDatabase.LoadAssetAtPath<CampaignCatalog>(CatalogPath);
        if (catalog == null)
        {
            issues.Add("CampaignCatalog não encontrado.");
            return issues;
        }

        Scene hub = default;
        Scene mission = default;
        try
        {
            hub = EditorSceneManager.OpenPreviewScene(BasePath);
            mission = EditorSceneManager.OpenPreviewScene(MissionPath);
            ValidateScene(hub, typeof(CampaignBaseController), issues);
            ValidateScene(mission, typeof(CampaignMissionRuntime), issues);

            var runtime = Components<CampaignMissionRuntime>(mission).FirstOrDefault();
            if (runtime != null)
            {
                var fields = new SerializedObject(runtime);
                foreach (string name in new[] { "catalog", "tower", "lightReveal", "playerHealth", "spawner", "darkness", "statsDefinition", "runes" })
                    if (fields.FindProperty(name).objectReferenceValue == null)
                        issues.Add($"HR Floor one — campo obrigatório da missão: {name}.");
            }
            var navigation = Components<CampaignPlanetNavigation>(hub).FirstOrDefault();
            var markers = navigation == null
                ? Array.Empty<string>()
                : ReadMarkerIds(navigation, issues);

            ValidatePlanets(catalog, markers, issues);
            foreach (CampaignSkill skill in catalog.skills ?? Array.Empty<CampaignSkill>())
                if (skill != null && skill.effectType == CampaignSkillEffectType.WeaponUnlock && skill.GetUnlockedWeapon() == null)
                    issues.Add($"{skill.name} — atribua o GunData desbloqueado.");
        }
        finally
        {
            if (mission.IsValid()) EditorSceneManager.ClosePreviewScene(mission);
            if (hub.IsValid()) EditorSceneManager.ClosePreviewScene(hub);
        }
        return issues;
    }

    private static IEnumerable<T> Components<T>(Scene scene) where T : Component =>
        scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true));

    private static void ValidateScene(Scene scene, Type entryPoint, List<string> issues)
    {
        if (!EditorBuildSettings.scenes.Any(buildScene => buildScene.enabled && buildScene.path == scene.path))
            issues.Add($"{scene.name} — habilite a cena nas configurações de build.");
        if (scene.GetRootGameObjects().Sum(root => root.GetComponentsInChildren(entryPoint, true).Length) != 1)
            issues.Add($"{scene.name} — a cena deve ter exatamente um {entryPoint.Name}.");
        if (!Components<Camera>(scene).Any(camera => camera.CompareTag("MainCamera")))
            issues.Add($"{scene.name} — falta a câmera com tag MainCamera usada pelo prefab do jogador.");
        var player = Components<PlayerInputHandler>(scene).FirstOrDefault(input => input.CompareTag("Player") && !input.GetIsClone());
        if (player == null || new SerializedObject(player).FindProperty("mainCamera").objectReferenceValue == null)
            issues.Add($"{scene.name} — atribua a câmera no PlayerInputHandler do jogador.");
        if (!Components<HolderScriptableObjects>(scene).Any(holder =>
                holder.CompareTag("ObjectHolder") && holder.globalStats != null && holder.statsCalculator != null &&
                holder.runData != null && holder.runeInventory != null))
            issues.Add($"{scene.name} — ObjectHolder ausente ou incompleto para os componentes compartilhados.");
    }

    private static string[] ReadMarkerIds(CampaignPlanetNavigation navigation, List<string> issues)
    {
        SerializedProperty list = new SerializedObject(navigation).FindProperty("markers");
        var ids = new string[list.arraySize];
        for (int i = 0; i < list.arraySize; i++)
        {
            var marker = list.GetArrayElementAtIndex(i).objectReferenceValue as CampaignPlanetMarker;
            if (marker == null || marker.Button == null || marker.PlanetArt == null)
                issues.Add($"CampaignBase — marcador {i + 1} sem botão ou arte.");
            ids[i] = marker?.PlanetId;
        }
        return ids;
    }

    private static void ValidatePlanets(CampaignCatalog catalog, string[] markers, List<string> issues)
    {
        var planets = (catalog.planets ?? Array.Empty<CampaignPlanet>()).Where(planet => planet != null).ToArray();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var checkedPrefabs = new HashSet<GameObject>();
        var manifest = Resources.Load<TextAsset>("Mission Travel/planetas-estaticos");
        var art = manifest != null ? JsonUtility.FromJson<TravelArt>(manifest.text) : null;

        foreach (CampaignPlanet planet in planets)
        {
            if (string.IsNullOrWhiteSpace(planet.id) || !ids.Add(planet.id))
                issues.Add($"{planet.displayName} — ID vazio ou duplicado: {planet.id}.");
            if (markers.Count(id => id == planet.id) != 1)
                issues.Add($"{planet.displayName} — deve haver um marcador no mapa para {planet.id}.");
            foreach (string stage in new[] { "escuro", "luz-25", "luz-75", "iluminado" })
                if (art?.entries?.Any(entry => entry.planet_id == planet.id && entry.lighting_id == stage) != true)
                    issues.Add($"{planet.displayName} — falta a arte de viagem do estágio {stage}.");
            for (int stage = 0; stage < 5; stage++)
            {
                string path = $"Assets/TINS - New Mechanics/Campaign/Resources/PlanetRotations/{planet.id}/{planet.id}-{stage}.png";
                if (AssetDatabase.GetMainAssetTypeAtPath(path) != typeof(Texture2D))
                    issues.Add($"{planet.displayName} — falta a rotação do mapa no estágio {stage}.");
            }

            var roster = planet.enemySettings;
            if (roster == null || roster.enemies == null ||
                !roster.enemies.Any(entry => entry != null && !entry.isElite && entry.prefab != null &&
                    entry.weight > 0f && entry.minimumLightRadius <= planet.initialLightRadius))
            {
                issues.Add($"{planet.displayName} — o elenco precisa de um inimigo comum disponível no raio inicial.");
            }
            if (roster == null) continue;
            foreach (CampaignEnemyEntry entry in roster.enemies ?? new List<CampaignEnemyEntry>())
            {
                ValidateEnemy(entry?.prefab, roster.name, checkedPrefabs, issues);
                if (entry?.fourDirectionPrefab != null)
                    ValidateEnemy(entry.fourDirectionPrefab, roster.name, checkedPrefabs, issues);
            }
            if (roster.finalBoss != null) ValidateEnemy(roster.finalBoss, roster.name, checkedPrefabs, issues);
        }

        foreach (CampaignPlanet planet in planets)
            foreach (string prerequisite in planet.prerequisitePlanetIds ?? Array.Empty<string>())
            {
                CampaignPlanet source = catalog.GetPlanet(prerequisite);
                if (source == null || CampaignPlanetDrawer.DependsOn(catalog, source, planet.id, new HashSet<string>()))
                    issues.Add($"{planet.displayName} — rota inválida ou circular: {prerequisite}.");
            }
    }

    private static void ValidateEnemy(GameObject prefab, string roster, HashSet<GameObject> checkedPrefabs, List<string> issues)
    {
        if (prefab == null)
        {
            issues.Add($"{roster} — há uma entrada sem prefab de inimigo.");
            return;
        }
        if (!checkedPrefabs.Add(prefab)) return;
        if (!prefab.TryGetComponent(out EnemyHealth health) || health.enemyData == null || !prefab.TryGetComponent<PoolCreator>(out _))
            issues.Add($"{prefab.name} — a raiz precisa de EnemyHealth, EnemyData e PoolCreator.");
        if (!prefab.TryGetComponent(out EnemyTowerAttack attack)) return;
        var settings = new SerializedObject(attack);
        if (!settings.FindProperty("useAnimationEvent").boolValue) return;
        var animator = settings.FindProperty("attackAnimator").objectReferenceValue as Animator ?? prefab.GetComponent<Animator>();
        string trigger = settings.FindProperty("attackTrigger").stringValue;
        var overrideController = animator?.runtimeAnimatorController as AnimatorOverrideController;
        var controller = (overrideController?.runtimeAnimatorController ?? animator?.runtimeAnimatorController) as AnimatorController;
        if (animator == null || animator.gameObject != attack.gameObject || controller == null ||
            !controller.parameters.Any(parameter => parameter.name == trigger && parameter.type == AnimatorControllerParameterType.Trigger))
        {
            issues.Add($"{prefab.name} — o Animator do ataque precisa estar na raiz e ter o trigger {trigger}.");
            return;
        }

        var clips = controller.layers.SelectMany(layer => FindAttackStates(layer.stateMachine, trigger))
            .SelectMany(state => AnimationClips(state.motion)).Distinct().ToArray();
        if (clips.Length == 0) issues.Add($"{prefab.name} — nenhuma transição usa {trigger} para tocar um ataque.");
        foreach (AnimationClip original in clips)
        {
            AnimationClip clip = overrideController?[original] ?? original;
            if (!AnimationUtility.GetAnimationEvents(clip).Any(animationEvent => animationEvent.functionName == nameof(EnemyTowerAttack.AnimationImpact)))
                issues.Add($"{prefab.name} — o clipe {clip.name} precisa do evento AnimationImpact.");
        }
    }

    private static IEnumerable<AnimatorState> FindAttackStates(AnimatorStateMachine machine, string trigger)
    {
        var transitions = machine.anyStateTransitions.Concat(machine.states.SelectMany(state => state.state.transitions));
        foreach (AnimatorStateTransition transition in transitions)
            if (transition.destinationState != null && transition.conditions.Any(condition => condition.parameter == trigger))
                yield return transition.destinationState;
        foreach (ChildAnimatorStateMachine child in machine.stateMachines)
            foreach (AnimatorState target in FindAttackStates(child.stateMachine, trigger))
                yield return target;
    }

    private static IEnumerable<AnimationClip> AnimationClips(Motion motion)
    {
        if (motion is AnimationClip clip) yield return clip;
        if (motion is BlendTree tree)
            foreach (ChildMotion child in tree.children)
                foreach (AnimationClip childClip in AnimationClips(child.motion))
                    yield return childClip;
    }

    [Serializable] private sealed class TravelArt { public TravelEntry[] entries; }
    [Serializable] private sealed class TravelEntry { public string planet_id; public string lighting_id; }
}
