using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class WaveLoader : MonoBehaviour
{
    [Header("CSV File (drag and drop from Resources or path)")]
    public TextAsset csvFile;

    [Header("Wave Scriptable Objects (must match CSV row count)")]
    public List<EnemyWave> waveScriptableObjects;

    void Start()
    {
        if (csvFile == null)
        {
            Debug.LogError("CSV file not assigned.");
            return;
        }

       // LoadCSVAndApply();
    }
    [ContextMenu(nameof(ImportWavesFromCsv))]
    void ImportWavesFromCsv()
    {
        string[] lines = csvFile.text.Split('\n');

        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV file is empty or contains only headers.");
            return;
        }

        for (int i = 1; i < lines.Length; i++) // Start at 1 to skip header
        {
            if (i - 1 >= waveScriptableObjects.Count)
            {
                Debug.LogWarning($"CSV line {i} has no corresponding ScriptableObject.");
                break;
            }

            string line = lines[i].Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] values = line.Split(',');

            if (values.Length < 4)
            {
                Debug.LogWarning($"Invalid data at line {i + 1}. Expected 4 values, got {values.Length}.");
                continue;
            }

            if (!int.TryParse(values[0], out int wave))
                continue;
            if (!int.TryParse(values[1], out int minI))
                continue;
            if (!int.TryParse(values[2], out int maxI))
                continue;
            if (!int.TryParse(values[3], out int pw))
                continue;

            EnemyWave targetWave = waveScriptableObjects[i - 1];
            //targetWave.wave = wave;
            targetWave.minEnemies = minI;
            targetWave.maxEnemies = maxI;
            targetWave.spawnPoints = pw;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(targetWave);
#endif
        }

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        Debug.Log("CSV data successfully applied to ScriptableObjects.");
    }
}