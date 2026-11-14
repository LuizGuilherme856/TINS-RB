
using UnityEngine;

//Deactivate objects when the player leaves the region.

public class CheckIfPlayerIsInRegion : MonoBehaviour
{
    [SerializeField] private GameObject gameObjectToDeativate;
    [SerializeField] private Health playerHealth;
    //private bool isPlayerInRegion;

    private void Awake()
    {
        gameObjectToDeativate.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player") )
        {
            Debug.Log("Player entered: " + name);
            //isPlayerInRegion = true;
            gameObjectToDeativate.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision  )
    {
        if (collision.gameObject.CompareTag("Player") )
        {
            Debug.Log("Player left: " + name);
            //isPlayerInRegion = false;

            if (playerHealth.IsAlive() == false) return; //Do not deactivate anything on death.
            gameObjectToDeativate.SetActive(false);
        }
    }
}
