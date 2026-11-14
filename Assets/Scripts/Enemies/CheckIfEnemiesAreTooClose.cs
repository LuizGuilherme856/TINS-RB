
using UnityEngine;

/// <summary>
/// Enemies very close to the player can miss bullet damage because projectiles spawn behind them.
/// Raycast between the player and enemy to detect an intervening target.
/// </summary>
public class CheckIfEnemiesAreTooClose : MonoBehaviour
{
    [SerializeField] private Transform originPos;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float circleRadius = 1f;
    //[SerializeField] private LineRenderer lineRenderer;

    /*
    public bool IsEnemyTooCloseRaycast() //Known issue: not working as intended.
    {
        //Test the player's position first.
        var distance = Vector3.Magnitude(transform.position - firePoint.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, firePoint.position, distance, enemyLayer);

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, firePoint.position);
        
        //Cast additional rays as a fallback.
        for (int i = 0; i < originPos.Length; i++)
        {
            if(hit.collider != null) break;

            distance = Vector3.Magnitude(originPos[i].position - firePoint.position);
            hit = Physics2D.Raycast(originPos[i].position, firePoint.position, distance, enemyLayer);

            lineRenderer.SetPosition(0, originPos[i].position);
            lineRenderer.SetPosition(1, firePoint.position);

        }

        

        if(hit.collider == null) return false;

        Debug.Log("Hit an "+hit.collider.gameObject.name);
        Debug.DrawLine(transform.position, firePoint.position);
        
        // If it hits something...
        if (hit.collider.CompareTag("Enemy"))
        {
            Debug.Log("Enemy is too close");
            return true;
        }

        return false;
    }
    */

    public bool IsEnemyTooCloseCircle()
    {
        Collider2D[] targetsHit = Physics2D.OverlapCircleAll(originPos.position, circleRadius, enemyLayer);

        foreach (var target in targetsHit)
        {
            if(target.gameObject)
            {
               // Debug.Log("acertou " + target.gameObject.name);
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected() //Visualize the attack hitbox.
    {
        Gizmos.DrawWireSphere(originPos.position, circleRadius);
    }
}
