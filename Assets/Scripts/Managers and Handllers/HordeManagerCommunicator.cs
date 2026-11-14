//Notify HordeManager when an enemy spawns or dies.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HordeManagerCommunicator : MonoBehaviour
{
    [SerializeField] private bool isWaveEnemy = true; //Bosses are excluded; Gromsch is treated as a mini-boss here.

    private EnemyHealth enemyHealth;
    [Tooltip("Não é necessário colocar manualmente, mas as vezes essa bagaça buga e nao acha o waveManager")]
    [SerializeField] private EnemyWaveManager hordeManager;
    private SurvivalModeManager survivalModeEnemySpawner;
    //public int currentWave { get; private set; }
    //Cache these values to avoid another HordeManager lookup.
    [Tooltip("True = toda a vez que enable nesse obj +1 enemy e -1 quando ele morrer no rundata , falso = conta somente -1 quando ele morrer")]
    [SerializeField] private bool updateRunData = true;// Bool that checks if we need to update the rundata, used in jumping enemys
    private float healthIncreaseCoef;

    private RunData runData;

    private void Awake()
    {
        if (TINS.Campaign.CampaignCombatRules.IsCampaignMission) return;
        runData = GetComponent<EnemyHealth>().GetRunData();

        if(runData == null )
        {
            Debug.Log("Nao encontrou a runData. coloque a rundata no Enemy health de: " + gameObject.name);
        }

        if(runData.survivalMode == false && hordeManager == null)
        {
            hordeManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<EnemyWaveManager>();
            if(hordeManager == null) hordeManager = FindFirstObjectByType<EnemyWaveManager>();

        }
        else
        {
            survivalModeEnemySpawner = GameObject.FindGameObjectWithTag("Manager").GetComponent<SurvivalModeManager>();
            if(survivalModeEnemySpawner == null) survivalModeEnemySpawner = FindFirstObjectByType<SurvivalModeManager>();
        }


        enemyHealth = GetComponent<EnemyHealth>();

        if (runData.survivalMode == false)
            if (isWaveEnemy) enemyHealth.OnZeroHealth += () => hordeManager.EnemyDied();  // Notify enemy death.
                                                                                          // else enemyHealth.OnZeroHealth

        healthIncreaseCoef = hordeManager.GetHealthIncreaseCoef();

    }

    private void OnEnable()
    {
        if (TINS.Campaign.CampaignCombatRules.IsCampaignMission) return;
        if (runData.survivalMode == false)
        {
            if (updateRunData)
            {
                if (isWaveEnemy) hordeManager.EnemySpawned();
            }

            healthIncreaseCoef = hordeManager.GetHealthIncreaseCoef();
            //currentWave = hordeManager.GetCurrentWave();

        }
        
    }

    public float GetHealthIncreaseCoefficient()
    {
        if (healthIncreaseCoef <= 0)
        {
            healthIncreaseCoef = hordeManager.GetHealthIncreaseCoef();

        }
        if (healthIncreaseCoef <= 0 )
        {
            Debug.LogError("Coeficiente de aumento de vida nao deveria ser zero!!!. Retornando 0,2f");
            return 0.2f;

        }
        return healthIncreaseCoef;
    }

    public int GetCurrentWave()
    {

        if(hordeManager== null)
        {
            //Retry because waveManager may not have been resolved on the first attempt.
            hordeManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<EnemyWaveManager>();
        }

        try
        {
         return hordeManager.GetCurrentWave();

        }
        catch (System.Exception e)
        {
            Debug.LogError("Nao conseguiu pegar a wave atual. Talvez não tenha conseguido referencia ao hordeManager. " +
                "Retornando 1 pra onda atual" +
                "\nExceção: " + e.Message);
            return 1;
        }
    }

    public float GetCurrentTimeElapsed()
    {
        return runData.timeElapsed;
    }

    public void NotifyDeath()
    {
        if (TINS.Campaign.CampaignCombatRules.IsCampaignMission) return;
        hordeManager.EnemyDied();
    }

    public void SetUpdateRunData(bool targetBool)
    {
        updateRunData = targetBool;
    }
}
