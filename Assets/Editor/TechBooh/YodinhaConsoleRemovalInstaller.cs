#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>
/// TechBooh publisher tool. Import this ONE file into Assets/Editor of the CONSOLE copy.
/// Importing does nothing. Preview first, then explicitly Apply. No project-specific types
/// are linked by this installer, so relocated runtime scripts are located by their names.
/// </summary>
public static class YodinhaConsoleRemovalInstaller
{
    private const string Marker = "TECHBOOH_YODINHA_CONSOLE_REMOVED_V1";
    private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
    private sealed class Plan
    {
        public readonly Dictionary<string, string> Code = new Dictionary<string, string>();
        public readonly HashSet<string> Forbidden = new HashSet<string>();
        public readonly List<string> Assets = new List<string>();
        public Object Character, FallbackCharacter;
        public Sprite FallbackIcon;
        public RuntimeAnimatorController FallbackAnimator;
    }

    [MenuItem("TechBooh/Console/Remove Yodinha/1 - Preview")]
    public static void Preview_TechBooh()
    {
        try
        {
            var plan = Prepare_TechBooh();
            var text = Describe_TechBooh(plan);
            Debug.Log(text);
            EditorUtility.DisplayDialog("Yodinha removal - preview", text + "\n\nNo files were changed.", "OK");
        }
        catch (Exception error) { Debug.LogException(error); EditorUtility.DisplayDialog("Preview failed", error.Message, "OK"); }
    }

    [MenuItem("TechBooh/Console/Remove Yodinha/2 - Apply to CONSOLE copy")]
    public static void Apply_TechBooh()
    {
        string backup = null;
        bool success = false;
        try
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                throw new InvalidOperationException("Exit Play Mode and wait for compilation first.");
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                throw new InvalidOperationException("Close Prefab Mode before applying.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save all open scenes before applying.");

            var plan = Prepare_TechBooh(); // Validate EVERY source patch before writing anything.
            if (!EditorUtility.DisplayDialog("Apply ONLY to the console project copy",
                Describe_TechBooh(plan) + "\n\nThis permanently disables Yodinha in this project, including PC builds made from this copy. " +
                "Use a dedicated console branch/copy. Backups will be created outside Assets. Continue?", "Apply", "Cancel")) return;

            var files = plan.Code.Keys.Concat(plan.Assets).Distinct().ToArray();
            foreach (var path in files)
                if ((File.GetAttributes(Absolute_TechBooh(path)) & FileAttributes.ReadOnly) != 0)
                    throw new IOException("Check out this read-only file first: " + path);

            backup = Path.Combine(ProjectRoot, "TechBoohBackups", "Yodinha", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            foreach (var path in files)
            {
                BackupFile_TechBooh(path, backup);
                if (File.Exists(Absolute_TechBooh(path + ".meta"))) BackupFile_TechBooh(path + ".meta", backup);
            }
            File.WriteAllLines(Path.Combine(backup, "manifest.txt"), files);
            var log = new List<string> { Describe_TechBooh(plan), "Backup: " + backup };
            EditorApplication.LockReloadAssemblies();
            AssetDatabase.DisallowAutoRefresh();
            try
            {
                // Prefab/scene changes use Unity APIs; never edit their YAML by text replacement.
                for (int i = 0; i < plan.Assets.Count; i++)
                {
                    var path = plan.Assets[i];
                    EditorUtility.DisplayProgressBar("Removing Yodinha", path, (float)i / Math.Max(1, plan.Assets.Count));
                    ProcessAsset_TechBooh(path, plan, log);
                }
                foreach (var patch in plan.Code)
                    File.WriteAllText(Absolute_TechBooh(patch.Key), patch.Value, new UTF8Encoding(true));
                AssetDatabase.SaveAssets();
                File.WriteAllLines(Path.Combine(backup, "report.txt"), log);
                success = true;
            }
            catch
            {
                // Restore every file from the transaction, not just the last modified asset.
                foreach (var path in files)
                    File.Copy(Path.Combine(backup, path), Absolute_TechBooh(path), true);
                throw;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.AllowAutoRefresh();
                EditorApplication.UnlockReloadAssemblies();
                AssetDatabase.Refresh();
            }
            Debug.Log("TechBooh: Yodinha removed from console copy. Backup/report: " + backup);
            EditorUtility.DisplayDialog("Applied - compilation and validation required",
                "Runtime selection is blocked and scene/prefab references were cleaned.\n\n" +
                "Wait for compilation, then run '3 - Audit build references'. Do not publish until that audit and gameplay tests pass.\n\nBackup: " + backup, "OK");
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorUtility.DisplayDialog("Removal failed", error.Message + (backup == null ? "" : "\n\nBackup: " + backup) +
                (success ? "" : "\nNo successful installation was confirmed. Reopen affected scenes after a rollback."), "OK");
        }
    }

    [MenuItem("TechBooh/Console/Remove Yodinha/3 - Audit build references")]
    public static void Audit_TechBooh()
    {
        try
        {
            var plan = Prepare_TechBooh();
            var roots = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path)
                .Concat(AssetDatabase.GetAllAssetPaths().Where(path => path.StartsWith("Assets/", StringComparison.Ordinal) &&
                    (path.IndexOf("/Resources/", StringComparison.OrdinalIgnoreCase) >= 0 || path.EndsWith(".spriteatlas", StringComparison.OrdinalIgnoreCase) ||
                     path.EndsWith(".spriteatlasv2", StringComparison.OrdinalIgnoreCase)))).Distinct().ToArray();
            var remaining = AssetDatabase.GetDependencies(roots, true).Where(plan.Forbidden.Contains).ToArray();
            var addressableWarnings = AssetDatabase.GetAllAssetPaths().Where(path => path.StartsWith("Assets/AddressableAssetsData/", StringComparison.Ordinal) && path.EndsWith(".asset"))
                .Where(path => plan.Forbidden.Any(forbidden => File.ReadAllText(Absolute_TechBooh(path)).Contains(AssetDatabase.AssetPathToGUID(forbidden)))).ToArray();
            string result = "Remaining forbidden build dependencies: " + remaining.Length + "\n" + string.Join("\n", remaining) +
                "\nAddressables files requiring inspection: " + addressableWarnings.Length + "\n" + string.Join("\n", addressableWarnings) +
                "\n\nAlso check custom AssetBundles, custom build pipelines and the final Build Report. This audit does not inspect external build scripts.";
            if (remaining.Length > 0 || addressableWarnings.Length > 0) Debug.LogError(result); else Debug.Log(result);
            EditorUtility.DisplayDialog("Yodinha build audit", result, "OK");
        }
        catch (Exception error) { Debug.LogException(error); EditorUtility.DisplayDialog("Audit failed", error.Message, "OK"); }
    }

    private static Plan Prepare_TechBooh()
    {
        var plan = new Plan();
        var paths = AssetDatabase.GetAllAssetPaths();
        foreach (var path in paths.Where(path => path.StartsWith("Assets/", StringComparison.Ordinal) && !AssetDatabase.IsValidFolder(path)))
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension == ".cs" || extension == ".md" || extension == ".meta") continue;
            string normalized = path.ToLowerInvariant();
            if (normalized.Contains("yodinha") || normalized.Contains("yodista")) plan.Forbidden.Add(path);
        }
        foreach (var guid in AssetDatabase.FindAssets("t:CharacterData", new[] { "Assets" }))
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid));
            var data = new SerializedObject(asset);
            var character = data.FindProperty("character");
            if (character == null) continue;
            if (character.intValue == 6) { plan.Character = asset; plan.Forbidden.Add(AssetDatabase.GetAssetPath(asset)); }
            if (character.intValue == 0 && plan.FallbackCharacter == null)
            {
                plan.FallbackCharacter = asset;
                plan.FallbackIcon = data.FindProperty("smallIcon")?.objectReferenceValue as Sprite;
                plan.FallbackAnimator = data.FindProperty("<animatorController>k__BackingField")?.objectReferenceValue as RuntimeAnimatorController;
            }
        }
        if (plan.Character == null || plan.FallbackCharacter == null || plan.FallbackIcon == null || plan.FallbackAnimator == null)
            throw new InvalidOperationException("Expected Yodinha (character 6) and Azaus (character 0, icon and Animator) assets were not found. Nothing will be modified.");

        foreach (var name in new[] { "CharacterDataManager", "HubManager", "ChangeCharacter", "DlcManager", "HighScoreTableRow" })
        {
            var matching = paths.Where(path => path.StartsWith("Assets/", StringComparison.Ordinal) && Path.GetFileName(path) == name + ".cs").ToArray();
            if (matching.Length != 1) throw new InvalidOperationException("Expected exactly one script named " + name + ".cs. Found: " + matching.Length);
            var source = File.ReadAllText(Absolute_TechBooh(matching[0]));
            var patched = PatchSourceForVerification_TechBooh(name, source);
            if (source != patched) plan.Code.Add(matching[0], patched);
        }
        foreach (var path in paths.Where(path => path.StartsWith("Assets/", StringComparison.Ordinal) &&
            (path.EndsWith(".prefab") || path.EndsWith(".unity") || path.EndsWith(".asset")) && !plan.Forbidden.Contains(path)))
        {
            if (path.Contains("/Packages") || path.Contains("/_Recovery/") || path.Contains("/AddressableAssetsData/")) continue;
            // Include direct dependencies only; cleaning a prefab does not justify rewriting every scene that uses it.
            if (AssetDatabase.GetDependencies(path, false).Any(plan.Forbidden.Contains)) plan.Assets.Add(path);
        }
        plan.Assets.Sort(StringComparer.Ordinal); // Prefabs before scenes; original assets remain in the project.
        return plan;
    }

    public static string PatchSourceForVerification_TechBooh(string name, string source)
    {
        if (source.Contains(Marker)) return source;
        string result = source;
        switch (name)
        {
            case "DlcManager":
                result = ReplaceBody_TechBooh(result, @"public\s+bool\s+CheckDlc\s*\(\s*\)",
                    "\n        // TechBooh console copy: ownership can never enable removed content.\n        hasDlc = false;\n        return false;\n    ");
                break;
            case "HubManager":
                result = ReplaceOnce_TechBooh(result, @"case\s+6\s*:\s*unlocked\s*=\s*hasDlc\s*;\s*break\s*;", "case 6: unlocked = false; break; // TechBooh: Yodinha is unavailable on consoles.");
                break;
            case "ChangeCharacter":
                result = Inject_TechBooh(result, @"private\s+void\s+TriggerChangeCharacter\s*\(\s*\)",
                    "\n        // TechBooh: reject before feedbacks, relocation or equipment changes.\n        if (characterData == null || characterData.character == Character.yodista) return;\n");
                result = ReplaceOnce_TechBooh(result, @"previousNpc\s*!=\s*null\)", "previousNpc != null && previousCharacter != Character.yodista)");
                break;
            case "HighScoreTableRow":
                result = ReplaceOnce_TechBooh(result, @"case\s+Character\.yodista\s*:\s*IconImage\.sprite\s*=\s*charIcons\[4\]\s*;",
                    "case Character.yodista:\n                // TechBooh: keep historical enum values, but use a supported portrait.\n                IconImage.sprite = charIcons[0];");
                break;
            case "CharacterDataManager":
                result = ReplaceOnce_TechBooh(result, @"public\s+void\s+SetCharacterData\s*\(CharacterData\s+data\)\s*=>\s*this\.currentCharacterData\s*=\s*data\s*;",
                    "public void SetCharacterData(CharacterData data) => this.currentCharacterData = ResolveConsoleCharacter_TechBooh(data);");
                result = ReplaceOnce_TechBooh(result, @"CharacterData\s+data\s*=\s*runData\.currentCharacter\s*;",
                    "CharacterData data = ResolveConsoleCharacter_TechBooh(runData.currentCharacter);\n        runData.currentCharacter = data; // TechBooh: sanitize legacy selections before applying the Animator.");
                result = Inject_TechBooh(result, @"public\s+void\s+SetCharacter\s*\(Character\s+character\)",
                    "\n        if (character == Character.yodista) character = Character.azaus; // TechBooh console restriction.\n");
                int closing = result.LastIndexOf('}');
                if (closing < 0) throw new InvalidOperationException("CharacterDataManager class ending not found.");
                result = result.Insert(closing,
                    "\n    // TechBooh: a missing/unsupported character falls back to Azaus; never load Yodinha.\n" +
                    "    private CharacterData ResolveConsoleCharacter_TechBooh(CharacterData data)\n    {\n" +
                    "        if (data != null && data.character != Character.yodista) return data;\n" +
                    "        var fallback = charactersData.Find(item => item != null && item.character == Character.azaus);\n" +
                    "        if (fallback == null) throw new System.InvalidOperationException(\"Console character fallback Azaus is missing.\");\n" +
                    "        return fallback;\n    }\n");
                break;
            default: throw new InvalidOperationException("Unsupported source: " + name);
        }
        return "// " + Marker + ": applied to a dedicated CONSOLE project copy.\n" + result;
    }

    private static string ReplaceOnce_TechBooh(string source, string pattern, string replacement)
    {
        var expression = new Regex(pattern);
        if (expression.Matches(source).Count != 1) throw new InvalidOperationException("Source version mismatch. Expected one match for: " + pattern);
        return expression.Replace(source, _ => replacement, 1);
    }

    private static string Inject_TechBooh(string source, string signature, string statements)
    {
        var expression = new Regex(signature + @"\s*\{");
        if (expression.Matches(source).Count != 1) throw new InvalidOperationException("Method version mismatch: " + signature);
        return expression.Replace(source, match => match.Value + statements, 1);
    }

    private static string ReplaceBody_TechBooh(string source, string signature, string body)
    {
        var expression = new Regex(signature + @"\s*\{");
        var matches = expression.Matches(source);
        if (matches.Count != 1) throw new InvalidOperationException("Method version mismatch: " + signature);
        int opening = matches[0].Index + matches[0].Length - 1;
        int closing = FindClosingBrace_TechBooh(source, opening);
        return source.Substring(0, opening + 1) + body + source.Substring(closing);
    }

    private static int FindClosingBrace_TechBooh(string source, int start)
    {
        int depth = 0;
        bool lineComment = false, blockComment = false, quoted = false, verbatim = false, character = false;
        for (int i = start; i < source.Length; i++)
        {
            char c = source[i], next = i + 1 < source.Length ? source[i + 1] : '\0';
            if (lineComment) { if (c == '\n') lineComment = false; continue; }
            if (blockComment) { if (c == '*' && next == '/') { blockComment = false; i++; } continue; }
            if (quoted || character)
            {
                if (!verbatim && c == '\\') { i++; continue; }
                if (quoted && c == '"') { if (verbatim && next == '"') { i++; continue; } quoted = false; verbatim = false; }
                if (character && c == '\'') character = false;
                continue;
            }
            if (c == '/' && next == '/') { lineComment = true; i++; continue; }
            if (c == '/' && next == '*') { blockComment = true; i++; continue; }
            if (c == '"') { quoted = true; verbatim = i > 0 && source[i - 1] == '@'; continue; }
            if (c == '\'') { character = true; continue; }
            if (c == '{') depth++;
            if (c == '}' && --depth == 0) return i;
        }
        throw new InvalidOperationException("Unbalanced source braces. No changes were applied.");
    }

    private static void ProcessAsset_TechBooh(string path, Plan plan, List<string> log)
    {
        if (path.EndsWith(".prefab"))
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (CleanHierarchy_TechBooh(new[] { root }, plan))
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path, out bool saved);
                    if (!saved) throw new IOException("Could not save prefab: " + path);
                    log.Add("Prefab: " + path);
                }
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        else if (path.EndsWith(".unity"))
        {
            var scene = SceneManager.GetSceneByPath(path);
            bool alreadyOpen = scene.IsValid() && scene.isLoaded;
            if (!alreadyOpen) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                if (CleanHierarchy_TechBooh(scene.GetRootGameObjects(), plan))
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save scene: " + path);
                    log.Add("Scene: " + path);
                }
            }
            finally { if (!alreadyOpen) EditorSceneManager.CloseScene(scene, true); }
        }
        else
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                if (asset != null && CleanObject_TechBooh(asset, plan, new HashSet<Object>())) { EditorUtility.SetDirty(asset); log.Add("Asset: " + path); }
        }
    }

    private static bool CleanHierarchy_TechBooh(GameObject[] roots, Plan plan)
    {
        var components = roots.SelectMany(root => root.GetComponentsInChildren<Component>(true)).Where(item => item != null).ToArray();
        var remove = new HashSet<GameObject>();
        foreach (var component in components.Where(item => item.GetType().Name == "ChangeCharacter"))
        {
            var data = new SerializedObject(component).FindProperty("characterData");
            if (data?.objectReferenceValue != plan.Character) continue;
            // Never delete the shared Hanba prefab asset or an outer prefab holding the entire Hub.
            var npc = PrefabUtility.GetNearestPrefabInstanceRoot(component.gameObject);
            if (npc == null || npc.GetComponentsInChildren<Component>(true).Count(item => item != null && item.GetType().Name == "ChangeCharacter") != 1 ||
                npc.GetComponentsInChildren<Component>(true).Any(item => item != null &&
                    (item.GetType().Name == "HubManager" || item.GetType().Name == "CharacterDataManager")))
                npc = component.gameObject;
            // TechBooh: an unexpectedly named standalone NPC prefab needs manual review.
            // Never destroy the loaded prefab root or silently save a broken prefab asset.
            if (roots.Contains(npc) && EditorSceneManager.IsPreviewSceneObject(npc))
                throw new InvalidOperationException("Yodinha occupies a standalone prefab root: " + npc.name +
                    ". Remove this prefab's console build references manually, then retry. Shared NPC prefabs must not be deleted.");
            remove.Add(npc);
        }
        var removedObjects = new HashSet<Object>();
        foreach (var npc in remove)
            foreach (var transform in npc.GetComponentsInChildren<Transform>(true))
            {
                removedObjects.Add(transform.gameObject);
                foreach (var component in transform.GetComponents<Component>()) if (component != null) removedObjects.Add(component);
            }
        bool changed = remove.Count > 0;
        foreach (var component in components)
            if (!removedObjects.Contains(component)) changed |= CleanObject_TechBooh(component, plan, removedObjects);
        foreach (var npc in remove) if (npc != null) Object.DestroyImmediate(npc);
        return changed;
    }

    private static bool CleanObject_TechBooh(Object target, Plan plan, HashSet<Object> removed)
    {
        var data = new SerializedObject(target);
        var property = data.GetIterator();
        bool changed = false;
        while (property.Next(true))
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference || property.name == "m_Script") continue;
            var reference = property.objectReferenceValue;
            if (reference == null) continue;
            bool forbidden = plan.Forbidden.Contains(AssetDatabase.GetAssetPath(reference));
            if (!forbidden && !removed.Contains(reference) && property.name != "yodinha" && property.name != "yodinhaCorpse") continue;
            Object replacement = null;
            if (reference == plan.Character && (property.name == "currentCharacter" || property.name == "currentCharacterData")) replacement = plan.FallbackCharacter;
            else if (forbidden && reference is Sprite) replacement = plan.FallbackIcon;
            else if (forbidden && reference is RuntimeAnimatorController) replacement = plan.FallbackAnimator;
            property.objectReferenceValue = replacement;
            changed = true;
        }
        if (changed) data.ApplyModifiedPropertiesWithoutUndo();
        return changed;
    }

    private static string Absolute_TechBooh(string path) => Path.Combine(ProjectRoot, path);
    private static void BackupFile_TechBooh(string path, string backup)
    {
        var destination = Path.Combine(backup, path);
        Directory.CreateDirectory(Path.GetDirectoryName(destination));
        File.Copy(Absolute_TechBooh(path), destination, false);
    }
    private static string Describe_TechBooh(Plan plan) => "TechBooh console-only Yodinha removal\n" +
        "Runtime scripts to patch: " + plan.Code.Count + "\n" + string.Join("\n", plan.Code.Keys) +
        "\nScenes/prefabs/assets to inspect: " + plan.Assets.Count + "\nExclusive asset paths to exclude from builds: " + plan.Forbidden.Count +
        "\nEnum values and existing score records will be preserved. Exclusive source assets are NOT deleted.";
}
#endif
