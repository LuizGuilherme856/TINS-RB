#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// TechBooh publisher patch. Importing does not modify the game.
/// Independent of the Yodinha installer; only EnemyIndicator.cs is patched.
/// </summary>
public static class CthulhuArrowPatchInstaller
{
    private const string Marker = "TECHBOOH_CTHULHU_ARROW_PATCH_V1";

    [MenuItem("TechBooh/Patches/Cthulhu Arrow/1 - Preview")]
    public static void Preview_TechBooh() => Run_TechBooh(false);

    [MenuItem("TechBooh/Patches/Cthulhu Arrow/2 - Apply")]
    public static void Apply_TechBooh() => Run_TechBooh(true);

    private static void Run_TechBooh(bool apply)
    {
        try
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                throw new InvalidOperationException("Saia do Play Mode e aguarde a compilacao.");
            var paths = AssetDatabase.GetAllAssetPaths().Where(path =>
                path.StartsWith("Assets/", StringComparison.Ordinal) && Path.GetFileName(path) == "EnemyIndicator.cs").ToArray();
            if (paths.Length != 1) throw new InvalidOperationException("Era esperado exatamente um EnemyIndicator.cs. Encontrados: " + paths.Length);
            string root = Directory.GetParent(Application.dataPath).FullName;
            string file = Path.Combine(root, paths[0]);
            string original = File.ReadAllText(file);
            string patched = PatchSourceForVerification_TechBooh(original);
            if (original == patched)
            {
                EditorUtility.DisplayDialog("TechBooh - Cthulhu", "O marcador deste patch ja esta presente. Confira a compilacao e os testes de gameplay.", "OK");
                return;
            }
            string description = "Arquivo: " + paths[0] + "\n\n" +
                "A seta usara o filho com CthulhuBossManager, em vez do pai parado. " +
                "Posicao e direcao serao atualizadas juntas a cada frame; alvos desativados serao ocultados.\n\n" +
                "Nao altera cenas, prefabs, DLC ou personagens. Funciona nas copias de PC e consoles.";
            if (!apply)
            {
                Debug.Log(description);
                EditorUtility.DisplayDialog("Previa do patch", description + "\n\nNenhum arquivo foi alterado.", "OK");
                return;
            }
            if (!EditorUtility.DisplayDialog("Aplicar patch independente", description + "\n\nFaca um commit antes de aplicar. Continuar?", "Aplicar", "Cancelar")) return;
            if ((File.GetAttributes(file) & FileAttributes.ReadOnly) != 0)
                throw new IOException("O arquivo esta somente leitura: " + file);
            string backup = Path.Combine(root, "TechBoohBackups", "CthulhuArrow",
                DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            string backupFile = Path.Combine(backup, paths[0]);
            Directory.CreateDirectory(Path.GetDirectoryName(backupFile));
            File.Copy(file, backupFile);
            if (File.Exists(file + ".meta")) File.Copy(file + ".meta", backupFile + ".meta");
            File.WriteAllText(Path.Combine(backup, "manifest.txt"), paths[0]);
            try { File.WriteAllText(file, patched, new UTF8Encoding(true)); }
            catch { File.Copy(backupFile, file, true); throw; }
            AssetDatabase.ImportAsset(paths[0], ImportAssetOptions.ForceUpdate);
            Debug.Log("TechBooh: patch da seta aplicado. Backup: " + backup);
            EditorUtility.DisplayDialog("Patch aplicado", "Aguarde a compilacao e teste os teleportes, a morte do boss e os demais inimigos.\n\nBackup: " + backup, "OK");
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorUtility.DisplayDialog("Falha no patch", error.Message, "OK");
        }
    }

    // Pure transformation: validates every expected fragment before any file is written.
    public static string PatchSourceForVerification_TechBooh(string source)
    {
        if (source.Contains(Marker)) return source;
        string patched = ReplaceOnce_TechBooh(source,
            @"Vector3\s+enemyWorldPosition\s*=\s*enemy\.transform\.position\s*;",
            "Vector3 enemyWorldPosition = GetIndicatorTarget_TechBooh(enemy).position;");
        patched = ReplaceOnce_TechBooh(patched,
            @"RotateArrowTowardsEnemy\(activeArrows\[i\]\.transform,\s*currentEnemies\[i\]\.transform\.position\);",
            "RotateArrowTowardsEnemy(activeArrows[i].transform, GetIndicatorTarget_TechBooh(currentEnemies[i]).position);");
        patched = ReplaceOnce_TechBooh(patched, @"if\s*\(arrowRect\s*!=\s*null\)\s*\{", @"if (arrowRect != null)
            {
                // TechBooh: a teleport moves the boss child, not its counting parent.
                // Refresh position and direction together, independently of the scan interval.
                if (currentEnemies == null || i >= currentEnemies.Length || currentEnemies[i] == null ||
                    !currentEnemies[i].isActiveAndEnabled || !GetIndicatorTarget_TechBooh(currentEnemies[i]).gameObject.activeInHierarchy)
                {
                    activeArrows[i].SetActive(false);
                    continue;
                }
                targetPositions[i] = CalculateArrowTargetPosition(currentEnemies[i]);");
        patched = ReplaceOnce_TechBooh(patched,
            @"Vector2\s+direction\s*=\s*\(Vector2\)\(enemyWorldPosition\s*-\s*mainCamera\.transform\.position\);",
            @"// TechBooh: measure from the visible arrow to the target in screen space.
        // Using the camera's world position made the direction wrong near screen edges.
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 arrowScreenPosition = RectTransformUtility.WorldToScreenPoint(uiCamera, arrowTransform.position);
        Vector3 targetScreenPosition = mainCamera.WorldToScreenPoint(enemyWorldPosition);
        if (targetScreenPosition.z < 0)
        {
            targetScreenPosition.x = Screen.width - targetScreenPosition.x;
            targetScreenPosition.y = Screen.height - targetScreenPosition.y;
        }
        Vector2 direction = (Vector2)targetScreenPosition - arrowScreenPosition;
        if (direction.sqrMagnitude < 0.0001f) return;");
        patched = ReplaceOnce_TechBooh(patched,
            @"private\s+Vector2\s+CalculateArrowTargetPosition\(CountEnemies_Helper\s+enemy\)",
            @"// TechBooh: retain the counting marker for wave bookkeeping, but follow the actual boss.
    // Ordinary enemies keep their original target. No prefab or combat changes are required.
    private Transform GetIndicatorTarget_TechBooh(CountEnemies_Helper enemy)
    {
        var boss = enemy.GetComponentInChildren<CthulhuBossManager>(true);
        return boss != null ? boss.transform : enemy.transform;
    }

    private Vector2 CalculateArrowTargetPosition(CountEnemies_Helper enemy)");
        return "// " + Marker + ": independent publisher fix, PC and consoles.\n" + patched;
    }

    private static string ReplaceOnce_TechBooh(string source, string pattern, string replacement)
    {
        var regex = new Regex(pattern);
        if (regex.Matches(source).Count != 1)
            throw new InvalidOperationException("Versao de EnemyIndicator.cs incompativel. Nenhuma alteracao aplicada. Trecho esperado: " + pattern);
        return regex.Replace(source, _ => replacement, 1);
    }
}
#endif
