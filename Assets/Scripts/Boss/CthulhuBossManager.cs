using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


/*Notify the other component to teleport, attack, or perform the upcoming action here.
 * */
public class CthulhuBossManager : MonoBehaviour
{
    [SerializeField] private float minTimeTp = 5f, MaxTimeTp = 10f;
    [SerializeField] private float minTimeAttack = 5f, MaxTimeAttack = 10f;
    [SerializeField] private thunderAttack_cthulhuBoss thunderAttack;
    [Space(10)]
    [SerializeField] private CthulhuShield cthulhuShield;

    [Space(15)]
    [Header("Teleport")]
    [SerializeField] private Vector3 minTeleportPos;
    [SerializeField] private Vector3 maxTeleportPos;
    [SerializeField] private MMF_Player teleportFeedbacks;

    [Space(10)]
    [Header("SFX feedbacks")]
    [SerializeField] private MMF_Player castLoopFeedback;
    [SerializeField] private MMF_Player radialShotFeedback;
    [SerializeField] private MMF_Player speakFeedback;


    private CthulhuState currentState;

    private float tpTimer = 10f, attackTimer = 5f;
    private Animator myAnimator;
    private MoveTowardsPlayer moveTowardsPlayer;
    private AttackPlayer attackPlayer;
    private BossNextForm bossNextForm;
    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
        moveTowardsPlayer = GetComponent<MoveTowardsPlayer>();
        attackPlayer = GetComponent<AttackPlayer>();
        bossNextForm = GetComponent<BossNextForm>();
        
    }

    private void OnEnable()
    {
        currentState = CthulhuState.flying;
        thunderAttack.OnStopAttacking += StopAttacking;
    }

    private void OnDisable()
    {
        thunderAttack.OnStopAttacking -= StopAttacking;
    }

    private void FixedUpdate()
    {
        if (currentState != CthulhuState.flying) return; //Do not interrupt the attack to teleport.
        CheckTeleportTime();
        CheckAttackTime();
    }

    private void CheckTeleportTime()
    {
        tpTimer -= Time.fixedDeltaTime;
        if (tpTimer <= 0)
        {
            TriggerTeleportAnimation();
            tpTimer = Random.Range(minTimeTp, MaxTimeTp);
        }
    }

    private void CheckAttackTime()
    {
        attackTimer -= Time.fixedDeltaTime;
        if (attackTimer <= 0)
        {
            TriggerAttack();
            attackTimer = Random.Range(minTimeAttack, MaxTimeAttack);
        }
    }

    private void TriggerTeleportAnimation()
    {
        StopMoving();
        currentState = CthulhuState.teleporting;
        myAnimator.SetTrigger("Tp");

    }

    private void TriggerAttack()
    {
        StopMoving();
        speakFeedback.PlayFeedbacks();
        cthulhuShield.ActivateShield();
        currentState = CthulhuState.attacking;
        thunderAttack.SetCallThunder(true);
        myAnimator.SetTrigger("Attack_");

    }

    public void RadialShot()//Called by the Animator.
    {
        if (bossNextForm.currentForm ==0)
        {
            return;
        }
        else if(bossNextForm.currentForm == 1)
        {
            attackPlayer.InstantiateProjectile(false, 12, 7, 26);

        }
        else
        {
            attackPlayer.InstantiateProjectile(false, 28, 7, 26);
            Invoke(nameof(RadialShot_aux), 1.6f);
        }

        radialShotFeedback.PlayFeedbacks();
    }

    private void RadialShot_aux()//Wrapper so the method can be called through Invoke().
    {
        attackPlayer.InstantiateProjectile(false, 30, 7, 16);
        radialShotFeedback.PlayFeedbacks();
    }


    private void StopMoving()
    {
        moveTowardsPlayer.SetCanMove(false);
        //Debug.Log("stop moving");
    }

    public  void StartMoving() //Called by the Animator.
    {
        currentState = CthulhuState.flying;
        moveTowardsPlayer.SetCanMove(true);
    }

    private void StopAttacking()
    {
        StartMoving();
        cthulhuShield.DeactivateShield();
        thunderAttack.SetCallThunder(false);
        myAnimator.SetTrigger("StopAttack");

    }

    public void Teleport() //Called by the Animator.
    {
        //Randomize the new position.
        float newX = Random.Range(minTeleportPos.x, maxTeleportPos.x);
        newX = Aux.RandomizeFloatSign(newX);
        newX += transform.position.x;
        float newY = Random.Range(minTeleportPos.y, maxTeleportPos.y);
        newY = Aux.RandomizeFloatSign(newY);
        newY += transform.position.y;

        transform.position = new Vector3(newX , newY, 0);
       // teleportFeedbacks.PlayFeedbacks();
    }

    public void PlayCastLoopFeedback()//animator
    {
        castLoopFeedback.PlayFeedbacks();
    }

    public void PlayTeleportFeedback() //animator
    {
        teleportFeedbacks.PlayFeedbacks();
    }



    enum CthulhuState { flying, attacking, teleporting}
}
