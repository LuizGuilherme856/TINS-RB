using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PsicoPatoDeathExplosions : MonoBehaviour
{
    public EnemyHealth enemyHealth; // Assign EnemyHealth in the Inspector or through code.

    public GameObject gate, brokenGate;

    private void Start()
    {
        if(enemyHealth == null) Debug.LogError("EnemyHealth não está atribuído! Por favor, arraste o componente EnemyHealth no Inspector.");

        enemyHealth.OnZeroHealth += StartExplosions; // Start the explosions when health reaches zero.
    }

    public List<GameObject> bombs; // Assign bombs in the Inspector or through code.

    public void StartExplosions()
    {
        StartCoroutine(ActivateBombsProgressively());
    }

    private IEnumerator ActivateBombsProgressively()
    {
        float totalDuration = 3f;
        int bombCount = bombs.Count;
        float elapsed = 0f;



        for (int i = 0; i < bombCount; i++)
        {
            if(i == 1)// On the second bomb.
            {
                gate.SetActive(false); // Deactivate the gate before starting explosions.
                brokenGate.SetActive(true); // Activate the broken gate.
            }

            // Activate the current bomb.
            if (bombs[i] != null)
            {
                bombs[i].SetActive(true); // Fallback: activate the GameObject.
            }

            // Calculate remaining time and the dynamic interval.
            float t = (float)(i + 1) / bombCount;
            float interval = Mathf.Lerp(0.5f, 0.05f, t); // Shorten the interval as the sequence progresses.

            // Do not exceed the total duration.
            elapsed += interval;
            if (elapsed > totalDuration)
                break;

            yield return new WaitForSeconds(interval);
        }
    }
}
