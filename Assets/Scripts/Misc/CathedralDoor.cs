using MoreMountains.Feedbacks;
using UnityEngine;

public class CathedralDoor : InteractWithPlayer
{
    private RitualsCounter sacrificesCounter;
    [SerializeField] private BoxCollider2D boxCollider2d;
    [SerializeField] private MMF_Player openDoorFeedbacks;

    [SerializeField] private int sacrificesNeeded = 5;
    private bool isOpen = false;
    private Animator myAnimator;

    protected override void Awake()
    {
        base.Awake();
        sacrificesCounter = FindFirstObjectByType<RitualsCounter>();
        myAnimator= GetComponent<Animator>();
    }

    protected override void StartInteraction()
    {
        if(sacrificesNeeded ==0)
        {
            OpenDoor();
            return;
        }

        if (sacrificesNeeded - sacrificesCounter.sacrificesCompleted == 0)
            OpenDoor();
        else if (sacrificesNeeded - sacrificesCounter.sacrificesCompleted == 1)
            SetOneSacrificeNeededText();
        else
            SetNMoreSacrificesNeededText();

    }


    private void OpenDoor()
    {
        if (isOpen) return;
        myAnimator.SetTrigger("Open");
        Invoke(nameof(DisableCollider), 2.2f);
        openDoorFeedbacks.PlayFeedbacks();
        isOpen = true;
    }

    private void DisableCollider()
    {
        boxCollider2d.enabled = false;
    }

    private void SetOneSacrificeNeededText()
    {
        switch (languageData.currentLanguage)
        {
            case Language.english:
                generalTextBoxUI.SetText("1 more sacrifice needed!");
                break;
            case Language.chinese:
                generalTextBoxUI.SetText(" 需要完成 1 祭坛");
                break;
            case Language.portuguese:
                generalTextBoxUI.SetText("1 sacrifício necessário");
                break;
            case Language.spanish:
                generalTextBoxUI.SetText("1 sacrificio más necesario");
                break;
            default:
                break;
        }
    }

    private void SetNMoreSacrificesNeededText()
    {

        int numberSacrifices = sacrificesNeeded - sacrificesCounter.sacrificesCompleted;
        switch (languageData.currentLanguage)
        {
            case Language.english:
                generalTextBoxUI.SetText(numberSacrifices + " more sacrifices needed!");
                break;
            case Language.chinese:
                generalTextBoxUI.SetText("需要完成 " + numberSacrifices + " 祭坛");
                break;
            case Language.portuguese:
                generalTextBoxUI.SetText(numberSacrifices + " sacrifícios necessários");
                break;
            case Language.spanish:
                generalTextBoxUI.SetText("Se necesitan " + numberSacrifices + " sacrificios más");
                break;
            default:
                break;
        }
    }
}
