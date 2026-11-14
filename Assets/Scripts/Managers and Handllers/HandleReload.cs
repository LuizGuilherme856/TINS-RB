using MoreMountains.Feedbacks;
using UnityEngine;

public class HandleReload : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private MMF_Player mmf_player;
    [SerializeField] private RuneInventory runeUpgradeManager;

    private SpriteRenderer spriteRenderer;
    private MMF_Sound mmf_sound;
   // private int shaderID;
    private GunData gunData;
    [SerializeField] private LoadingCircle loadingCircle, loadingCircleMousePos;
    [SerializeField] private StatsCalculator statsCalculator;

    private GunManager gunManager;
    private void Awake()
    {
        gunManager = GetComponent<GunManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        //loadingCircle = GameObject.FindGameObjectWithTag("Canvas").GetComponent<LoadingCircle>();
        mmf_sound = mmf_player.GetFeedbackOfType<MMF_Sound>();
    }

    private void OnEnable()
    {
        gunManager.OnStartReloading += StartReloading;
        gunManager.OnCancelReloading += loadingCircle.StopReloading;
        gunManager.OnCancelReloading += loadingCircleMousePos.StopReloading;
        gunManager.OnFinishReload += FinishReloading;
        gunManager.OnWeaponChange += UpdateGunDataReference_TechBooh;

        gunData = gunManager.GetEquipedGun();
        //shaderID = Shader.PropertyToID("_UVRotateSpeed");
        //spriteRenderer.material.SetFloat(shaderID, 0);
    }

    private void OnDisable()
    {
        if (gunManager == null)
        {
            return;
        }

        gunManager.OnStartReloading -= StartReloading;
        gunManager.OnCancelReloading -= loadingCircle.StopReloading;
        gunManager.OnCancelReloading -= loadingCircleMousePos.StopReloading;
        gunManager.OnFinishReload -= FinishReloading;
        gunManager.OnWeaponChange -= UpdateGunDataReference_TechBooh;
    }

    private void StartReloading()
    {
        //spriteRenderer.material.SetFloat(shaderID, 50);
        float reloadTime = statsCalculator.CalculateNewReloadTime(gunData.reloadTime);
        loadingCircle.SetReloadTime(reloadTime);
        loadingCircleMousePos.SetReloadTime(reloadTime);
        mmf_sound.Sfx = gunData.startReloadingSFX;
        SetVolumeAndPlay();

    }


    private void FinishReloading()
    {
        //spriteRenderer.material.SetFloat(shaderID, 0);
        loadingCircle.StopReloading();
        loadingCircleMousePos.StopReloading();
        mmf_sound.Sfx = gunData.stopReloadingSFX;
        SetVolumeAndPlay();

    }
    private void SetVolumeAndPlay()
    {
        mmf_sound.MinVolume = gunData.reloadVolume;
        mmf_sound.MaxVolume = gunData.reloadVolume;
        mmf_player.PlayFeedbacks();
    }

    private void UpdateGunDataReference_TechBooh()
    {
        gunData = gunManager.GetEquipedGun();
    }
}
