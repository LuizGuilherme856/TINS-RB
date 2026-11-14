using MoreMountains.Feedbacks;

using UnityEngine;

public class SetPlayerHitSoundFx : MonoBehaviour
{
    [SerializeField] private RunData runData;
    private MMF_Player hitFeedback;
    private MMF_Sound mmf_sound;
    private void Awake()
    {
        hitFeedback = GetComponent<MMF_Player>();
        mmf_sound = hitFeedback.GetFeedbackOfType<MMF_Sound>();
    }
    
    void Start()
    {
        mmf_sound.Sfx = runData.currentCharacter.hitSfx;
    }


}



