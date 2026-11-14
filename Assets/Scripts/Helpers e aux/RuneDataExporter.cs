#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;

//[CreateAssetMenu(fileName = "RuneDataExporter", menuName = "Exporters/RuneData Exporter")]
public class RuneDataExporter : MonoBehaviour
{
    [Header("Arraste aqui os RuneData")]
    public List<RuneData> runeDataList;

    private const string ExportPath = "Assets/RuneDataExport.csv";

    [ContextMenu("Exportar para CSV")]
    public void ExportToCSV()
    {
        if (runeDataList == null || runeDataList.Count == 0)
        {
            Debug.LogWarning("A lista de RuneData está vazia.");
            return;
        }

        var sb = new StringBuilder();

        // Headers.
        sb.AppendLine("Name,Descrição English,Descrição Português (Brasil),Descrição Espanhol,Descrição Chinês Simplificado,Caminho,Preço,Max Preço,Mul. Preço,Buying Limit,Raridade");

        foreach (var rune in runeDataList)
        {
            if (rune == null) continue;

            sb.AppendLine(string.Join(",",
                Escape(rune.GetName(Language.english)),
                Escape(rune.GetDescription(Language.english)),
                Escape(rune.GetDescription(Language.portuguese)),
                Escape(rune.GetDescription(Language.spanish)),
                Escape(rune.GetDescription(Language.chinese)),
                rune.path,
                rune.price,
                rune.maxPrice,
                rune.priceMultiplier.ToString("0.##", CultureInfo.InvariantCulture),
                rune.numberOfTimesCanBeBought,
                rune.rarity
            ));
        }

        File.WriteAllText(ExportPath, sb.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();

        Debug.Log($"Exportação concluída para: {ExportPath}");
    }

    private string Escape(string input)
    {
        if (string.IsNullOrEmpty(input)) return "\"\"";
        return $"\"{input.Replace("\"", "\"\"")}\"";
    }
}
#endif
