using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>
///  Return bullets to their pool when their lifetime expires.
///HandleBulletImpact handles collisions; this component also spawns death effects.
/// </summary>
public class BulletDeathHandler : MonoBehaviour
{
    //TODO: Add a settings toggle for bullet effects.
    [Tooltip("leva em considera��o a probabilidade de particulas do SettingsData. basicamente efetua o calculo de probabilidade" +
        "duas vezes. na primeira � a chance colocada no GunData e a outra � a que � colocada nos SettingsData." +
        "Pretendo usar isso em armas que atiram muitor projeteis como a lan�a chamas. que dai o player pode desligar a particula ")]
    [SerializeField] bool spawnImpactWithParticleProbability = false;

    [Space(25)]
    [Tooltip("Se falso, n�o spawna nada")]
    [SerializeField] private bool spawnDeathFx = true;
    [Tooltip("Se for pro player tem que deixar setado como true. Esse bool s� vai ser usado pra quando nao � o uma bala do player instanciando esses efeitos")]
    [SerializeField] private bool usePool_fromPlayer = true;
    [SerializeField] private GameObject deathFx_withoutPool;
    [Space(25)]
    [SerializeField] private SettingsData settingsData;
    [Tooltip("Se esta usando Varia Bullet tem que marcar essa op��o para que o varia possa adminitrar o destroy/disable da bala corretamente")]
    [SerializeField] private bool usesVaria = false;

    [Tooltip("Se estiver sendo usado pelo player, n�o h� necessidade de colocar a gunData aqui")]
    [SerializeField] private GunData gunData;
    
    private float timer = 0;
    private Rigidbody2D rb;
    private MMMiniPoolableObject miniPoolableObject;
    private BulletDeathFxPooler bulletDeathFxPooler;
    /// <summary>
    /// Without the shared player death-effect pool, create one reusable inactive effect
    /// per bullet instance.
    /// </summary>
    private GameObject pooledDeathFx;
    private bool spawnedEffect = false;

    private RepoolVariaBullet repoolVariaBullet;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if(usesVaria) repoolVariaBullet = GetComponent<RepoolVariaBullet>();

        if(usePool_fromPlayer == false && gunData == null)
        {
            Debug.LogWarning("Essa bala n�o � do player e n�o tem a gunData setada. Considere colocar a gunData pelo inspector " + gameObject.name);
            return;
        }
        

        miniPoolableObject = GetComponent<MMMiniPoolableObject>();
        //BulletDeathFxPooler = FindObjectOfType<BulletDeathFxPooler>();

        if(usePool_fromPlayer == false && deathFx_withoutPool == false)
        {
            Debug.LogError("Nao vai usar a pool e nem colocou o objeto a ser instanciado! " + gameObject.name);
        }
        else if(usePool_fromPlayer == false)
        {
            pooledDeathFx = Instantiate(deathFx_withoutPool, transform);
            pooledDeathFx.transform.localPosition = Vector3.zero;
            pooledDeathFx.SetActive(false);
        }

    }

    

    private void OnEnable()
    {
        if(gunData == null) gunData = (GunData)GetComponent<BulletStats>().GetGunData();
        spawnedEffect = false;
        timer = 0;

        //Parent the effect on enable to keep the hierarchy organized.
        if (usePool_fromPlayer == false)
        {
            pooledDeathFx.transform.parent = transform;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if(timer > gunData.bulletLifeTime)
        {
            BulletKilledByTime();
        }

        //Varia bullets require more control because their patterns can intentionally move slowly.
        if (!usesVaria)
        {
            if (rb.linearVelocity.magnitude < gunData.bulletSpeed / 20)
            {
                BulletKilledByTime();
                //Remove bullets whose speed falls below the threshold.
            }
        }


    }

    public void BulletKilledByTime()
    {
        //Debug.LogWarning("Destroy bullet por tempo");
        bool spawnDeathFX = CheckSpawnDeathEffect();

        if(spawnDeathFX)
        {
            spawnedEffect = true;

            if(usePool_fromPlayer)
            {
                GameObject bulletDeath;
                bulletDeath = bulletDeathFxPooler.GetPooler(gunData.bulletDeathFX.name).GetPooledGameObject();
                bulletDeath.transform.parent = null;
                bulletDeath.transform.position = transform.position;
                bulletDeath.SetActive(true);
            }
            else
            {
                pooledDeathFx.transform.parent = null;
                pooledDeathFx.transform.position = transform.position;
                pooledDeathFx.SetActive(true);
            }

        }

        RePool();
    }

    public void BulletKilledByCollision()
    {
        //Debug.LogWarning("Destroy bullet por collision");
        bool spawnDeathFX = CheckSpawnDeathEffect();

        if (spawnDeathFX)
        {
            spawnedEffect = true;
            if (usePool_fromPlayer)
            {
                //Supply the GunData that fired this bullet.
                GameObject bulletDeath = bulletDeathFxPooler.GetPooler(gunData.bulletRicochetFx.name).GetPooledGameObject();
                bulletDeath.transform.parent = null;
                bulletDeath.transform.position = transform.position;
                bulletDeath.SetActive(true);
            }
            else
            {
                pooledDeathFx.transform.parent = null;
                pooledDeathFx.transform.position = transform.position;
                pooledDeathFx.SetActive(true);
            }

        }

        RePool();
    }

    /// <summary>
    /// Return to the pool.
    /// </summary>
    private void RePool()
    {
        if (!usesVaria)
        {
            //This MMF method deactivates the object despite its Destroy name.
            miniPoolableObject.Destroy();
        }
        else
        {
            repoolVariaBullet.Repoolbullet();
        }
    }

    private bool CheckSpawnDeathEffect()
    {
        //Do not spawn an effect.
        if(spawnDeathFx == false) return false;
        //The effect already spawned.
        if (spawnedEffect) return false;

        if (Random.Range(0f, 1f) > gunData.deathFxSpawnProbability)
        {
            return false; //The weapon's effect-spawn probability check failed.
        }

        if (spawnImpactWithParticleProbability) //Apply the SettingsData chance only after the weapon allows spawning.
        {
            if (Random.Range(0f, 1f) > settingsData.GetParticles()) return false;
        }

        return true;
    }

    /// <summary>
    /// Update the owner when another weapon reuses this bullet type.
    /// </summary>
    public void SetGunData(GunData gunData, BulletDeathFxPooler bulletDeathFxPooler)
    {
        this.gunData = gunData;
        //Refresh deathFxPooler if required.
        if(this.bulletDeathFxPooler == null) this.bulletDeathFxPooler = bulletDeathFxPooler;
    }
}
