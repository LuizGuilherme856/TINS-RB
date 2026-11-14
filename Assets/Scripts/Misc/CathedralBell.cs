using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CathedralBell : MonoBehaviour
{
    public int bellIndex;
    [SerializeField] private GameObject numberGlow;
    private BellPuzzleManager bellPuzzleManager;

    private float timer, minTimeBetweenShots = 0.5f; //Prevent multi-projectile weapons from registering repeated hits.

    private void Awake()
    {
        bellPuzzleManager = FindFirstObjectByType<BellPuzzleManager>();
        numberGlow.SetActive(false);

        
    }

    private void FixedUpdate()
    {
        if (timer > 0) timer -= Time.fixedDeltaTime;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            if(timer <= 0)
            {
                bellPuzzleManager.RingBell(bellIndex);
                timer = minTimeBetweenShots;

            }
            

        }
    }

    public void CorrectOrder()
    {
        numberGlow.SetActive(true);
        Debug.Log("Correct order");
    }

    public void ResetPuzzle()
    {
        numberGlow.SetActive(false);
    }
}
