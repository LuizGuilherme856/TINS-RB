using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Play music with separate intro, loop, and ending clips.
/// </summary>
public class SimpleAudioManager : MonoBehaviour
{
    [Header("Refer�ncias")]
    [SerializeField] private AudioSource audioSource;

    [Header("Clipes de �udio")]
    [SerializeField] private AudioClip startClip;
    [SerializeField] private AudioClip loopClip;
    [SerializeField] private AudioClip endClip;

    private bool isEnding = false;

    /// <summary>
    /// Start the intro followed by the loop.
    /// </summary>
    public void Play()
    {
        isEnding = false;

        try
        {
            if (audioSource == null)
                throw new System.InvalidOperationException("AudioSource n�o atribu�do ao SimpleAudioManager.");
            if (startClip == null)
                throw new System.InvalidOperationException("StartClip n�o atribu�do ao SimpleAudioManager.");
            if (loopClip == null)
                throw new System.InvalidOperationException("LoopClip n�o atribu�do ao SimpleAudioManager.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro no simple audio manager: " + e.Message);
            return;
        }


        audioSource.loop = false;
        audioSource.clip = startClip;
        audioSource.Play();
        StartCoroutine(PlayLoopAfterStart());
    }


    /// <summary>
    /// Transition from the loop to the ending.
    /// </summary>
    public void PlayEnd()
    {
        if (audioSource == null)
            throw new System.InvalidOperationException("AudioSource n�o atribu�do ao SimpleAudioManager.");
        if (endClip == null)
            throw new System.InvalidOperationException("EndClip n�o atribu�do ao SimpleAudioManager.");

        isEnding = true;
        audioSource.loop = false;
        audioSource.clip = endClip;
        audioSource.Play();
    }

    private IEnumerator PlayLoopAfterStart()
    {
        yield return new WaitUntil(() => !audioSource.isPlaying);

        if (!isEnding)
        {
            audioSource.clip = loopClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    
    /// <summary>
/// Replace the intro AudioClip.
/// </summary>
public void SetStartClip(AudioClip newClip)
{
    startClip = newClip;
}

    /// <summary>
    /// Replace the looping AudioClip.
    /// </summary>
    public void SetLoopClip(AudioClip newClip)
    {
        loopClip = newClip;
    }

    public void SetStartAndLoopClip(AudioClip newClip)
    {
        loopClip = newClip;
        startClip = newClip;
}

    /// <summary>
    /// Replace the ending AudioClip.
    /// </summary>
    public void SetEndClip(AudioClip newClip)
    {
        endClip = newClip;
    }
}
