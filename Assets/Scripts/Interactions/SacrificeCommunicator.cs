using UnityEngine;

/// Tells the ManageScarifice that the enemy died withing the range

public class SacrificeCommunicator : MonoBehaviour
{
    private OrbSpawner OrbSpawner;
    private EnemyHealth enemyHealth;
    private Transform altarTransform;
    [SerializeField] private int numberOfSoulsToSpawn = 1;
    [SerializeField] private GameObject sacrificeIcon;
    ManageBloodRitual manageRitual;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void Start()
    {
        enemyHealth.OnZeroHealth += HandleDeath;
    }

    private void OnEnable()
    {
        sacrificeIcon.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Altar"))
        {
            OrbSpawner = collision.gameObject.GetComponent<OrbSpawner>();

            manageRitual = collision.gameObject.GetComponent<ManageBloodRitual>();
            altarTransform = manageRitual.ritualCenter;

            if (OrbSpawner.IsCompleted() == false)
            {
                sacrificeIcon.SetActive(true);
                manageRitual.OnCompleted += DeactivateSacrificeIconOnComplete;
            }
            // Aux.Print("Enemy within altar range", showDebug);
        }
    }

    private void DeactivateSacrificeIconOnComplete()
    {
        sacrificeIcon.SetActive(false);
        manageRitual.OnCompleted -= DeactivateSacrificeIconOnComplete;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Altar"))
        {
            sacrificeIcon.SetActive(false);
            manageRitual.OnCompleted -= DeactivateSacrificeIconOnComplete;
        }
    }


    private void HandleDeath()
  {
        if (OrbSpawner == null) return;

        if (Vector2.Distance(altarTransform.position, transform.position) < OrbSpawner.GetRange())
        {
            for (int i = 0; i < numberOfSoulsToSpawn; i++)
            {
                if (i > 0) //If more than one orb should spawn,
                {
                    float addRandomValueX = Random.Range(-0.5f, 0.5f);
                    float addRandomValueY = Random.Range(-0.5f, 0.5f);
                    OrbSpawner.SpawnOrb(transform.position + new Vector3(addRandomValueX, addRandomValueY, 0));
                }
                else
                {
                    OrbSpawner.SpawnOrb(transform.position);
                }
            }
        }
    }
}
