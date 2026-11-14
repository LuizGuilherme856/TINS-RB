using UnityEngine;
[RequireComponent(typeof(UpdateBulletGunData))]
[RequireComponent(typeof(HandleBulletImpact))]
[RequireComponent(typeof(BulletDeathHandler))]
/// <summary>
/// Stores bullet data such as damage.
/// Enemy health handlers read these values when applying hits.
/// </summary>
public class BulletStats : DamageObjectStats
{
    [field:SerializeField] private RuneInventory runeUpgradeManager;
    [SerializeField] private bool isEnemyBullet = false;
    //[SerializeField] private bool spawnBulletImpact = true;

    //[SerializeField] private bool considerTriggerCollision = false;
    [SerializeField] private bool destroyOnTouchingEnemy = true;

    //[SerializeField] private GunData gunData;
    [SerializeField] private StatsCalculator statsCalculator;
    private GunData gunData;
    private float timeAlive = 0;
    private HandleBulletImpact handleBulletImpact;
    private int collisionCounter;
    private BulletDeathHandler bulletDeathHandler;

    private float damageMultiplier_firstBullet = 1;

    private void Awake()
    {
        gunData = (GunData)weaponData;
        handleBulletImpact = GetComponent<HandleBulletImpact>();
        bulletDeathHandler = GetComponent<BulletDeathHandler>();

        if (GetComponent<ReturnToPoolOnCollision>())
        {
            Debug.LogError($"Esse objeto {gameObject.name} já possui o script BulletStats portanto NAO DEVERIA ter um script de ReturnToPoolOnCollision");
            Destroy(GetComponent<ReturnToPoolOnCollision>());
        }
    }

    private void OnEnable()
    {
        collisionCounter = 0;// Reset state when reusing a pooled object.
        timeAlive = 0;
    }
    private void Update()
    {
        timeAlive += Time.deltaTime;
    }

    public override (float damage, CriticalDamage critical) GetDamage()
    {
        float baseDamage = gunData.GetDamage() * damageMultiplier_powerUp * damageMultiplier_firstBullet;

        //Calculate damage falloff with distance.
        baseDamage = baseDamage - baseDamage * timeAlive * gunData.damageFalloffMultiplier;
        if (baseDamage < gunData.minDamageFalloff) baseDamage = gunData.minDamageFalloff;

        if (isEnemyBullet) return (baseDamage, CriticalDamage.no);

        return Damage2.GetDamageAndCritical(baseDamage, gunData, statsCalculator);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        collisionCounter++;
        

        if (collision.gameObject.CompareTag("Enemy") && destroyOnTouchingEnemy)
        {
            bulletDeathHandler.BulletKilledByCollision();
            return;
        }
        else if (collisionCounter >= gunData.maxCollisions)
        {
            bulletDeathHandler.BulletKilledByCollision();
        }
        else if( gunData.bulletRicochetFx != null)
        {
            handleBulletImpact.SpawnBulletImpact(transform.position, collision);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isEnemyBullet == false) return;

        if (collision.CompareTag("Melee"))
        {
            GetComponent<HandleBulletImpact>().SpawnBulletImpact(transform.position);
        }
    }


    public GunData GetGunData()
    {
        return (GunData)weaponData;
    }

    /// <summary>
    /// Update the owner when another weapon reuses this bullet type.
    /// </summary>
    public void SetGunData( GunData gunData) 
    {
        //Debug.Log($"coloquei {gunData.name} no {gameObject.name}");
        this.gunData = gunData;
        weaponData = gunData;
    }

    public void SetDamageMultiplier_FirstBullet(float multiplier)
    {
        damageMultiplier_firstBullet = multiplier;
    }

}
