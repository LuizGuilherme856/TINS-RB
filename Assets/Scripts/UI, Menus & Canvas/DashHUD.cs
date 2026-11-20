using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DashHUD : MonoBehaviour
{
    //Data Related
    [SerializeField]private PlayerHolder playerHolder;
    [SerializeField] private MeleeManager dashManager;
    private UpgradeManager upgradeManager;

    //UI Related
    public Slider dashCooldown;
    [SerializeField] private TextMeshProUGUI moreThanMaxDashIndicator;
    [SerializeField] private MMF_Player moreThanMaxDashFeel;

    [SerializeField] private float coolDownTimer;
    [SerializeField] private int maxUiActionCap = 1;
    [SerializeField] private GameObject[] cases;

    

    private int oldExtraDash = 0;
    void Awake()
    {
        dashManager = playerHolder.GetDashManager();
        //Try to resolve UpgradeManager.
        try
        {
            upgradeManager = FindFirstObjectByType<UpgradeManager>();
            upgradeManager.OnChangeStats += UpdateDash;
        }
        catch (System.Exception)
        {
            Debug.Log("N�o encontrou o upgradeManager nessa scene");
        }


    }

    private void FixedUpdate()
    {
        // Poll in FixedUpdate until a suitable event is available.
        UpdateDash();
        dashCooldown.value = dashManager.GetTotalActionCooldown_Percentage(maxUiActionCap);
    }

    private void LateUpdate()
    {
        int extraDash = dashManager.GetCurrentActionQuantity() - maxUiActionCap;


        if (extraDash > 0)
        {
            moreThanMaxDashIndicator.gameObject.SetActive(true);
            if (oldExtraDash < extraDash)
            {
                moreThanMaxDashFeel.PlayFeedbacks();
            }
            
            moreThanMaxDashIndicator.text = "+ " + extraDash.ToString();
        }
        else
        {
            moreThanMaxDashIndicator.gameObject.SetActive(false);
            moreThanMaxDashIndicator.text = "";
        }
        oldExtraDash = extraDash;
    }

    //TODO: Subscribe to a dash-count change event instead.
    public void UpdateDash()
    {
        switch (dashManager.MaxDashActions)
        {
            case 0:
                foreach (GameObject gb in cases)
                {
                    gb.SetActive(false);
                }
                Debug.LogError("Jogador esta com Max de Dash = 0");
                break;
            case 1:
                foreach (GameObject gb in cases)
                {
                    gb.SetActive(false);
                }
                break;
            case 2:
                for (int i = 0; i < 3; i++)
                {
                    if (i == 0)
                    {
                        cases[i].gameObject.SetActive(true);
                    }
                    else 
                    {
                        cases[i].gameObject.SetActive(false);
                    }
                }
                break;
            case 3:
                for (int i = 0; i < 3; i++)
                {
                    if (i == 1)
                    {
                        cases[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        cases[i].gameObject.SetActive(false);
                    }
                }
                break;
            default:
                for (int i = 0; i < 3; i++)
                {
                    if (i == 2)
                    {
                        cases[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        cases[i].gameObject.SetActive(false);
                    }
                }
                break;
        }
    }

}
