
using UnityEngine;

public class FloatObject : MonoBehaviour
{
    [SerializeField] protected Vector3 speed;
    [SerializeField] protected float lifeTime = 0.8f;
    [SerializeField] protected bool useLifetime = true;

    protected float timer;

    protected void OnEnable()
    {
        timer = 0;
    }


    protected void FixedUpdate()
    {
        transform.position += speed * Time.fixedDeltaTime;

        if (useLifetime == false) return;

        if (timer < lifeTime)
        {
            timer += Time.fixedDeltaTime;
        }
        else
        {
            gameObject.SetActive(false);
        }

    }

    public void SetSpeed(Vector3 speed)
    {
        this.speed = speed;
    }

    public void SetSpeed(float xSpeed, float ySpeed)
    {
        this.speed = new Vector3(xSpeed, ySpeed);
    }
}
