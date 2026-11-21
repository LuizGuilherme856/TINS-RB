using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleMeleeSFX : MonoBehaviour
{

    [SerializeField] private MMF_Player mmf_player;
    private MMF_Sound mmf_sound;
    private MeleeAttackHandler meleeAttackHandler;
    private MeleeManager meleeManager;

    private MeleeData meleeData;

    private void Awake()
    {
        meleeAttackHandler = GetComponent<MeleeAttackHandler>();
        meleeManager = GetComponent<MeleeManager>();

        mmf_sound = mmf_player.GetFeedbackOfType<MMF_Sound>();
    }

    private void OnEnable()
    {
        meleeAttackHandler.OnMeleeAttack += PlaySFX;
        meleeManager.OnChangeMeleeWeapon += UpdateMeleeData;

        UpdateMeleeData();
    }

    private void PlaySFX()
    {
        mmf_sound.Sfx = meleeData.GetAttackSfx();
        mmf_sound.MinVolume = meleeData.volume;
        mmf_sound.MaxVolume = meleeData.volume;

        float minPitch = mmf_sound.MinPitch;
        float maxPitch = mmf_sound.MaxPitch;
        if (meleeAttackHandler.IsFinishingSlash)
        {
            mmf_sound.MinPitch *= 0.8f;
            mmf_sound.MaxPitch *= 0.8f;
        }
        mmf_player.PlayFeedbacks();
        mmf_sound.MinPitch = minPitch;
        mmf_sound.MaxPitch = maxPitch;
    }

    private void UpdateMeleeData()
    {
        meleeData = meleeManager.GetMeleeData();
    }
}
