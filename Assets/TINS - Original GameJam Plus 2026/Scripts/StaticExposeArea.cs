using UnityEngine;

public class StaticExposeArea : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealth targetHealth = collision.gameObject.GetComponent<EnemyHealth>();
            targetHealth.IsProtectedUntilPlayerMelee = false;
        }
    }
}
