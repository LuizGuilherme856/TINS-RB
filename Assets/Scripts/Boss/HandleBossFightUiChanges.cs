//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

///// <summary>
///// Updates the UI when a boss fight starts.
///// </summary>
//public class HandleBossFightUiChanges : MonoBehaviour
//{
//    [SerializeField] private EnemyHealth bossHealth;

//    [SerializeField] private bool hidePoints = false;
//    [SerializeField] private GameObject pointsGameObj, waveGameObj;

//    [SerializeField] private BossFightUiInfo bossHealthUI;


//    void Start()
//    {
//        bossHealth.OnHealthChange += UpdateHealth;
//    }


//    private void UpdateHealth()
//    {
//        bossHealthUI.UpdateHealth(bossHealth.currentHealth);
//    }

//    public void ShowBossFrame(float delay, float duration)
//    {
//        StartCoroutine(bossHealthUI.ShowBossFrame(delay, duration));
//    }

//    public IEnumerator SetBossFightUI(float delay)
//    {

//        yield return new WaitForSeconds(delay);
//        if (hidePoints)
//        {
//            pointsGameObj.SetActive(false);
//        }

//        waveGameObj.SetActive(false);
//        bossHealthUI.ShowBossUI(bossHealth.maxHealth, bossHealth.enemyData.EnemyName);
//    }

//    public IEnumerator ResetUI(float delay)
//    {
//        yield return new WaitForSeconds(delay);
//        waveGameObj.SetActive(true);
//        pointsGameObj.SetActive(true);
//        bossHealthUI.HideBossUI();
//    }
//}
