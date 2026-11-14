using System.Collections;
using UnityEngine;

public class AmmoBringer : MonoBehaviour
{
    [Header("Prefabs e referências")]
    [SerializeField] private GameObject ammoCratePrefab;
    [Tooltip("Se você ativar um storkInstance em cena em vez de instanciar o prefab, o script começa ao OnEnable.")]
    [SerializeField] private Transform spawnPointOverride;

    [Header("Movimento")]
    [SerializeField] private float speed = 5f;
    [Tooltip("Se true move da esquerda para a direita; se false, da direita para a esquerda.")]
    [SerializeField] private bool leftToRight = true;
    [Tooltip("Altura relativa à posição do jogador onde o Stork deve estar ao passar (p.ex. sobre a cabeça).")]
    [SerializeField] private float headOffsetY = 1.5f;

    [Header("Comportamento")]
    [SerializeField][Tooltip("Tempo que o stork espera sobre a cabeça do jogador antes de voltar a voar.")]
    private float waitToMove = 5f;
    [SerializeField][Tooltip("Distância horizontal máxima do jogador para considerar fora da tela e destruir.")]
    private float cleanupDistanceFromPlayer = 50f;
    [SerializeField]
    [Tooltip("Velocidade do crate ao se mover em direção ao jogador.")]
    private float crateSpeed = 3f;

    private Transform playerTransform;
    private bool hasSpawnedCrate = false;
    private bool isWaiting = false;
    private float previousX;

    private void OnEnable()
    {
        // Resolve the player through PlayerInputHandler, then fall back to the tag.
        var ph = FindFirstObjectByType<PlayerInputHandler>();
        if (ph != null) playerTransform = ph.transform;
        else
        {
            var newPlayerPos = GameObject.FindGameObjectWithTag("Player");
            if (newPlayerPos != null) playerTransform = newPlayerPos.transform;
        }

        previousX = transform.position.x;
        hasSpawnedCrate = false;
        isWaiting = false;
        StartCoroutine(MoveAndDeliver());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator MoveAndDeliver()
    {
        // Optionally force spawnPoint when activating storkInstance at a specific location.
        if (spawnPointOverride != null)
            transform.position = spawnPointOverride.position;

        // Fly, pause over the player to drop the crate, wait, then resume flight.
        while (true)
        {
            // Update previousX only when moving so crossing detection remains correct.

            // Stay still until waitToMove expires.
            if (!isWaiting)
            {
                float dir = leftToRight ? 1f : -1f;
                transform.position += Vector3.right * dir * speed * Time.deltaTime;
            }

            // Check for crossing above the player if the crate has not dropped and the player exists.
            if (!hasSpawnedCrate && playerTransform != null)
            {
                float playerX = playerTransform.position.x;
                float storkY = transform.position.y;
                float playerHeadY = playerTransform.position.y + headOffsetY;

                bool crossedX = transform.position.x >= playerX;


                if (crossedX)
                {
                    SpawnCrateAbovePlayer();
                    hasSpawnedCrate = true;

                    // Pause above the player for waitToMove seconds.
                    isWaiting = true;
                    yield return new WaitForSeconds(waitToMove);
                    isWaiting = false;

                    // Resume flight in the same direction after waiting;
                    // clean up only after moving sufficiently offscreen.
                }
            }
            else if (playerTransform == null)
            {
                Debug.LogWarning("AmmoBringer: jogador não encontrado; não será possível dropar o crate.");
            }

            // After dropping the crate, wait until offscreen before cleanup.
            if (hasSpawnedCrate)
            {
                float referenceX = playerTransform != null ? playerTransform.position.x : 0f;
                if (Mathf.Abs(transform.position.x - referenceX) > cleanupDistanceFromPlayer)
                {
                    TryCleanup();
                    yield break;
                }
            }

            yield return null;
        }
    }

    private void SpawnCrateAbovePlayer()
    {
        if (ammoCratePrefab == null)
        {
            Debug.LogWarning("AmmoBringer: ammoCratePrefab não atribuído.");
            return;
        }

        Vector3 cratePos;

        cratePos = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        

        var crate = Instantiate(ammoCratePrefab, cratePos, Quaternion.identity);
        Debug.Log("AmmoBringer: Crate spawned at " + cratePos);
        StartCoroutine(MoveCrateToPlayer(crate));
    }

    private IEnumerator MoveCrateToPlayer(GameObject crate)
    {
        while (crate != null && playerTransform != null)
        {
            Vector3 directionToPlayer = (playerTransform.position - crate.transform.position).normalized;
            crate.transform.position += directionToPlayer * crateSpeed * Time.deltaTime;

            // Check whether the object is within 0.5 units of the player.
            if (Vector3.Distance(crate.transform.position, playerTransform.position) < 0.5f)
            {
                yield break;
            }

            yield return null;
        }
    }

    private void TryCleanup()
    {
        // Destroy dynamically created instances; scene-owned instances may instead
        // need SetActive(false).
        Destroy(gameObject);
    }
}
