using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fallback to prevent Lucifer's encounter from becoming stuck.
/// </summary>

public class LuciferForceTransformation : MonoBehaviour
{
    [SerializeField] BossHealth bossHealth;
    [SerializeField] LuciferTransformationManager luciferTransformationManager;

    [SerializeField] private float maxTimeIn1stForm = 300f; // Maximum time in Lucifer's first form; 300 seconds is five minutes.
    [SerializeField] private float timeIn1stForm = 0f;
    [SerializeField] private float checkInterval = 5f; // Check interval in seconds.
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Starting Lucifer Force Transformation Check");
        StartCoroutine(DoubleCheckLuciTransformation());
    }

    private IEnumerator DoubleCheckLuciTransformation()
    {
        Debug.Log("Starting Lucifer Force Transformation Check-  entrou na coroutina");
        while (true && bossHealth.gameObject.activeInHierarchy)
        {
            if (bossHealth != null && luciferTransformationManager != null)
            {
                Debug.Log("Checking Lucifer Transformation: Current Health = " + bossHealth.currentHealth + ", Time in 1st Form = " + timeIn1stForm + "s");
               
                float hpPercentage = bossHealth.currentHealth / bossHealth.maxHealth;
                if ((!luciferTransformationManager.startedTransformation && hpPercentage <= 0.15f) || timeIn1stForm >maxTimeIn1stForm)
                {
                    Debug.Log("Forcing Lucifer Transformation due to time limit or initial state.");
                    StartCoroutine(luciferTransformationManager.StartTransformation());
                }
            }
            else
            {
                Debug.LogError("BossHealth or LuciferTransformationManager is not assigned.");
            }

            timeIn1stForm += checkInterval;
            yield return new WaitForSeconds(checkInterval);

        }

    }

}
