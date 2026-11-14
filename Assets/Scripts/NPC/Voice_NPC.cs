using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Select NPC voice duration based on dialogue length.
//Count characters and choose an audio duration that matches.

public class Voice_NPC : MonoBehaviour
{
    [SerializeField] private MMF_Player voiceFeedbacks;

    [Tooltip("esse vetor de audios espera que os audios sejam maiores a medida que o tu avança no vetor")]
    [SerializeField] private AudioClip[] audioClips;
    [Tooltip("aqui diz o tamanho da string para cada posição do vetor. tem que ter o mesmo tamanho do vetor audioClips")]
    [SerializeField] private int[] lineSizes;
    private MMF_Sound mmf_sound;

    private void Awake()
    {
        mmf_sound = voiceFeedbacks.GetFeedbackOfType<MMF_Sound>();
    }


    public void PlayVoice(string line)
    {
        int index = 0;
        for (; index < lineSizes.Length; index++)
        {
            if (lineSizes[index] > line.Length)
            {
                break;
            }
        }

        mmf_sound.Sfx = audioClips[index];
        voiceFeedbacks.PlayFeedbacks();

    }
}
