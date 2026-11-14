using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] protected TrajectoryMode mode;
    [SerializeField] float senoidSpeed = 0.01f;
    //[SerializeField] Vector2 ra�dius;

    [SerializeField] protected bool isStatic = false; //Support non-gun objects such as traps and upgrades.
    [SerializeField] protected Vector3 spinDirection;

    protected float projectileSpeed = 10f;
    protected int direction = 1;
    protected Rigidbody2D rb;
    protected float angle, radAngle, t = 0;
    protected Vector3 velocityBeforeCollision; //Cache velocity before the collision clears it.

    protected Transform controlPoint; // Bezier control point.
    protected Transform target; // Movement target.

    protected void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected void OnEnable()
    {
        RefreshVelocity();

        if (isStatic)
        {
            velocityBeforeCollision = spinDirection;
        }
    }

    public void RefreshVelocity()
    {
        if (isStatic) return;

        radAngle = angle * Mathf.Deg2Rad;

        if (mode == TrajectoryMode.none)
            rb.linearVelocity = new Vector2(direction * projectileSpeed * Mathf.Cos(angle * Mathf.Deg2Rad), projectileSpeed * Mathf.Sin(angle * Mathf.Deg2Rad));
        else//Known issue: not working as intended.
        {
            rb.linearVelocity = new Vector2(direction * projectileSpeed * Mathf.Cos(angle * Mathf.Deg2Rad), projectileSpeed * Mathf.Sin(angle * Mathf.Deg2Rad));

            t = 0;
        }

    }

    protected void FixedUpdate()
    {
        if (mode != TrajectoryMode.none)
        {
            HandleTrajectoryModes();
        }

        if(rb.linearVelocity.magnitude > 0) //A positive value indicates no collision has occurred yet.
        {
            velocityBeforeCollision = rb.linearVelocity;
        }
    }

    protected void HandleTrajectoryModes()
    {
        t += senoidSpeed * Time.deltaTime;
        if (mode == TrajectoryMode.increasingAmplitudeMode)
            rb.linearVelocity = new Vector2(direction * projectileSpeed * Mathf.Cos(radAngle) + t * Mathf.Sin(radAngle) * Mathf.Sin(t), Mathf.Sin(radAngle) * projectileSpeed - Mathf.Sin(t) * Mathf.Cos(radAngle) * t);
        else if (mode == TrajectoryMode.sinExpMode)
            rb.linearVelocity = new Vector2(direction * projectileSpeed * Mathf.Cos(radAngle) + Mathf.Exp(t) * Mathf.Sin(radAngle) * Mathf.Sin(t), Mathf.Sin(radAngle) * projectileSpeed - Mathf.Sin(t) * Mathf.Cos(radAngle) * Mathf.Exp(t));
        else if (mode == TrajectoryMode.sqrtMode)
            rb.linearVelocity = new Vector2(direction * projectileSpeed * Mathf.Cos(radAngle) + Mathf.Sqrt(t) * Mathf.Sin(radAngle) * Mathf.Sin(t), Mathf.Sin(radAngle) * projectileSpeed - Mathf.Sin(t) * Mathf.Cos(radAngle) * Mathf.Sqrt(t));
        else if (mode == TrajectoryMode.tgMode)
            rb.linearVelocity = new Vector2(direction * projectileSpeed * Mathf.Cos(radAngle) + Mathf.Tan(t) * Mathf.Sin(radAngle) * Mathf.Sin(t), Mathf.Sin(radAngle) * projectileSpeed - Mathf.Sin(t) * Mathf.Cos(radAngle) * Mathf.Tan(t));
        else if (mode == TrajectoryMode.expMode)
            rb.linearVelocity = new Vector2(direction * projectileSpeed * Mathf.Cos(radAngle) + t * Mathf.Sin(radAngle) * Mathf.Exp(t), Mathf.Sin(radAngle) * projectileSpeed + Mathf.Exp(t) * Mathf.Cos(radAngle) * t);
        else if (mode == TrajectoryMode.boomerangMode)
            rb.linearVelocity -= new Vector2(direction * t, t);
    }

    public void SetDirection(int direction)
    {
        this.direction = direction;

    }

    public void SetRotation(Vector3 rotation)
    {
        if (direction == 1)
            transform.eulerAngles = rotation;
        else
            transform.eulerAngles = -rotation;
        angle = rotation.z;
    }

    public void SetAngle(float angle)
    {
        this.angle = angle;
    }

    public void SetprojectileSpeed(float speed)
    {
        projectileSpeed = speed;
    }


    public int GetDirection()
    {
        return direction;
    }

    public float GetProjectileSpeed()
    {
        return projectileSpeed;
    }

    public float GetRotationAngle()
    {
        return angle;
    }

    public Vector3 GetVelocityBeforeCollision()
    {
        return velocityBeforeCollision;
    }
    public Vector3 GetMomentumBeforeCollision() //Use momentum, mass times velocity, for collision calculations.
    {
        return velocityBeforeCollision*rb.mass;
    }

    public void multScale(float targetMod)
    {
        transform.localScale = transform.localScale * targetMod;
    }


    /// <summary>
    /// Move toward a target along a quadratic Bezier curve.
    /// </summary>

    public void MoveTowardsTarget(Transform target)
    {
        this.target = target;

        // Create a control point if none is assigned.
        if (controlPoint == null)
        {
            GameObject controlPointObj = new GameObject("ControlPoint");
            controlPoint = controlPointObj.transform;
            controlPoint.position = (transform.position + target.position) / 2 + Vector3.up * 2; // Position the control point.
        }

        // Start interpolation.
        t = 0;
        StartCoroutine(MoveAlongCurve());
    }

    private IEnumerator MoveAlongCurve()
    {
        Vector3 startPoint = transform.position;
        Vector3 endPoint = target.position;

        while (t < 1)
        {
            t += Time.deltaTime * projectileSpeed;

            // Calculate the point along the Bezier curve.
            Vector3 currentPosition = CalculateBezierPoint(t, startPoint, controlPoint.position, endPoint);
            transform.position = currentPosition;

            //// Face the target.
            LookAt2D(target);
            //transform.LookAt(target)

            yield return null;
        }

        // Set the exact target position at completion.
        transform.position = endPoint;
        transform.gameObject.GetComponent<HandleBulletImpact>().SpawnBulletImpact(transform.position);
    }

    /// <summary>
    /// Face a target in 2D.
    /// </summary>
    /// <param name="target">Target transform.</param>
    private void LookAt2D(Transform target)
    {
        // Calculate target direction.
        Vector2 direction = (target.position - transform.position).normalized;

        // Calculate the angle from the X axis.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Apply Z rotation.
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // Quadratic Bezier formula.
        return Mathf.Pow(1 - t, 2) * p0 +
               2 * (1 - t) * t * p1 +
               Mathf.Pow(t, 2) * p2;
    }

}

public enum TrajectoryMode {none, sinExpMode, sqrtMode, tgMode, expMode, boomerangMode, increasingAmplitudeMode, variaBullet }


