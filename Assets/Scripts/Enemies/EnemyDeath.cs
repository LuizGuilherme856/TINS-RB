///Returns the enemy to its pool and handles death VFX.

using UnityEngine;

public class EnemyDeath : MonoBehaviour
{

    [SerializeField] private GameObject enemyCorpseImpact; //Impact is the damage type of the killing hit.
    [SerializeField] private GameObject corpseSlashHorizontal, corpseSlashUp, corpseSlashDown;
    [SerializeField] private GameObject corpseExplosion;

    [SerializeField] private bool hasFourDirections = true, useUnityPool = true;
    [Tooltip("Spawna só o corpo de impacto e não tem corpo slash etc. e tbm nao tem direções o corpo")]
    [SerializeField] private SimpleCorpseType onlySimpleImpactCorpse = SimpleCorpseType.no; //Simple corpse mode always uses the impact corpse, ignoring other variants.

    [SerializeField] private bool corpseFromPool = true;
    private EnemyMovementAnimation enemyMovement;
    private PoolCreator poolCreator;


    private CorpsePoolManager corpsePoolManager;

    private Vector3 corpseLocalScale;
    private bool hasSpawnedCorpse = false;

    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovementAnimation>();
        corpseLocalScale = enemyCorpseImpact != null ? enemyCorpseImpact.transform.localScale : Vector3.one;

        poolCreator = GetComponent<PoolCreator>();

        corpsePoolManager = GameObject.FindGameObjectWithTag("CorpsePoolManager")?.GetComponent<CorpsePoolManager>();

    }

    private void OnEnable()
    {
        hasSpawnedCorpse = false; //Reset on every enable.
        //Required for pooled reuse.
    }

    public void HandleDeath(Vector3 lastHitDirection, DamageType damageType)
    {
        if (!hasSpawnedCorpse)
        {
            SpawnCorpse(lastHitDirection, damageType);
            hasSpawnedCorpse = true;
        }

        bool campaignPooled = TINS.Campaign.CampaignCombatRules.IsCampaignMission &&
            poolCreator != null && poolCreator.HasPool;
        if (useUnityPool || campaignPooled) poolCreator.ReleaseToPool();
        else gameObject.SetActive(false);
    }

    private void SpawnCorpse(Vector3 lastHitDirection, DamageType damageType)
    {
        if (!corpseFromPool || corpsePoolManager == null || TINS.Campaign.CampaignCombatRules.IsCampaignMission)
        {
            if (enemyCorpseImpact != null)
            {
                GameObject corpse = Instantiate(enemyCorpseImpact, transform.position, transform.rotation);
                TINS.Campaign.CampaignMissionRuntime.Active?.TrackCorpse(corpse);
            }
            return;
        }


        if (onlySimpleImpactCorpse == SimpleCorpseType.staticCorpse) //Static bodies receive no force.
        {
            //Instantiate(enemyCorpseImpact, transform.position, transform.rotation);
            var simpleCorpse = corpsePoolManager.GetEnemyCorpseFromPool(enemyCorpseImpact.name);
            simpleCorpse.transform.position = transform.position;
            return;
        }
        else if (onlySimpleImpactCorpse == SimpleCorpseType.dynamicCorpse) //Apply force in the direction of the last shot.
        {
            var simpleCorpse = corpsePoolManager.GetEnemyCorpseFromPool(enemyCorpseImpact.name);
            WholeCorpseSpawn(simpleCorpse, lastHitDirection);
            return;
        }


        if (hasFourDirections)
        {
            var enemyDirection = enemyMovement.GetDirection();


            if (damageType == DamageType.Slash)
            {
                // corpse = corpsePoolManager.GetEnemyCorpseFromPool(enemy.name);
                SlashedCorpseSpawn(lastHitDirection, enemyDirection);
            }
            else if (damageType == DamageType.Explosive || damageType == DamageType.Ice)
            {
                var corpse = ExplodedCorpseSpawn(lastHitDirection);
            }
            else if (damageType == DamageType.Fire)
            {
                var corpse = corpsePoolManager.GetEnemyCorpseFromPool(enemyCorpseImpact.name);
                corpse.GetComponent<ShaderReplacer>().SetShaderValue(damageType);
                WholeCorpseSpawn(corpse, lastHitDirection, enemyDirection);
            }
            else
            {
                var corpse = corpsePoolManager.GetEnemyCorpseFromPool(enemyCorpseImpact.name);
                WholeCorpseSpawn(corpse, lastHitDirection, enemyDirection);
            }


        }
        else
        {

            if (damageType == DamageType.Slash)
            {
                var corpse = SlashedCorpseSpawn(lastHitDirection);
            }
            else if (damageType == DamageType.Explosive || damageType == DamageType.Ice)
            {
                var corpse = ExplodedCorpseSpawn(lastHitDirection);
            }
            else if (damageType == DamageType.Fire)
            {
                var corpse = corpsePoolManager.GetEnemyCorpseFromPool(enemyCorpseImpact.name);
                corpse.GetComponent<ShaderReplacer>().SetShaderValue(damageType);
                WholeCorpseSpawn(corpse, lastHitDirection);
            }
            else
            {
                var corpse = corpsePoolManager.GetEnemyCorpseFromPool(enemyCorpseImpact.name);
                WholeCorpseSpawn(corpse, lastHitDirection);
            }
        }

    }

    private GameObject SlashedCorpseSpawn(Vector3 lastHitDirection, Direction enemyDirection = Direction.East)
    {
        GameObject newEnemyCorpse;
        if (enemyDirection == Direction.North)
        {
            newEnemyCorpse = corpsePoolManager.GetEnemyCorpseFromPool(corpseSlashUp.name);
            ApplyFeedbacksOnEveryChild(lastHitDirection, newEnemyCorpse);
        }
        else if (enemyDirection == Direction.South)
        {
            newEnemyCorpse = corpsePoolManager.GetEnemyCorpseFromPool(corpseSlashDown.name);
            ApplyFeedbacksOnEveryChild(lastHitDirection, newEnemyCorpse);
        }
        else if (enemyDirection == Direction.East)
        {

            newEnemyCorpse = corpsePoolManager.GetEnemyCorpseFromPool(corpseSlashHorizontal.name);
            ApplyFeedbacksOnEveryChild(lastHitDirection, newEnemyCorpse);
        }
        else
        {
            newEnemyCorpse = corpsePoolManager.GetEnemyCorpseFromPool(corpseSlashHorizontal.name);
            ApplyFeedbacksOnEveryChild(lastHitDirection, newEnemyCorpse);
            newEnemyCorpse.transform.localScale = new Vector3(-corpseLocalScale.x, corpseLocalScale.y, 1);
        }

        return newEnemyCorpse;
    }

    private void ApplyFeedbacksOnEveryChild(Vector3 lastHitDirection, GameObject newEnemyCorpse)
    {
        newEnemyCorpse.transform.position = transform.position;

        foreach (Transform child in newEnemyCorpse.transform)
        {
            child.gameObject.GetComponent<ImpactOnTheFloor>().SetImpulse(lastHitDirection, 0.5f, true, true);
            child.gameObject.GetComponent<PlayFeedbacks>().PlayAllFeedbacks();
        }
    }

    private void WholeCorpseSpawn(GameObject newEnemyCorpse, Vector3 lastHitDirection, Direction enemyDirection = Direction.East)
    {
        //var newEnemyCorpse = Instantiate(enemyCorpseImpact);
        newEnemyCorpse.transform.position = transform.position;
        newEnemyCorpse.GetComponent<ImpactOnTheFloor>().SetImpulse(lastHitDirection, 0.5f, true, true);
        newEnemyCorpse.GetComponent<PlayFeedbacks>().PlayAllFeedbacks();
        //ApplyFeedbacksOnEveryChild(lastHitDirection, newEnemyCorpse);

        if (hasFourDirections)
        {
            var corpseAnimator = newEnemyCorpse.GetComponentInChildren<Animator>();
            if (enemyDirection == Direction.North)
            {
                corpseAnimator.SetTrigger("Up");
            }
            else if (enemyDirection == Direction.South)
            {
                corpseAnimator.SetTrigger("Down");
            }
            else if (enemyDirection == Direction.East)
            {
                corpseAnimator.SetTrigger("Horizontal");
                newEnemyCorpse.transform.localScale = new Vector3(corpseLocalScale.x, corpseLocalScale.y, 1);
            }
            else
            {
                corpseAnimator.SetTrigger("Horizontal");
                newEnemyCorpse.transform.localScale = new Vector3(-corpseLocalScale.x, corpseLocalScale.y, 1);
            }
        }
        else //Use sprite flipping when four directional variants are unavailable.
        {
            if (transform.localScale.x < 0)
            {
                newEnemyCorpse.transform.localScale = new Vector3(-corpseLocalScale.x, corpseLocalScale.y, 1);
            }
            else
            {
                newEnemyCorpse.transform.localScale = new Vector3(corpseLocalScale.x, corpseLocalScale.y, 1);
            }
        }

        //return newEnemyCorpse;
    }

    private GameObject ExplodedCorpseSpawn(Vector2 lastHitDirection)
    {
        var newEnemyCorpse = corpsePoolManager.GetEnemyCorpseFromPool(corpseExplosion.name);
        newEnemyCorpse.SetActive(true);
        newEnemyCorpse.transform.position = transform.position;


        foreach (Transform child in newEnemyCorpse.transform)
        {
            child.gameObject.SetActive(true);
            var newRandomHitDirection = lastHitDirection * Random.Range(-2, 2); //Randomize force magnitude and direction slightly.
            var randomness = Random.Range(-0.5f, 0.5f); //Randomize the direction slightly.
            newRandomHitDirection = new Vector2(newRandomHitDirection.x + newRandomHitDirection.x * randomness, newRandomHitDirection.y + newRandomHitDirection.y * randomness);

            if (Random.Range(0, 2) == 0) //50% chance to invert the axes.
            {
                newRandomHitDirection = new Vector2(newRandomHitDirection.y, newRandomHitDirection.x);
            }

            child.gameObject.GetComponent<ImpactOnTheFloor>().SetImpulse(newRandomHitDirection, 0.5f, true, true);
        }

        return newEnemyCorpse;
    }

    private enum SimpleCorpseType { no, staticCorpse, dynamicCorpse }
}
