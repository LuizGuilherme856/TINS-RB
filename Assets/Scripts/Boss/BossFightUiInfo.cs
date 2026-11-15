using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Update boss health and names in the UI.
/// </summary>
public class BossFightUiInfo : MonoBehaviour
{
    [SerializeField] private BossAmount bossAmount = BossAmount.OneBoss; // Number of bosses.
    [Header("1 boss")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI bossNameTmp;
    [SerializeField] private EnemyHealth bossHealth;

    [Header("2 bosses")]
    [SerializeField] private List<Slider> healthSlider_2Bosses;
    [SerializeField] private List<TextMeshProUGUI> bossNameTmp_2Bosses;
    [SerializeField] private List<EnemyHealth> bossHealths_2Bosses;

    [Header("3 bosses")]
    [SerializeField] private List<Slider> healthSlider_3Bosses;
    [SerializeField] private List<TextMeshProUGUI> bossNameTmp_3Bosses;
    [SerializeField] private List<EnemyHealth> bossHealths_3Bosses;
    [Space(20)]

    [Tooltip("Enemy data que vai ser mostrado na UI com o nome e descri��o do boss")]
    [SerializeField] private EnemyData uiEnemyData;
    [Tooltip("Só é utilizado quando tem dois bosses na mesma fase. Tipo o gael no inferno")]
    [SerializeField] private EnemyData uiEnemyData2;
    private int bossesFaced = 0; // Tracks bosses already encountered so uiEnemyData can advance in fights with multiple bosses.

    [SerializeField] private bool hidePoints = false;
    [SerializeField] private GameObject pointsGameObj, waveGameObj;

    [SerializeField] private LanguageData languageData;

    [Header("Show off frame")]
    [SerializeField] private GameObject bossFrame;
    [SerializeField] private Animator frameAnimator;
    [SerializeField] private TextMeshProUGUI bossNameOnFrame;
    [SerializeField] private TextMeshProUGUI bossDescriptionOnFrame;
    [SerializeField] private LanguageData LanguageData;

    private void Awake()
    {
        switch (bossAmount)
        {
            case BossAmount.OneBoss:
                if (bossHealth == null) bossHealth = FindFirstObjectByType<BossHealth>();
                if (bossHealth != null) uiEnemyData = bossHealth.enemyData;
                break;
            case BossAmount.None:
                break;
            default:
                if(uiEnemyData == null)
                {
                    Debug.LogError("uiEnemyData esta nulo. " +
                        "Lembre-se que a enemy data tem que ser colocada manualmente quando tem mais de um boss");
                }
                break;
        }
    }


    private void UpdateHealth()
    {
        switch (bossAmount)
        {
            case BossAmount.OneBoss:
                healthSlider.value = bossHealth.currentHealth;
                bossNameTmp.text = bossHealth.enemyData.GetName(languageData.currentLanguage);

                break;

            case BossAmount.TwoBosses:
                for (int i = 0; i < bossHealths_2Bosses.Count; i++)
                {
                    healthSlider_2Bosses[i].value = bossHealths_2Bosses[i].currentHealth;
                    bossNameTmp_2Bosses[i].text = bossHealths_2Bosses[i].enemyData.GetName(languageData.currentLanguage);
                }
                break;

            case BossAmount.ThreeBosses:
                for (int i = 0; i < bossHealths_3Bosses.Count; i++)
                {
                    healthSlider_3Bosses[i].value = bossHealths_3Bosses[i].currentHealth;

                    bossNameTmp_3Bosses[i].text = bossHealths_3Bosses[i].enemyData.GetName(languageData.currentLanguage);

                }
                break;
        }
    }

    public IEnumerator ShowBossFrame(float delayToStart, float delayToFinish)
    {
        //Restore scale to 1 if it was left at zero.
        GetComponent<RectTransform>().localScale = Vector3.one;

        if (bossHealth == null && bossAmount == BossAmount.OneBoss)
        {
            bossHealth = FindFirstObjectByType<BossHealth>();
            Debug.LogError("BossHealth esta nulo. Lembre-se que o a vida do boss tem que ser colocada manualmente pelo inspector" +
                " Se esta usando mais de um boss, coloque um enemy data");
        }

        if(bossesFaced == 0)
        {
            bossNameOnFrame.text = uiEnemyData.GetName(languageData.currentLanguage);
            bossDescriptionOnFrame.text = uiEnemyData.GetDescription(languageData.currentLanguage);
            
        }
        else
        {
            bossNameOnFrame.text = uiEnemyData2.GetName(languageData.currentLanguage);
            bossDescriptionOnFrame.text = uiEnemyData2.GetDescription(languageData.currentLanguage);
        }

        yield return new WaitForSeconds(delayToStart);
        frameAnimator.SetTrigger("Start");
        //bossFrame.SetActive(true);

        yield return new WaitForSeconds(delayToFinish);
        frameAnimator.SetTrigger("Finish");
        //bossFrame.SetActive(false);
        
        bossesFaced++;

    }

    public void UpdateUiValues()
    {
        if (bossAmount == BossAmount.OneBoss && bossHealth == null) bossHealth = FindFirstObjectByType<BossHealth>();
        if (bossAmount == BossAmount.OneBoss && bossHealth == null) return;

        switch (bossAmount)
        {
            case BossAmount.OneBoss:
                bossNameTmp.gameObject.SetActive(true);
                bossNameTmp.text = bossHealth.enemyData.GetName(LanguageData.currentLanguage);
                healthSlider.maxValue = bossHealth.maxHealth;
                healthSlider.value = bossHealth.maxHealth;
                break;

            case BossAmount.TwoBosses:
                for (int i = 0; i < bossHealths_2Bosses.Count; i++)
                {
                    bossNameTmp_2Bosses[i].gameObject.SetActive(true);
                    bossNameTmp_2Bosses[i].text = bossHealths_2Bosses[i].enemyData.GetName(LanguageData.currentLanguage);
                    healthSlider_2Bosses[i].maxValue = bossHealths_2Bosses[i].maxHealth;
                    healthSlider_2Bosses[i].value = bossHealths_2Bosses[i].maxHealth;
                }
                break;

            case BossAmount.ThreeBosses:
                for (int i = 0; i < bossHealths_3Bosses.Count; i++)
                {
                    bossNameTmp_3Bosses[i].gameObject.SetActive(true);
                    bossNameTmp_3Bosses[i].text = bossHealths_3Bosses[i].enemyData.GetName(LanguageData.currentLanguage);
                    healthSlider_3Bosses[i].maxValue = bossHealths_3Bosses[i].maxHealth;
                    healthSlider_3Bosses[i].value = bossHealths_3Bosses[i].maxHealth;
                }
                break;
        }
    }

    public IEnumerator ShowBossFightUI(float delay)
    {
        yield return new WaitForSeconds(delay);


        if (bossAmount == BossAmount.OneBoss && bossHealth == null) bossHealth = FindFirstObjectByType<BossHealth>();
        if (bossAmount == BossAmount.OneBoss && bossHealth == null)
            yield break;

        switch (bossAmount)
        {
            case BossAmount.OneBoss:
                bossHealth.OnHealthChange += UpdateHealth;
                healthSlider.gameObject.SetActive(true);
                bossNameTmp.gameObject.SetActive(true);
                break;

            case BossAmount.TwoBosses:
                for (int i = 0; i < bossHealths_2Bosses.Count; i++)
                {
                    bossHealths_2Bosses[i].OnHealthChange += UpdateHealth;
                    healthSlider_2Bosses[i].gameObject.SetActive(true);
                    bossNameTmp_2Bosses[i].gameObject.SetActive(true);
                }
                break;

            case BossAmount.ThreeBosses:
                for (int i = 0; i < bossHealths_3Bosses.Count; i++)
                {
                    bossHealths_3Bosses[i].OnHealthChange += UpdateHealth;
                    healthSlider_3Bosses[i].gameObject.SetActive(true);
                    bossNameTmp_3Bosses[i].gameObject.SetActive(true);
                }
                break;
        }

        UpdateUiValues();
        
        if (hidePoints)
        {
            pointsGameObj.SetActive(false);
        }

        waveGameObj.SetActive(false);
    }

    public void HideBossUI()
    {
        switch (bossAmount)
        {
            case BossAmount.OneBoss:
                healthSlider.gameObject.SetActive(false);
                bossNameTmp.gameObject.SetActive(false);
                break;

            case BossAmount.TwoBosses:
                foreach (var slider in healthSlider_2Bosses)
                {
                    slider.gameObject.SetActive(false);
                }
                foreach (var nameTmp in bossNameTmp_2Bosses)
                {
                    nameTmp.gameObject.SetActive(false);
                }
                break;

            case BossAmount.ThreeBosses:
                foreach (var slider in healthSlider_3Bosses)
                {
                    slider.gameObject.SetActive(false);
                }
                foreach (var nameTmp in bossNameTmp_3Bosses)
                {
                    nameTmp.gameObject.SetActive(false);
                }
                break;
        }
    }

    public IEnumerator ResetUI(float delay)
    {
        yield return new WaitForSeconds(delay);
        waveGameObj.SetActive(true);
        pointsGameObj.SetActive(true);
        RefreshPointsUi_TechBooh();
        HideBossUI();
    }

    private void RefreshPointsUi_TechBooh()
    {
        if (pointsGameObj == null)
        {
            return;
        }

        foreach (var pointsUi in pointsGameObj.GetComponentsInParent<PointsUI>(true))
        {
            pointsUi.RefreshPointsImmediate();
        }

        foreach (var pointsUi in pointsGameObj.GetComponentsInChildren<PointsUI>(true))
        {
            pointsUi.RefreshPointsImmediate();
        }
    }

    public void SetBossHealth(EnemyHealth enemyHealth)
    {
        bossHealth.OnHealthChange -= UpdateHealth;
        this.bossHealth = enemyHealth;
        UpdateUiValues();
        bossHealth.OnHealthChange += UpdateHealth;
    }

    public enum BossAmount
    {
        OneBoss,
        TwoBosses,
        ThreeBosses,
        None
    }
}
