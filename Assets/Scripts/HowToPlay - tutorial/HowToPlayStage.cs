using System;
using UnityEngine;

//[CreateAssetMenu(fileName = "HowToPlay_Stage_", menuName = "stage_")]
public class HowToPlayStage : MonoBehaviour
{
    //Stores the current tutorial-stage information.
    public Transform npcPos;

    [Space(10)]
    [Header("-- ENGLISH --")]
    public string[] englishLines; //NPC dialogue lines.
    //public string englishObjective; //Objective displayed at the top of the screen.

    [Space(10)]
    [Header("-- Chinese --")]
    public string[] chineseLines; 
    //public string chineseObjective;

    [Space(10)]
    [Header("-- portuguese --")]
    public string[] portugueseLines;
    //public string portugueseObjective;

    [Space(10)]
    [Header("-- Spanish --")]
    public string[] spanishLines;
    //public string spanishObjective;

    [Space(10)]
    [SerializeField] private bool changeObjShader = false;
    [SerializeField] SpriteRenderer objSpriteRenderer;
    public bool changeLineAutomaticallyWhenComplete = true; //Conversation-only stages already advance dialogue on completion; avoid advancing again when the next stage starts.

    public bool isConversation = false;
    protected bool isComplete = false;
    public virtual event Action OnComplete;
    //private Material spriteMaterial;

    private void Awake()
    {
        //if (changeObjShader) spriteMaterial = objSpriteRenderer.material;
    }

    public virtual void StartStage()
    {
        if(changeObjShader)
        {
            objSpriteRenderer.enabled = true;
        }
    }

    public virtual void DialogueIsFinished()
    {

    }

    protected void SetCompleted()
    {
        if (changeObjShader)
        {
            objSpriteRenderer.enabled = false;
        }
        isComplete = true;
        OnComplete?.Invoke();
    }

    public string GetLine(int index, Language language)
    {
        switch (language)
        {
            case Language.english:
                return englishLines[index];
            case Language.chinese:
                return chineseLines[index];
            case Language.portuguese:
                return portugueseLines[index];
            
            case Language.spanish:
                return spanishLines[index];
            default:
                return englishLines[index];
        }
    }

    public string[] GetLines(Language language)
    {
        switch (language)
        {
            case Language.english:
                return englishLines;
            case Language.chinese:
                return chineseLines;
            case Language.portuguese:
                return portugueseLines;

            case Language.spanish:
                return spanishLines;
            default:
                return englishLines;
        }
    }
    /*
    public string GetObjective(Language language)
    {
        switch (language)
        {
            case Language.english:
                return englishObjective;
            case Language.chinese:
                return chineseObjective;
            case Language.portuguese:
                return portugueseObjective;

            case Language.spanish:
                return englishObjective;
            default:
                return englishObjective;
        }
    }
    */
}
