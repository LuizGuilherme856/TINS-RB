 using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class ChaliceBehaviour : MonoBehaviour
{
    public Transform target; // Destination the chalice flies toward.
    public float waitSeconds = 1.5f; // Delay after enabling.
    public float flyDuration = 1.0f; // Flight duration.
    public float arcHeight = 2.0f; // Maximum arc height.


    public MMF_Player flyFeedback, arriveFeedback;

    void OnEnable()
    {
        StartCoroutine(WaitAndFly());
    }

    IEnumerator WaitAndFly()
    {
        yield return new WaitForSeconds(waitSeconds);
        flyFeedback.PlayFeedbacks();

        if (target != null)
        {
            yield return StartCoroutine(FlyToTarget());
            gameObject.SetActive(false);
        }
    }

    IEnumerator FlyToTarget()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = target.position;
        float elapsed = 0f;
        while (elapsed < flyDuration)
        {
            float t = elapsed / flyDuration;
            // Parabolic movement.
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            float height = Mathf.Sin(Mathf.PI * t) * arcHeight;
            currentPos.y += height;
            transform.position = currentPos;
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
        arriveFeedback.PlayFeedbacks();
    }
}
