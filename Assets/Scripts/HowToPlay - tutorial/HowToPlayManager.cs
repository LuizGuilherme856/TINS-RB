using MoreMountains.Feedbacks;
using System;
using UnityEngine;

//Used by both tutorials and generic interactions.

public class HowToPlayManager : MonoBehaviour
{
    [SerializeField] private HowToPlayStage[] howToPlayStages;
    [SerializeField] private TalkToNpc dialogueSystem;
    [SerializeField] private LanguageData languageData;
    [SerializeField] private Transform duckTransform;
    [SerializeField] private EnemyAI duckAI;


    [Space(10)]
    [Header("OTHERS")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private float skipLineAutomaticallyTime = 10f;
    [field: SerializeField] public int currentStage { get; private set; }
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private int sceneToLoadIndex = 0;
    [SerializeField] private PlayFeedbacks transitionScreenFeedback;
    [SerializeField] private HandleStartNewRun handleStartNewRun;
    [SerializeField] private bool isTutorial = true;
    private bool isConversation = false; //Advance dialogue automatically for conversation-only stages.
    private float timer;
    private bool finishedTutorial = false;
    public event Action OnNewStage;
    
    private void Awake()
    {
        dialogueSystem.SetLines(howToPlayStages[currentStage].GetLines(languageData.currentLanguage));
        howToPlayStages[currentStage].OnComplete += GoToNextStage;
    }

    private void Start()
    {
        dialogueSystem.OnFinishDialogue += () => howToPlayStages[currentStage].DialogueIsFinished();
        dialogueSystem.OnChangeLine += () => timer = skipLineAutomaticallyTime;
    }

    private void FixedUpdate()
    {
        if (finishedTutorial) return;
        if (isConversation)
        {
            timer -= Time.fixedDeltaTime;
            if(timer <=0)
            {
                dialogueSystem.NextLine();
                timer = skipLineAutomaticallyTime;
            }
        }
    }

    public void GoToNextStage()
    {
        if (finishedTutorial) return;

        howToPlayStages[currentStage].OnComplete -= GoToNextStage;
        
        currentStage++;
        OnNewStage?.Invoke();

        if (currentStage >= howToPlayStages.Length)
        {
            currentStage--; //Decrement because the next entry is accessed afterward.
            if(isTutorial == false)//Shared with generic interactions.
            {
                finishedTutorial = true;
                return;
            }

            transitionScreenFeedback.PlayAllFeedbacks();

            // TechBooh: tutorial points, kills, and time do not represent
            // a gameplay attempt and must never reach the leaderboard.
            handleStartNewRun.ResetAfterTutorial_TechBooh();
            playerData.isTutorialFinished = true;
            sceneLoader.LoadSpecificScene(sceneToLoadIndex);
            Debug.Log("Chegou no fim dos estagios de tutorial");
            finishedTutorial = true;
            return;
        }

        howToPlayStages[currentStage].OnComplete += GoToNextStage;
        howToPlayStages[currentStage].StartStage();
        duckAI.SetTarget(howToPlayStages[currentStage].npcPos);

        dialogueSystem.SetLines(howToPlayStages[currentStage].GetLines(languageData.currentLanguage));
        
        if (howToPlayStages[currentStage - 1].changeLineAutomaticallyWhenComplete) //Whether entering the next stage advances dialogue.
            dialogueSystem. NextLine();

        isConversation = howToPlayStages[currentStage].isConversation;
        timer = skipLineAutomaticallyTime;
    }

}
