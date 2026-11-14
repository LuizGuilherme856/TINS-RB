using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpulseArea : MonoBehaviour
{
    [SerializeField]protected Transform repelObject;

    [SerializeField] protected float impulseForce = 1f;
    [SerializeField] protected float impulseIncreaseDuration = 2f; // Seconds to reach the final value.

    [SerializeField] protected bool impulseMode = false;
    [SerializeField] protected bool affectMobs = false;

    private float targetImpulseForce;

    void OnEnable()
    {
        targetImpulseForce = impulseForce;
        impulseForce = 0f;
        StartCoroutine(IncreaseImpulseForce());
    }

    /// <summary>
    /// Ramp force from zero to impulseForce.
    /// </summary>
    /// <returns></returns>
    private IEnumerator IncreaseImpulseForce()
    {
        float elapsed = 0f;
        while (elapsed < impulseIncreaseDuration)
        {
            impulseForce = Mathf.Lerp(0f, targetImpulseForce, elapsed / impulseIncreaseDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        impulseForce = targetImpulseForce;
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        bool isAndCanAffectMob = collision.CompareTag("Enemy") && affectMobs;
        if (collision.CompareTag("Player") || isAndCanAffectMob)
        {
            Transform target = collision.gameObject.transform;

            try
            {
                Impulse targetImpulse = collision.gameObject.GetComponent<Impulse>();

                Vector2 dir = target.position - repelObject.transform.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                //Apply the impulse.
                if (impulseMode) 
                {
                    targetImpulse.TriggerImpulse(angle, impulseForce, false);
                }
                else
                {
                    targetImpulse.TriggerForce(angle, impulseForce, false);
                }

            }
            catch (System.Exception)
            {
                Debug.LogError("Player ou Mob não tem impulse script");
            }
        }
    }
}
