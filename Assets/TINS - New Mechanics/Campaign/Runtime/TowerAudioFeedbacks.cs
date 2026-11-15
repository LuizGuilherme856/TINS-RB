using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using GraveyardShift.Gameplay;

/// <summary>Triggers the scene-authored MMF audio players for tower events.</summary>
public sealed class TowerAudioFeedbacks : MonoBehaviour
{
    private static TowerAudioFeedbacks instance;
    public static TowerAudioFeedbacks Instance
    {
        get
        {
            if (instance == null)
                instance = FindFirstObjectByType<TowerAudioFeedbacks>();
            return instance;
        }
    }

    [Header("Scene MMF Players")]
    [SerializeField] private MMF_Player towerLightGrowthPlayer;
    [SerializeField] private MMF_Player beamRechargePlayer;
    [SerializeField] private MMF_Player beamAttackPlayer;

    private MMF_Player skillCheckErrorPlayer;
    private MMF_Player noChargesPlayer;
    private MMF_Player crystalPickupPlayer;
    private AudioSource crystalDeliverySource;
    private AudioClip crystalDeliveryClip;
    private SkillCheckBar observedSkillCheck;
    private float nextSkillCheckSearchTime;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        skillCheckErrorPlayer = CreateAudioPlayer("MMF - Skill Check Error Audio", "skill check error sound");
        noChargesPlayer = CreateAudioPlayer("MMF - No Charges Audio", "no charges feedback");
        crystalPickupPlayer = CreateAudioPlayer("MMF - Crystal Pickup Audio", "cristal pick up sound");
        crystalDeliveryClip = Resources.Load<AudioClip>("Audio/cristal pick up sound");
        crystalDeliverySource = gameObject.AddComponent<AudioSource>();
        crystalDeliverySource.playOnAwake = false;
        crystalDeliverySource.spatialBlend = 0f;
        crystalDeliverySource.volume = 0.65f;
    }

    private void OnEnable()
    {
        if (instance == null)
            instance = this;
        BindSkillCheck();
    }

    private void Update()
    {
        if (observedSkillCheck == null && Time.unscaledTime >= nextSkillCheckSearchTime)
        {
            nextSkillCheckSearchTime = Time.unscaledTime + 0.5f;
            BindSkillCheck();
        }
    }

    private void OnDisable()
    {
        if (observedSkillCheck != null)
            observedSkillCheck.ResultResolved -= HandleSkillCheckResult;
        observedSkillCheck = null;
    }

    public void PlayTowerLightGrowth() => Play(towerLightGrowthPlayer);
    public void PlayBeamRecharge() => Play(beamRechargePlayer);
    public void PlayBeamAttack() => Play(beamAttackPlayer);
    public void PlayNoCharges() => Play(noChargesPlayer);
    public void PlayCrystalPickup() => Play(crystalPickupPlayer);
    public void PlayCrystalDepositPitch(float pitch)
    {
        if (crystalDeliverySource == null || crystalDeliveryClip == null) return;
        crystalDeliverySource.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
        crystalDeliverySource.PlayOneShot(crystalDeliveryClip);
    }

    private void BindSkillCheck()
    {
        if (observedSkillCheck != null)
            observedSkillCheck.ResultResolved -= HandleSkillCheckResult;

        observedSkillCheck = null;
        SkillCheckBar[] skillChecks = FindObjectsByType<SkillCheckBar>(FindObjectsSortMode.None);
        if (skillChecks.Length > 0)
        {
            observedSkillCheck = skillChecks[0];
            observedSkillCheck.ResultResolved += HandleSkillCheckResult;
        }
    }

    private void HandleSkillCheckResult(SkillCheckBar.SkillCheckResult result)
    {
        if (!result.hitTarget)
            Play(skillCheckErrorPlayer);
    }

    private MMF_Player CreateAudioPlayer(string playerName, string resourceName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Audio/" + resourceName);
        if (clip == null)
        {
            Debug.LogError($"Audio clip not found at Assets/TINS - New Mechanics/Resources/Audio/{resourceName}.mp3", this);
            return null;
        }

        GameObject playerObject = new GameObject(playerName);
        playerObject.transform.SetParent(transform, false);
        MMF_Player player = playerObject.AddComponent<MMF_Player>();
        player.FeedbacksList = new List<MMF_Feedback>
        {
            new MMF_Sound
            {
                Sfx = clip,
                PlayMethod = MMF_Sound.PlayMethods.OnDemand,
                MinVolume = 1f,
                MaxVolume = 1f,
                SpatialBlend = 0f
            }
        };
        player.Initialization();
        return player;
    }

    private static void Play(MMF_Player player)
    {
        if (player == null)
        {
            Debug.LogError("Tower audio MMF_Player reference is missing.");
            return;
        }

        if (!player.gameObject.activeInHierarchy)
            player.gameObject.SetActive(true);
        if (!player.enabled)
            player.enabled = true;

        player.PlayFeedbacks();
    }

    private void OnDestroy()
    {
        if (observedSkillCheck != null)
            observedSkillCheck.ResultResolved -= HandleSkillCheckResult;
        if (instance == this) instance = null;
    }
}
