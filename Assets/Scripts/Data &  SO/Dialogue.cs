using System.Collections.Generic;
using UnityEngine;

public enum DialogueParticipant
{
    Player,
    NPC,
}

[System.Serializable]
public class DialogueLine
{
    public DialogueParticipant participant;
    [TextArea] public string englishLine;
    [TextArea] public string portugueseLine;
    [TextArea] public string chineseLine;
    [TextArea] public string spanishLine;

    public string GetLine(Language language)
    {
        return language switch
        {
            Language.english => englishLine,
            Language.chinese => chineseLine,
            Language.portuguese => portugueseLine,
            Language.spanish => spanishLine,
            _ => englishLine
        };
    }
}

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/Dialogue")]
public class Dialogue : ScriptableObject
{
    public List<DialogueLine> lines = new();

    public DialogueLine GetLineAt(int index)
    {
        if (index < 0 || index >= lines.Count)
            return null;
        return lines[index];
    }

    public int LineCount => lines.Count;
}