using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriestGaelBossAttacks : MonoBehaviour
{
    [SerializeField] private GaelBossAttack energyBallAttack;
    [Space(9)]
        
    [SerializeField] private GaelBossAttack clawAttack;
    [Space(9)]
    [SerializeField] private GaelBossAttack horizontalClawAttack;
    [Space(9)]

    [SerializeField] private GaelBossAttack multipleBallAttack;
    [SerializeField] private GameObject[] multipleBallPrefabs;
    private Queue<GameObject> multipleBulletPool = new();
    [Space(9)]

    [SerializeField] private ShieldPriestBossManager shieldManager;
    [SerializeField] private MinionSpawner minionSpawner;

    [SerializeField] MMF_Player ShieldSfxFeedback;

    private PriestGaelBoss priestGaelBoss;
    private Animator animator;

    private readonly string 
        IDLE_NAME = "Priest Idle", 
        ENERGY_BALL_NAME = "priest energyball throw", 
        LAUGH_NAME = "laugh animation", 
        CLAWS_NAME = "claw attack",
        HORIZONTAL_CLAW_NAME = "double claw animaiton";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        priestGaelBoss = GetComponent<PriestGaelBoss>();

        //Enqueue the elements.
        for (int i = 0; i < multipleBallPrefabs.Length; i++)
        {
            multipleBulletPool.Enqueue(multipleBallPrefabs[i]);
        }
    }


    public IEnumerator EnergyBallAttack()
    {
        //Play the energy-ball animation.
        //An animation event instantiates the energy ball.
        animator.Play(ENERGY_BALL_NAME);
        energyBallAttack.currentAttacks = Random.Range(0, energyBallAttack.MaxAttacks);

        while (energyBallAttack.currentAttacks < energyBallAttack.MaxAttacks)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        animator.Play(IDLE_NAME);
    }

    public IEnumerator MultipleEnergyBallsAttack()
    {
        //Play the energy-ball animation.
        //An animation event instantiates the energy ball.
        animator.Play(ENERGY_BALL_NAME);
        multipleBallAttack.currentAttacks = Random.Range(0, multipleBallAttack.MaxAttacks);

        while (multipleBallAttack.currentAttacks < multipleBallAttack.MaxAttacks)
        {
            yield return null;
        }

        animator.Play(IDLE_NAME);
    }

    public IEnumerator LaughAttack()
    {
        animator.Play(LAUGH_NAME);
        ShieldSfxFeedback.PlayFeedbacks();
        yield return new WaitForSeconds(1f);

        shieldManager.EnableShield();

        yield return new WaitForSeconds(3f);
        animator.Play(IDLE_NAME);
    }

    public IEnumerator ClawsAttack()
    {
        animator.Play(CLAWS_NAME);
        clawAttack.currentAttacks = 0;

        while (clawAttack.currentAttacks < clawAttack.MaxAttacks)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        animator.Play(IDLE_NAME);
    }

    public IEnumerator HorizontalClawAttack()
    {
        yield return new WaitForSeconds(0.5f);

        animator.Play(HORIZONTAL_CLAW_NAME);
        horizontalClawAttack.currentAttacks = 0;

        while (horizontalClawAttack.currentAttacks < horizontalClawAttack.MaxAttacks)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        animator.Play(IDLE_NAME);
    }

    /// <summary>
    /// Called by an animation event.
    /// </summary>
    public void ClawAttack_AnimationEvent()
    {
        Instantiate(clawAttack.AttackPrefab, clawAttack.AttackSpawnPoint.position, Quaternion.identity);
        clawAttack.currentAttacks++;
    }

    /// <summary>
    /// Called by an animation event.
    /// </summary>
    public void HorizontalClawAttack_AnimationEvent()
    {
        Instantiate(horizontalClawAttack.AttackPrefab, horizontalClawAttack.AttackSpawnPoint.position, Quaternion.identity);
        horizontalClawAttack.currentAttacks++;
    }

    /// <summary>
    /// Called by an animation event.
    /// </summary>
    public void ThrowBall_AnimationEvent()
    {
        switch (priestGaelBoss.currentState)
        {
            //Launch Alkarion projectiles that follow the player.
            case PriestGaelState.EnergyBall:
                var projectileBall = Instantiate(energyBallAttack.AttackPrefab, energyBallAttack.AttackSpawnPoint.position, Quaternion.identity);
                projectileBall.SetActive(true);
                energyBallAttack.currentAttacks++;
                break;
            case PriestGaelState.MultipleEnergyBalls: //Launch a projectile that emits additional Varia bullets.

                //Take the bullet at the front of the queue.
                var projectile = multipleBulletPool.Dequeue();
                //Move it to the spawn point.
                projectile.transform.position = multipleBallAttack.AttackSpawnPoint.position;
                projectile.GetComponent<MoveObjectToPos>().StartMove(0);
                //Start its firing pattern.
                StartCoroutine(projectile.GetComponent<VariaShootingManager>().EnableVariaAutoFire_Routine(5f));
                //Return it to the queue.
                multipleBulletPool.Enqueue(projectile);
                multipleBallAttack.currentAttacks++;
                break;
            default:
                break;
        }

    }

    public IEnumerator LaughSpawnMinionAttack()
    {
        animator.Play(LAUGH_NAME);
        yield return new WaitForSeconds(1f);

        StartCoroutine(minionSpawner.SpawnEnemies());

        yield return new WaitForSeconds(3f);
        animator.Play(IDLE_NAME);
    }



}


[System.Serializable]
public class GaelBossAttack
{
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private Transform attackSpawnPoint;
    [SerializeField] private int maxAttacks = 3;
    [HideInInspector] public int currentAttacks = 0;

    public GameObject AttackPrefab => attackPrefab;
    public Transform AttackSpawnPoint => attackSpawnPoint;
    public int MaxAttacks => maxAttacks;
}

