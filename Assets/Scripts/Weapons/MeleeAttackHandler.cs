///Manage melee projectile pooling and configuration.
using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using System;


public class MeleeAttackHandler : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private Impulse playerImpulse;
    [SerializeField] private ControlCameraShake controlCameraShake;
    [SerializeField] private MMMiniObjectPooler miniObjectPooler;
    [SerializeField] private Rigidbody2D playerRb2D;
    [SerializeField] private Shield_PowerUp shield_PowerUp;

    [Header("Aim")]
    [SerializeField] private Transform firepoint;
    [SerializeField] private Aim aim;

    [Header("Feedbacks")]
    [SerializeField] private MMF_Player mmfPlayer_FreezeTime;
    [SerializeField] private MMF_Player mmfPlayer_attack;
    [SerializeField] private TrailRenderer meleeTrail;
    [SerializeField] private MMF_Player mmfPlayer_cantAttack;

    [Space(10)]
    [SerializeField] private SettingsData settingsData;

    private MMF_FreezeFrame freezeFrameFeedback;
    private MMF_LensDistortion_URP lensDistortionFeedback;

    private GameDataAndSettings gameData;

    public event Action OnMeleeAttack;
    //private float playerSpeed;
    
    //[SerializeField] private Rigidbody2D playerRigidbody; //Add player velocity to projectile velocity.
    //private IObjectPool<PoolCreator> meleeObjectPool;
    private MeleeData meleeData;
    private MeleeStats currentMeleeStats;

    private float aimAngle; //Required by OnGet.
    private Vector3 originalMeleeLocalScale; //Apply configurable local scale rather than hard-coding it.


    private float damageMultiplier = 1; //Used by the instant-kill power-up.
    private int numberOfInstaKillsActive = 0; //Current number of instant-kill effects.
    private void Awake()
    {
        gameData = FindFirstObjectByType<GameDataAndSettings>();

        freezeFrameFeedback = mmfPlayer_FreezeTime.GetFeedbackOfType<MMF_FreezeFrame>(); ;
        lensDistortionFeedback = mmfPlayer_attack.GetFeedbackOfType<MMF_LensDistortion_URP>();
        //Resolve controlCameraShake here.
        //Find a manager with ControlCameraShake.
        try
        {
            GameObject.FindGameObjectWithTag("CameraRelated").TryGetComponent<ControlCameraShake>(out controlCameraShake); 
        }
        catch (Exception)
        {
            Debug.Log("N�o encontrou o ControlCameraShake nessa scene, lembre que ele deve estar com o tag 'CameraRelated'");
            //throw;
        }

        
    }

    public Rigidbody2D PlayerBody => playerRb2D;
    public float DamageMultiplier => damageMultiplier;
    public bool IsFinishingSlash { get; private set; }

    public void PlaySlashSwing(bool finisher)
    {
        IsFinishingSlash = finisher;
        OnMeleeAttack?.Invoke();
        IsFinishingSlash = false;
    }

    public void PlaySlashImpact(float duration, float angle, float shake)
    {
        if (controlCameraShake != null) controlCameraShake.TriggerCameraShake(angle, shake);
        freezeFrameFeedback.FreezeFrameDuration = duration;
        mmfPlayer_FreezeTime.PlayFeedbacks();
    }

    public void InstantiateMeleeAttack(float aimAngle)
    {
        Projectile newMelee = miniObjectPooler.GetPooledGameObject().GetComponent<Projectile>();
        //Debug.Log(newMelee.gameObject.name);
        newMelee.gameObject.SetActive(true);
        
        newMelee.gameObject.GetComponent<MeleeStats>().SetDamageMultiplier_PowerUp(damageMultiplier);

        this.aimAngle = aimAngle;
        if (meleeData.isPlayerChild)
        {
            newMelee.transform.SetParent(firepoint);
        }
        if (meleeData.playerAction == PlayerAction.dash)
        {
            newMelee.transform.position = this.gameObject.transform.position;// + new Vector3(meleeData.distOffset *Mathf.Cos(aimAngle), 0,0);
            newMelee.transform.rotation = new Quaternion(newMelee.transform.rotation.x, newMelee.transform.rotation.y, aimAngle, newMelee.transform.rotation.w);

        }
        else 
        {
            newMelee.transform.position = firepoint.position;// + new Vector3(meleeData.distOffset *Mathf.Cos(aimAngle), 0,0);
            newMelee.transform.rotation = aim.transform.rotation;
        }
       
        SetMeleeProjectileSpeed(newMelee.GetComponent<Projectile>());
        playerImpulse.TriggerImpulse(aimAngle, meleeData.playerImpulse, false);

        OnMeleeAttack?.Invoke();

        HandleFeedbacks(aimAngle, newMelee);

        if(meleeData.invulnerableTimer> 0) //Some attacks, such as dash, grant invulnerability.
        {
            shield_PowerUp.ActivateShield(meleeData.invulnerableTimer);
            playerHealth.SetInvulnerableTime(meleeData.invulnerableTimer);
        }

    }



    private void HandleFeedbacks(float aimAngle, Projectile newMelee)
    {
        //feedbacks
        controlCameraShake.TriggerCameraShake(aimAngle, meleeData.camShakeMagnitude);
        lensDistortionFeedback.RemapIntensityOne = meleeData.lensDistortion *settingsData.GetLensDistortion();
        mmfPlayer_attack.PlayFeedbacks();
        //Keep the listener reference so it can be removed without leaving duplicate callbacks.
        currentMeleeStats = newMelee.gameObject.GetComponent<MeleeStats>();
        currentMeleeStats.OnHitEnemy += HandleOnEnemyHitFeedback;

        if (meleeData.hasTrail)
        {
            meleeTrail.gameObject.SetActive(true);
            meleeTrail.time = meleeData.trailTime;
        }
    }

    private void HandleOnEnemyHitFeedback()
    {
        freezeFrameFeedback.FreezeFrameDuration = meleeData.freezeFrameTime;
        mmfPlayer_FreezeTime.PlayFeedbacks();
        currentMeleeStats.OnHitEnemy -= HandleOnEnemyHitFeedback;
    }

    public void UpdateMeleeData( MeleeData meleeData)
    {
        this.meleeData = meleeData;
        try
        {
            originalMeleeLocalScale = meleeData.meleeGameObject.transform.localScale;

        }
        catch (Exception)
        {
            Debug.Log("nenhuma meleedata presente. Segue o baile");
        }
    }

    public void PlayCantAttackFeedback()
    {
        mmfPlayer_cantAttack.PlayFeedbacks();
    }

    //Similar to ShootingHandler, but kept separate because the pools differ.
    //Combining them still required separate method implementations,
    //so this path remains independent for future changes.
    private void SetMeleeProjectileSpeed(Projectile newProjectile)
    {
        if (meleeData.isPlayerChild)
        {
            //return;
        }

        if (meleeData.playerAction != PlayerAction.dash)
        {
            if (aim.GetIsAimingRight())
            {
                newProjectile.SetDirection(1);
                newProjectile.SetRotation(aim.GetAimTransform().rotation.eulerAngles);
                newProjectile.gameObject.GetComponent<Transform>().localScale = new Vector3(originalMeleeLocalScale.x, originalMeleeLocalScale.y, 1);
            }
            else
            {
                newProjectile.SetDirection(-1);
                Vector3 newAngle = new Vector3(0, 0, -(aimAngle + 180));
                newProjectile.SetRotation(newAngle);
                newProjectile.gameObject.GetComponent<Transform>().eulerAngles = -newAngle;
                newProjectile.gameObject.GetComponent<Transform>().localScale = new Vector3(-originalMeleeLocalScale.x, originalMeleeLocalScale.y, 1);
            }
        }
        else 
        {
            newProjectile.SetDirection(-1);
            Vector3 newAngle = new Vector3(0, 0, -(aimAngle + 180));
            newProjectile.SetRotation(newAngle);
            newProjectile.gameObject.GetComponent<Transform>().eulerAngles = -newAngle;
            newProjectile.gameObject.GetComponent<Transform>().localScale = new Vector3(-originalMeleeLocalScale.x, originalMeleeLocalScale.y, 1);
        }

        //newProjectile.transform.position = firePoint;
        float flySpeed = meleeData.flySpeed;
        if (flySpeed > 0) //Thrown projectile with its own launch velocity.
        {
            newProjectile.SetprojectileSpeed(flySpeed);  
        }
        else //Stationary relative to the player; inherit only player velocity.
        {

            if (meleeData.isPlayerChild == false) newProjectile.SetprojectileSpeed(playerRb2D.linearVelocity.magnitude);

        }

        if (meleeData.isPlayerChild) //Reset local scale only for objects parented to the player.
        {
            newProjectile.gameObject.GetComponent<Transform>().localScale = new Vector3(originalMeleeLocalScale.x, originalMeleeLocalScale.y, 1);
        }

        newProjectile.RefreshVelocity();
    }

    //Switch bullet pools when the equipped weapon changes.
    public void SetObjectPooler(MMMiniObjectPooler miniObjectPooler)
    {
        this.miniObjectPooler = miniObjectPooler;
    }


    public void SetDamageMultiplierDuration(float damageMultiplier, float duration)
    {
        numberOfInstaKillsActive++;
        this.damageMultiplier = damageMultiplier * numberOfInstaKillsActive;
        Invoke(nameof(UpdateDamageMultiplier), duration);
    }

    private void UpdateDamageMultiplier()
    {
        numberOfInstaKillsActive--;
        if(numberOfInstaKillsActive <= 0)
        {
            numberOfInstaKillsActive = 0;//Defensive check.
            damageMultiplier = 1;
        }
        else
        {
            float damageMultiplier_gambiarra = 5; //TODO: Obtain this value from SetDamageMultiplierDuration instead of hard-coding 5.
            damageMultiplier = damageMultiplier_gambiarra * numberOfInstaKillsActive;
        }
    }

}
