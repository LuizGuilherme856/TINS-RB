using UnityEngine;

/// <summary>
/// Starts locked; obtaining the key unlocks it for opening.
/// KeyPickUp is responsible for unlocking the door.
/// </summary>

public class LockedDoor : ClosedDoor
{

    [Space(15)]
    [Header("Locked door message")]
    public string englishText = "A Key is Required";
    public string chineseText = "需要钥匙", 
        portugueseText = "Uma Chave é necessária", 
        spanishText = "Se requiere llave";

    protected override void StartInteraction()
    {
        if (isOpen) return; //its already open so you cant open it again
                            //generalTextBoxUI.SetText("Press " + buttons.interaction + " To "+openText+" for " + GetPrice());

        base.StartInteraction();
        if (generalTextBoxUI == null) return;

        if(canOpen == false)
        {
            Language currentLanguage = languageData != null ? languageData.currentLanguage : Language.english;
            switch (currentLanguage)
            {
                case Language.english:
                    generalTextBoxUI.SetText(englishText);
                    break;
                case Language.chinese:
                    generalTextBoxUI.SetText(chineseText);
                    break;
                case Language.portuguese:
                    generalTextBoxUI.SetText(portugueseText);
                    break;
                case Language.spanish:
                    generalTextBoxUI.SetText(spanishText);
                    break;
                default:
                    break;
            }

        }




    }




   
}
