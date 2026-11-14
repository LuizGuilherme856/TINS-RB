///Controls raycast-based laser weapons.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastShootingHandler : MonoBehaviour
{
    ///Disabled until laser weapons are reintroduced.
    /*
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask laserColliderLayer;

    private Aim aim;
    private LineRenderer lineRenderer;
    private GunData gunData;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }


    public void UpdateBulletData(Aim aim, GunData gunData)
    {
        this.aim = aim;
        this.gunData = gunData;
        //muzzleFlashSpawner.UpdateBulletData(gunData);
    }
    public void ShootLaser()
    {
        var direction = new Vector3(0, 0, aim.GetAimAngle());
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction.normalized, gunData.laserMaxRange, laserColliderLayer);
        Debug.Log(direction);
        if ( hit)
        {
            //lineRenderer.SetPosition(1, hit.point);
            Draw2DRay(firePoint.position, hit.point);
            
        }
        else
        {
            //Draw2DRay(firePoint.position, firePoint.transform.right*gunData.laserMaxRange);
        }

    }

    private void Draw2DRay(Vector2 startPos, Vector2 endPos)
    {
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }
    */
}
