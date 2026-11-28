///Handles attacks against the player.
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;
using UnityEngine;
using System;
using MoreMountains.Feedbacks;
using Random = UnityEngine.Random;
using TINS.Campaign;

public class AttackPlayer : MonoBehaviour
{
     public Transform player;
     protected EnemyData enemyData;

    [Header("RANGED ATTACK")]
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected bool hasRangedAttacks = false, autoAttack = false, hasFeedback = false;
    [SerializeField] protected MMMiniObjectPooler miniObjectPooler;
    [SerializeField] protected MMF_Player rangedAttackFeedback_shoot;
    [SerializeField] protected GlobalStats globalStats;
    public event Action OnAttackPlayer;
    protected HandleAttackOnEnemy handleAttackOnEnemy;


    protected Rigidbody2D myRigidbody;
    //protected Animator myAnimator;
    protected float timerAfterRespawnToAttack;

    protected float playerContactTimer = 0;// Time in contact with the player; reset after each attack.
    protected float timeSinceLastShot = 0f, multipleBulletsAngleOffset=0;
    protected IObjectPool<PoolCreator> bulletPool;

    protected void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        //myAnimator = GetComponent<Animator>();
        enemyData = GetComponent<EnemyHealth>().enemyData;
        bulletPool = new ObjectPool<PoolCreator>(CreatePoolObject, OnGet, OnRelease, OnDestroyPoolObject, maxSize: 25);

        handleAttackOnEnemy = GetComponent<HandleAttackOnEnemy>();

        if (globalStats == null)
        {
            globalStats = AssetFinder.GetGlobalStats();
        }
    }
    protected void Start()
    {
        if(player == null) player = GameObject.FindGameObjectWithTag("Player").transform; //This lookup may be redundant because EnemyAI already stores the reference.
    }

    protected void OnEnable()
    {
        timerAfterRespawnToAttack = enemyData.timeAfterRespawnToAttack;
        playerContactTimer = 0; 
    }


    protected void Update()
    {
        timeSinceLastShot += Time.deltaTime;

        if (CampaignCombatRules.IsCampaignMission) return;

        if(timerAfterRespawnToAttack > 0)
        {
            timerAfterRespawnToAttack -= Time.deltaTime;
        }

        if (hasRangedAttacks)
        {

            if (Vector2.Distance(transform.position, player.position) <= enemyData.maxDistanceToShoot  && timeSinceLastShot >= enemyData.GetFireRate() )
            {
                //myAnimator.SetTrigger("Attack");
                if(autoAttack) //Fire automatically without an external call.
                {
                    InstantiateProjectile();
                }
                OnAttackPlayer?.Invoke();
                timeSinceLastShot = 0;
            }

        }
    }

    #region MELEE ATTACK
  
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (CampaignCombatRules.IsCampaignMission) return;
        if (timerAfterRespawnToAttack > 0) return; //Minimum delay before attacking the player after respawning.

        if (collision.gameObject.CompareTag("Player"))
        {
            //Damage the player.
            var  handleDamageOnPlayer = collision.gameObject.GetComponent<HandleAttackOnPlayer>();
            Vector2 attackDirectionForce = myRigidbody.linearVelocity.normalized * enemyData.attackForce;
            handleDamageOnPlayer.DamagePlayer(enemyData.attackDamage, attackDirectionForce);
            ThornDamage();
        }
    }
 
    
    protected void OnCollisionStay2D(Collision2D collision)
    {
        if (CampaignCombatRules.IsCampaignMission) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            playerContactTimer += Time.deltaTime;
            //Damage the player.
            if(playerContactTimer >= enemyData.attackRate)
            {
                Attack(collision);
                playerContactTimer = 0;
            }

        }
    }
    

    protected void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerContactTimer = 0;

        }
    }

    #endregion

    protected virtual void Attack(Collision2D collision)
    {

        var handleDamageOnPlayer = collision.gameObject.GetComponent<HandleAttackOnPlayer>();
        Vector2 attackDirectionForce = myRigidbody.linearVelocity.normalized * enemyData.attackForce;
        handleDamageOnPlayer.DamagePlayer(enemyData.attackDamage, attackDirectionForce);

        ThornDamage();

    }

    /// <summary>
    /// Take damage when damaging the player.
    /// </summary>

    protected void ThornDamage()
    {
        if (globalStats.ForCurrentRun.ThornDamage > 0)
        {
            var newDamage = globalStats.ForCurrentRun.ThornDamage + globalStats.ForCurrentRun.IncreaseGlobalDamageBy;
            handleAttackOnEnemy.AttackEnemy_SimpleVersion(globalStats.ForCurrentRun.ThornDamageType, newDamage);
        }

        if (Random.Range(0.1f, 1f) < globalStats.ForCurrentRun.ThornInstaKillChance)
        {
            handleAttackOnEnemy.InstaKill(globalStats.ForCurrentRun.ThornDamageType);
        }
    }

    public void CallInstantiateProjectileAnimator() //Animation-event adapter because Animator cannot supply InstantiateProjectile's parameters.
    {
        InstantiateProjectile();
    }

    public void CallInstantiateProjectile4Directions_Animation() //Animation-event adapter because Animator cannot supply InstantiateProjectile's parameters.
    {
       var projectiles =  InstantiateProjectile(false, enemyData.GetBulletsPerShot(), (int)enemyData.ProjectileSpeed,  0, true);

        foreach (var projectile in projectiles)
        {
            projectile.transform.parent = null; //Detach from the parent.
            projectile.transform.rotation = Quaternion.identity;
            projectile.gameObject.SetActive(true);
            projectile.MoveTowardsTarget(player);
        }
    }


    /// <summary>
    ///  Use the supplied parameters or the enemy data.
    ///  Return the projectiles so callers can modify them afterward.
    /// </summary>
    public virtual Projectile[] InstantiateProjectile(bool useEnemyDataInfo = true, 
        int bulletsPerShot = 0, int projectileSpeed = 0, int multipleBulletAngleOffset =0, bool useFourDirections = false) //Called by the Animator.
    {
        if (CampaignCombatRules.IsCampaignMission) return Array.Empty<Projectile>();
        if(hasFeedback)
        {
            rangedAttackFeedback_shoot.PlayFeedbacks();
        }

        int _bulletsPerShot;
        float _projectileSpeed, _multipleBulletAngleOffset;
        
        if (useEnemyDataInfo)
        {
            _multipleBulletAngleOffset = enemyData.multipleBulletAngleOffset;
            _bulletsPerShot = enemyData.GetBulletsPerShot();
            _projectileSpeed = enemyData.ProjectileSpeed;
        }
        else //Use the supplied parameters.
        {
            _multipleBulletAngleOffset = multipleBulletAngleOffset;
            _bulletsPerShot = bulletsPerShot;
            _projectileSpeed = projectileSpeed;
        }


        Projectile[] projectiles = new Projectile[_bulletsPerShot]; //Return an array because some weapons fire multiple bullets per shot.

        multipleBulletsAngleOffset = 0;

        float angle;

        if (useFourDirections)
        {
            // Calculate the angle from the four cardinal directions.
            Vector2 direction = player.position - firePoint.position;
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                angle = direction.x > 0 ? 0 : 180; // Right or left.
            }
            else
            {
                angle = direction.y > 0 ? 90 : 270; // Up or down.
            }
        }
        else
        {
            // Calculate the exact angle toward the player.
            angle = Mathf.Atan2(player.position.y - firePoint.position.y, player.position.x - firePoint.position.x) * Mathf.Rad2Deg;
        }
        for (int i = 0; i < _bulletsPerShot; i++)
        {
            projectiles[i] = miniObjectPooler.GetPooledGameObject().GetComponent<Projectile>();
            projectiles[i].gameObject.SetActive(true);
            projectiles[i].transform.position = firePoint.position;
           // SetBulletSpeed(projectiles[i], i);
            //Refresh bullets after assigning their data; otherwise, the new values take effect only on the next shot.
            projectiles[i].SetprojectileSpeed(_projectileSpeed);


            if (_bulletsPerShot > 1)
            {
                if (i == 0)
                {
                    //Do not offset the first bullet's angle.
                }
                else if (i % 2 != 0) //For odd indices, apply a positive angle offset.
                {
                    multipleBulletsAngleOffset = Mathf.Abs(multipleBulletsAngleOffset);
                    multipleBulletsAngleOffset += _multipleBulletAngleOffset;
                }
                else //For even indices, apply a negative angle offset.
                {
                    multipleBulletsAngleOffset *= -1;
                }

            }

            projectiles[i].SetRotation(new Vector3(0,0,angle + multipleBulletsAngleOffset));
            projectiles[i].RefreshVelocity();
        }

        return projectiles;
    }



    #region Pool methods
    public PoolCreator CreatePoolObject()
    {
        PoolCreator bullet = Instantiate(enemyData.projectile.GetComponent<PoolCreator>());
        bullet.SetPool(bulletPool);
        return bullet;
    }

    public void OnGet(PoolCreator obj)
    {
        obj.gameObject.SetActive(true);
        //SetBulletSpeed(obj.GetComponent<Projectile>(), 0);
    }

    public void OnRelease(PoolCreator obj)
    {
        obj.gameObject.SetActive(false);
    }

    public void OnDestroyPoolObject(PoolCreator obj) //Named OnDestroyBullet to avoid Unity's OnDestroy lifecycle callback.
    {
        Destroy(obj.gameObject);
    }


    #endregion

}
