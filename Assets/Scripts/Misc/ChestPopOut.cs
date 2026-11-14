using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestPopOut : MonoBehaviour
{
    public List<GameObject> objectsToLaunch;
    public float delayBetweenObjects = 0.1f;
    public float moveDuration = 0.3f;
    public float spread = 2f;
    public float upwardDistance = 1.5f;

    void OnEnable()
    {
        StartCoroutine(LaunchSequence());
        
    }


    IEnumerator LaunchSequence()
    {
        foreach (GameObject obj in objectsToLaunch)
        {
            if (obj != null)
            {
                obj.SetActive(true);

                Vector3 startPos = transform.position;
                Vector3 randomOffset = new Vector3(
                    Random.Range(-spread, spread),
                    Random.Range(0.5f, upwardDistance),
                    0f
                );

                Vector3 targetPos = startPos + randomOffset;

                StartCoroutine(MoveToPosition(obj.transform, targetPos, moveDuration));
            }

            yield return new WaitForSeconds(delayBetweenObjects);
        }
    }

    IEnumerator MoveToPosition(Transform obj, Vector3 target, float duration)
    {
        Vector3 initialPos = obj.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            obj.position = Vector3.Lerp(initialPos, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.position = target;
    }
}
