using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoStepDamageArea : MonoBehaviour
{
    [SerializeField] protected HandleAttackOnPlayer attackPlayer;
    [SerializeField] protected List<HandleAttackOnEnemy> attackEnemyList;
    [SerializeField] protected GameObject damageArea; // Assigned in the Editor.
    [SerializeField] protected List<GameObject> safeAreas; // Assigned in the Editor.
    [SerializeField] protected int maxActivatedSafeAreas = 3;

    [Space(10)]
    public MMF_Player activatedFeedback;
    public MMF_Player explosionFeedback;

    public float timeToDeactivate = 1f;

    public Animator animator;
    public SpriteRenderer spriteRenderer;

    public void ActivateArea()
    {
        activatedFeedback.PlayFeedbacks();

        ChangeSpriteColor();

        if (safeAreas.Count > 0)
        {
            foreach (GameObject obj in safeAreas)
            {
                obj.SetActive(false);
            }

            // Copy the original list for random selection.
            List<GameObject> copia = new List<GameObject>(safeAreas); // Potentially inefficient; revisit if this becomes a performance issue.

            // Clamp the count to the number of available objects.
            int realMax = Mathf.Min(maxActivatedSafeAreas, copia.Count);

            // Choose random objects.
            for (int i = 0; i < realMax; i++)
            {
                int indice = Random.Range(0, copia.Count);
                copia[indice].SetActive(true);
                copia.RemoveAt(indice); // Remove the selected item to avoid duplicates.
            }
        }
    }
    public void TriggerDamage_AnimationEvent()
    {
        animator.SetTrigger("Boom");
        explosionFeedback.PlayFeedbacks();

        if (attackPlayer != null)
        {
            DamageAreaChecker target = attackPlayer.GetComponent<DamageAreaChecker>();
            if (target.inDamageArea && !target.inSafeArea)
            {
                attackPlayer.DamagePlayer(1);
            }
            target.Reset();
        }

        if (attackEnemyList.Count > 0)
        {
            foreach (HandleAttackOnEnemy attackEnemy in attackEnemyList)
            {
                DamageAreaChecker target = attackEnemy.GetComponent<DamageAreaChecker>();
                if (target.inDamageArea && !target.inSafeArea)
                {
                    attackEnemy.ApplyStatusEffect(1,100,100,DamageType.Fire);
                }
                target.Reset();
            }
        }

        StartCoroutine(DeactivateFx_Routine());
    }

    private IEnumerator DeactivateFx_Routine()
    {
        yield return new WaitForSeconds(timeToDeactivate);
        foreach (GameObject target in safeAreas)
        {
            target.SetActive(false);
        }


        damageArea.SetActive(false);

    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject target = collision.gameObject;
        bool isPlayer = target.tag == "Player" && collision.gameObject.name == "Player";
        if (isPlayer || target.tag == "Enemy")
        {
            try
            {
                DamageAreaChecker targetChecker = target.GetComponent<DamageAreaChecker>();
                targetChecker.inDamageArea = true;
                if(target.tag == "Player")
                {
                    attackPlayer = target.GetComponent<HandleAttackOnPlayer>();
                }
                else
                {
                    attackEnemyList.Add(target.GetComponent<HandleAttackOnEnemy>());
                }

            }
            catch (System.Exception)
            {
                // Debug.Log("Object has no DamageAreaChecker");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject target = collision.gameObject;
        bool isPlayer = target.tag == "Player" && collision.gameObject.name == "Player";
        if (isPlayer || target.tag == "Enemy")
        {
            try
            {
                DamageAreaChecker targetChecker = target.GetComponent<DamageAreaChecker>();
                targetChecker.inDamageArea = false;

                if (target.tag == "Player")
                {
                    attackPlayer = null;
                }
                else
                {
                    attackEnemyList.Remove(target.GetComponent<HandleAttackOnEnemy>());
                }

            }
            catch (System.Exception)
            {
                // Debug.Log("Object has no DamageAreaChecker");
            }
        }
    }

    /// <summary>
    /// Reset the checkers if disabled before completion.
    /// </summary>
    protected void OnDisable()
    {
        ResetCheckers();
        //disable safe areas too
        foreach (GameObject target in safeAreas)
        {
            target.SetActive(false);
        }
    }
    #region color stuff

    private void ChangeSpriteColor()
    {
        float targetAlpha = 0.7f;

        Color newColor = spriteRenderer.color; // Copy the current color.
        newColor.a = 0f; // Reset sprite alpha to zero.

        spriteRenderer.color = newColor; // Apply the updated alpha.

        damageArea.SetActive(true);
        StartCoroutine(FadeInSprite_Routine(targetAlpha, 1f));
    }

    private IEnumerator FadeInSprite_Routine(float targetAlpha, float duration)
    {
        if (spriteRenderer == null)
            yield break;

        Color color = spriteRenderer.color;
        color.a = 0f;
        spriteRenderer.color = color;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(0f, targetAlpha, elapsed / duration);
            color.a = newAlpha;
            spriteRenderer.color = color;
            yield return null;
        }
        // Set the exact final alpha.
        color.a = targetAlpha;
        spriteRenderer.color = color;
    }
    #endregion



    private void ResetCheckers()
    {
        if (attackPlayer != null)
        {
            attackPlayer.gameObject.GetComponent<DamageAreaChecker>().Reset();
        }


        try
        {
            foreach (var item in attackEnemyList)
            {
                item.gameObject.GetComponent<DamageAreaChecker>().Reset();
                attackEnemyList.Remove(item);
            }
        }
        catch (System.Exception)
        {

        }
    }
}
