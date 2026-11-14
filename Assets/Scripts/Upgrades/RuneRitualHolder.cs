using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds rune choices for RuneRitualManager.
/// </summary>
[CreateAssetMenu(fileName = "new rune ritual Holder", menuName = "Upgrades/rune ritual Holder")]
public class RuneRitualHolder : GenericData
{
    [SerializeField] private List<RuneData> runes;
    [SerializeField] private Sprite icon;

    public List<RuneData> GetRunes()
    {
        return runes;
    }

    [ContextMenu("DoubleChckDuplicates")]

    public void DoubleChckDuplicates()
    {
        HashSet<RuneData> uniqueRunes = new HashSet<RuneData>();
        foreach (var rune in runes)
        {
            if (!uniqueRunes.Add(rune))
            {
                Debug.LogWarning($"Duplicate rune found: {rune.name}");
            }
        }

        runes = new List<RuneData>(uniqueRunes);
    }

    public Sprite GetIcon() { return icon; }
}
