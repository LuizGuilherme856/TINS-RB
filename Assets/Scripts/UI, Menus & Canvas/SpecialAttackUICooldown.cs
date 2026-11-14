using System.Collections;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Display time remaining until the next attack.
/// </summary>
public class SpecialAttackUICooldown : MonoBehaviour
{
    [SerializeField] private PlayerHolder playerHolder;
     private MeleeManager meleeManager;

    /// <summary>
    /// Uses three icons: a colored foreground and monochrome background
    /// show readiness, while an additional loading icon
    /// fills behind them when multiple charges are available.
    /// </summary>
    [SerializeField] private Image icon, iconBackground, loadingIcon;

    [SerializeField] private MMF_Player isFullFeedbacks;
    [SerializeField] private TextMeshProUGUI quantityText;


    [Tooltip("Taxa de atualizacao. de quanto em quanto tempo vai atualizar os valores da UI. Em Segundos")]
    [SerializeField] [Range(0.001f, 2)] private float updateRate = 0.1f;

    [SerializeField] private GlobalStats globalStats;
    
    private float fillAmount;
    private bool canPlayFeedback = false;
    private int chargesLastTime; // Number of charges at the previous update.

    private void Awake()
    {
        meleeManager = playerHolder.GetSpecialMeleeManager();
    }

    private void Start()
    {
        Invoke(nameof(SetCanPlay), 3); //Avoid playing feedback when the run starts.

        meleeManager.OnChangeMeleeWeapon += UpdateIcon;
        UpdateIcon();

        StartCoroutine(UpdateUI_Rotine());


    }


    private void PlayIsFullFeedback()
    {
        if (canPlayFeedback)
        {
            icon.fillAmount = 1;
            isFullFeedbacks.PlayFeedbacks();
            //Debug.Log("full feedback");
        }
    }

    private IEnumerator UpdateUI_Rotine()
    {
        while (true)
        {
            UpdateUI();

            yield return new WaitForSeconds(updateRate);
        }
    }

    private void UpdateUI()
    {

        if (meleeManager.GetMeleeData() == null) return;

        int currentCharges = meleeManager.GetCurrentActionQuantity();
        //Already full; no update needed.
        if (currentCharges == globalStats.maxSpecialActionQuantity)
        {
            icon.fillAmount = 1;
            loadingIcon.enabled = false;

            if (currentCharges > 1)
            {
                quantityText.text = currentCharges.ToString();
            }
            else
            {
                quantityText.text = string.Empty;
            }

        }
        else //Not full; refresh progress.
        {
            fillAmount = Mathf.Abs(meleeManager.GetTotalActionCooldown_Percentage(1));
            icon.fillAmount = fillAmount;

            if (globalStats.maxSpecialActionQuantity == 1) //Single charge.
            {
                loadingIcon.enabled = false;
            }
            else if (globalStats.maxSpecialActionQuantity > 1)  //Multiple charges.
            {
                loadingIcon.enabled = true;

                quantityText.text = currentCharges.ToString();

                loadingIcon.fillAmount = meleeManager.CalculateNextActionPercentageCompleted();

            }

        }

        if (chargesLastTime < currentCharges)
        {
            PlayIsFullFeedback();
        }

        chargesLastTime = currentCharges;

        //double check
        fillAmount = Mathf.Abs(meleeManager.GetTotalActionCooldown_Percentage(1));
        float marginOfError = 0.1f;
        if (fillAmount > icon.fillAmount + marginOfError )
        {
            Debug.LogWarning("O valor do fill amount ta maior do que o que ta na UI. Isso pode ser um bug");
            icon.fillAmount = fillAmount;
        }

    }

    public void SetIcon(Sprite newIcon)
    {
        icon.sprite= newIcon;
        iconBackground.sprite= newIcon;
    }

    private void UpdateIcon()
    {
        if (meleeManager.GetMeleeData() == null) return;

        var newIcon = meleeManager.GetMeleeData().buyIcon;
        icon.sprite = newIcon;
        iconBackground.sprite = newIcon;
    }


    public void SetCanPlay()
    {
        canPlayFeedback = true;
    }
}
