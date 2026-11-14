using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Controls the EmissiveShot rune's emitted projectiles.
/// </summary>
public class EmissiveShot : MonoBehaviour
{
    private Rigidbody2D mainBulletRigidBody;
    private MMMiniObjectPooler miniObjectPooler;
    private float delay;
    public GunData gunData { get; set; }
    public float damageMultiplier {  get; set; }
    public bool spawnBullets = false; //Notify the caller that regular bullets should not spawn.

    private void Awake()
    {
        mainBulletRigidBody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        
        if(spawnBullets) StartCoroutine(nameof(SpawnBullets));

    }

    private IEnumerator SpawnBullets()
    {
        yield return new WaitForSeconds(delay);

        if (gameObject.activeSelf == false) yield break;

        //Spawn two bullets at +90 and -90 degrees.
        int bulletsCount = 2;
        Projectile[] projectiles = new Projectile[bulletsCount];

        for (int i = 0; i < bulletsCount; i++)
        {
            projectiles[i] = miniObjectPooler.GetPooledGameObject().GetComponent<Projectile>();
            projectiles[i].gameObject.SetActive(true);
            //projectiles[i].gameObject.GetComponent<BulletStats>().SetGunData(gunData); //Used when a different weapon reuses this bullet type.
            projectiles[i].gameObject.GetComponent<BulletStats>().SetDamageMultiplier_PowerUp(damageMultiplier);
            projectiles[i].transform.position = transform.position;
            projectiles[i].transform.parent = null;

        }

        projectiles[0].GetComponent<Rigidbody2D>().linearVelocity = new Vector3(-mainBulletRigidBody.linearVelocity.y, mainBulletRigidBody.linearVelocity.x, 0);
        projectiles[1].GetComponent<Rigidbody2D>().linearVelocity = new Vector3(mainBulletRigidBody.linearVelocity.y, -mainBulletRigidBody.linearVelocity.x, 0);

        StartCoroutine(SpawnBullets());
    }

    public void SetDelay(float delay) { this.delay = delay; }
    public void SetObjectPooler(MMMiniObjectPooler miniObjectPooler)
    {
        this.miniObjectPooler = miniObjectPooler;
    }

}
