using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBait : MonoBehaviour
{

    [SerializeField] private float duration = 10f;
    [SerializeField] private MMF_Player spawnFeedbacks, destroyFeedbacks;
    private CircleCollider2D myCollider;

    private List<ChangeTarget> enemiesInRange = new List<ChangeTarget>();

    private void Awake()
    {
        myCollider= GetComponent<CircleCollider2D>();
    }
    private void OnEnable()
    {
        spawnFeedbacks.PlayFeedbacks();
        Invoke(nameof(DestroyBait), duration);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {

            try
            {
                var changeTarget = collision.gameObject.GetComponent<ChangeTarget>();
               // if (changeTarget.IsTargetingPlayer()) 
               // {
                    changeTarget.SetNewTarget(transform);
                    enemiesInRange.Add(collision.gameObject.GetComponent<ChangeTarget>());

               // }
            }
            catch (System.Exception)
            {
                Debug.Log("Nao tem change target no inimigo");
                //throw;
            }

        }
    }

    private void DestroyBait()
    {
        foreach (ChangeTarget enemy in enemiesInRange)
        {
            enemy.SetPlayerAsTarget();
        }

        destroyFeedbacks.PlayFeedbacks();
        myCollider.enabled= false; //Deactivate this because the feedback only destroys the sprite.
       
        Invoke(nameof(DestroyNow), 6f);
        
    }

    private void DestroyNow()
    {
        Destroy(gameObject);
    }


}
