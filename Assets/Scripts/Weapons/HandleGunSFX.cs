///Controls firing audio.
///Discrete sounds use MMF; continuous sounds use an AudioSource.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class HandleGunSFX : MonoBehaviour
{
    //private Camera mainCamera;
    [SerializeField] private MMF_Player shootingFeedbacks, stopShootingFeedbacks;
    [SerializeField] private AudioSource audioSource;
    private MMF_Sound shooting_mmf_Sound, stopShooting_mmf_Sound;

    private ShootingHandler shootingHandler;
    private GunManager gunManager;
    private GunData gunData;
    //private AudioSource audioSource;

    private bool playContinuousSFX = false;

    private void Awake()
    {
        shootingHandler = GetComponent<ShootingHandler>();
        gunManager = GetComponent<GunManager>();
        GetSoundFeedback();
        // mainCamera = Camera.main;
    }

    private void Start()
    {
        shootingHandler.OnShoot += PlayShotSFX;
    }

    private void Update()
    {
        if (playContinuousSFX)
        {
            if (audioSource.isPlaying == false )
            {
                audioSource.Play();
                SetMMF_SoundClip(gunData.startShootingSFX, shooting_mmf_Sound);
                shootingFeedbacks.PlayFeedbacks();
            }

            if (gunManager.IsShooting() == false)
            {
                StopContinuousShootingSfx();
            }
        }
    }

    /// <summary>
    /// Play the firing-end sound and stop continuous firing audio.
    /// </summary>
    public void StopContinuousShootingSfx()
    {
        SetMMF_SoundClip(gunData.stopShootingSFX, stopShooting_mmf_Sound);
        stopShootingFeedbacks.PlayFeedbacks();
        playContinuousSFX = false;
        audioSource.Stop();
    }

 
    private void PlayShotSFX()
    {
        var soundType = gunData.soundType;
        if (soundType == SoundType.discrete)
        {
            shootingFeedbacks.PlayFeedbacks();
        }
        else
        {
            playContinuousSFX = true;
        }
    }

    private void SetMMF_SoundClip( AudioClip shootingSFX, MMF_Sound mmf_Sound)
    {
        //if (mmf_Sound == null) mmf_Sound = shootingFeedbacks.GetFeedbackOfType<MMF_Sound>();

        mmf_Sound.Sfx = shootingSFX;
        mmf_Sound.MinVolume = gunData.shootingVolume;
        mmf_Sound.MaxVolume = gunData.shootingVolume;
    }
    public void SetGunData(GunData gunData)
    {
        GetSoundFeedback();

        this.gunData = gunData;
        if (gunData.soundType == SoundType.discrete)
        {
            SetMMF_SoundClip(gunData.shootingSFX, shooting_mmf_Sound);
        }
        else
        {
            audioSource.clip = gunData.shootingSFX;
            audioSource.volume = gunData.shootingVolume;
        }
    }

    private void GetSoundFeedback()
    {
        if (shooting_mmf_Sound == null)
        {
            shooting_mmf_Sound = shootingFeedbacks.GetFeedbackOfType<MMF_Sound>();
        }
        else if (stopShooting_mmf_Sound == null)
        {
            stopShooting_mmf_Sound = stopShootingFeedbacks.GetFeedbackOfType<MMF_Sound>();
        }
    }
}
