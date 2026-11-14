using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Play feedback every configured number of waves, usually five.
/// TODO: Merge with the regular wave-feedback script.
/// </summary>
public class FeedbackOnWaveChange : MonoBehaviour
{
    [SerializeField] private MMF_Player waveStartedFeedback;

    [Header("       SPECIAL WAVE")]
    [SerializeField] private int specialWave = 5;
    [SerializeField] private MMF_Player specialWaveStartedFeedback;


    [Header("       Finished WAVE")]
    [SerializeField] private MMF_Player waveFinishedFeedback;
    [SerializeField] private MMF_Player levelLoadFeedback;

    [Space(20)]
    [SerializeField] private TextMeshProUGUI waveText;

    [SerializeField] private string englishText, chineseText, portugueseText, spanishText;
    
    [SerializeField] private LanguageData languageData;
    [SerializeField] private EnemyWaveManager hordeManager;
    [SerializeField] private MusicManager musicManager;

    [SerializeField] private Animator animator;

    bool showedLevelName = false;



    private void Awake()
    {
        hordeManager = GetComponent<EnemyWaveManager>();

    }

    private void OnEnable()
    {
        showedLevelName = false;
        hordeManager.OnStartNewWave += FinishWaveFeedback;
        hordeManager.OnEnemyCanStartSpawning += StartSpawningEnemiesFeedback;

    }

    /// <summary>
    /// Play the wave-end sound and animate the wave text.
    /// Show a countdown; when it ends, play the wave-start sound and update the wave number.
    /// Zoom the text back to its original position.
    /// </summary>
    private void FinishWaveFeedback()
    {


        musicManager.FadeOutBossMusicVolume(2.5f);
        musicManager.FadeOutMusicVolume(2.5f);
        

        if (showedLevelName)
        {
            animator.SetTrigger("End");
            waveFinishedFeedback.PlayFeedbacks();

        }
        else
        {
            showedLevelName = true;
            levelLoadFeedback.PlayFeedbacks();
        }
            //var wave = hordeManager.GetCurrentWave();

        //switch (languageData.currentLanguage)
        //{
        //    case Language.english:
        //        waveText.text = englishText + " " + wave;
        //        break;
        //    case Language.chinese:
        //        waveText.text = chineseText + " " + wave;
        //        break;
        //    case Language.portuguese:
        //        waveText.text = portugueseText + " " + wave;
        //        break;
        //    case Language.spanish:
        //        waveText.text = spanishText + " " + wave;
        //        break;
        //    default:
        //        waveText.text = englishText + " " + wave;
        //        break;
        //}


    }


    private void StartSpawningEnemiesFeedback()
    {

        waveText.gameObject.GetComponent<HandleWaveTextMaterialChange>().UpdateWaveNumber_Animation();

        waveStartedFeedback.PlayFeedbacks();
        animator.SetTrigger("Start");

        if (hordeManager.GetCurrentWave() % specialWave == 0)
        {
            specialWaveStartedFeedback.PlayFeedbacks();
            musicManager.FadeInBossMusicVolume(10f);


        }
        else
        {
            musicManager.FadeInMusicVolume(10f);
        }
    }

}
