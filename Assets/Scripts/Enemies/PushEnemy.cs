using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushEnemy : MonoBehaviour
{

    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private float impulseRadius = 1f, impulseStrength =  10f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// In crowds, faster enemies or enemies with special abilities can
    /// get stuck behind slower ones. Gently push nearby enemies
    /// so the elite enemy can pass.
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        Vector3 epicenter = transform.position;
        Collider2D[] targetsHit = Physics2D.OverlapCircleAll(transform.position, impulseRadius, targetLayer);
        //Collider2D[] targetsHit = Physics2D.OverlapCapsuleAll(transform.position, impulseRadius, targetLayer);
        foreach (Collider2D target in targetsHit)
        {

            float targetDistanceFromEpicenter = Mathf.Abs(Vector2.Distance(epicenter, target.transform.position));
            var impulseDirection = target.transform.position - epicenter;

            //Reduce force with distance from the explosion center.
            float forceMultiplier = Mathf.Abs((impulseRadius - targetDistanceFromEpicenter) / impulseRadius);
            //Also verify the tag as a defensive check beyond the enemy layer filter.
            if (target.CompareTag("Enemy"))
            {
                try
                {
                    target.GetComponent<Rigidbody2D>().AddForce(impulseDirection * impulseStrength * forceMultiplier, ForceMode2D.Impulse);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Erro ao aplicar empurro no inimigo.\nException: " + e);
                }
            }
        }
    }

    private void OnDrawGizmosSelected() //Visualize the push hitbox.
    {
        Gizmos.DrawWireSphere(transform.position, impulseRadius);
    }
}
