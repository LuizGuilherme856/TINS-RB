using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource bossMusicAudioSource, musicAudioSource;
    [SerializeField] private PlayerHolder playerHolder;
     private Health playerHealth;
    private float defaultMusicVolume, defaultBossMusicVolume;


    private void Awake()
    {
        playerHealth = playerHolder.GetPlayerHealth();
    }

    private void Start()
    {
        playerHealth.OnZeroHealth += StopAllMusic;


        defaultMusicVolume = musicAudioSource.volume;
        defaultBossMusicVolume= bossMusicAudioSource.volume;

    }

    #region Play & Stop Music

    public void StopAllMusic()
    {
        musicAudioSource.Stop();
        bossMusicAudioSource.Stop();
        musicAudioSource.volume = defaultMusicVolume;
        bossMusicAudioSource.volume = defaultBossMusicVolume;

    }

    public void PlayBossMusic( AudioClip music = null)
    {
        if (playerHealth.IsAlive() == false) return;

        if (music != null)
        {
            bossMusicAudioSource.clip = music;
        }

        //bossMusicAudioSource.Stop();
        bossMusicAudioSource.Play();
        musicAudioSource.Stop();
        bossMusicAudioSource.volume = defaultBossMusicVolume;
    }

    public void PlayDefaultMusic()
    {
        if (playerHealth.IsAlive() == false) return;

        bossMusicAudioSource.Stop();
        bossMusicAudioSource.volume = defaultBossMusicVolume;

        musicAudioSource.volume = defaultMusicVolume;
        if (musicAudioSource.isPlaying == false)
        {
            musicAudioSource.Play();
        }
    }

    #endregion


    #region Fade In & Fade Out

    public void FadeOutMusicVolume(float fadeOutTime)
    {
        if(musicAudioSource.isPlaying == false)
        {
            return;
        }

        StopCoroutine("FadeInMusicCoroutine");
        StartCoroutine(FadeOutMusicCoroutine(fadeOutTime));
    }

    public void FadeInMusicVolume(float fadeInTime)
    {
        if(musicAudioSource.isPlaying == false)
        {
            musicAudioSource.Play();
        }

        StopCoroutine("FadeOutMusicCoroutine");
        StartCoroutine(FadeInMusicCoroutine(fadeInTime));
    }

    public void FadeOutBossMusicVolume(float fadeOutTime)
    {
         if(bossMusicAudioSource.isPlaying == false)
         {
            return;
        }

        StopCoroutine("FadeInBossMusicCoroutine");
        StartCoroutine(FadeOutBossMusicCoroutine(fadeOutTime));
    }

    public void FadeInBossMusicVolume(float fadeInTime)
    {
        if (bossMusicAudioSource.isPlaying == false)
        {
            bossMusicAudioSource.Play();
        }

        StopCoroutine("FadeOutBossMusicCoroutine");
        StartCoroutine(FadeInBossMusicCoroutine(fadeInTime));
    }

    private IEnumerator FadeOutMusicCoroutine(float duration)
    {
        if(musicAudioSource.volume <= 0f)
        {
            yield break; // Do nothing if volume is already zero.
        }

        float startVolume = musicAudioSource.volume;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }
        musicAudioSource.volume = 0f;
    }

    private IEnumerator FadeInMusicCoroutine(float duration)
    {
        float startVolume = musicAudioSource.volume;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            musicAudioSource.volume = Mathf.Lerp(startVolume, defaultMusicVolume, time / duration);
            yield return null;
        }
        musicAudioSource.volume = defaultMusicVolume;
    }

    private IEnumerator FadeOutBossMusicCoroutine(float duration)
    {
        float startVolume = bossMusicAudioSource.volume;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            bossMusicAudioSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }
        bossMusicAudioSource.volume = 0f;
    }

    private IEnumerator FadeInBossMusicCoroutine(float duration)
    {
        float startVolume = bossMusicAudioSource.volume;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            bossMusicAudioSource.volume = Mathf.Lerp(startVolume, defaultBossMusicVolume, time / duration);
            yield return null;
        }
        bossMusicAudioSource.volume = defaultBossMusicVolume;
    }

    #endregion


    public void SetBossMusicVolume(float newVolume, bool force = false)
    {
        if(force == true)
        {
            bossMusicAudioSource.volume = newVolume;
            return;
        }

        if (newVolume > defaultBossMusicVolume)
        {
            newVolume = defaultBossMusicVolume;
            Debug.LogWarning("Volume nao deveria ser maior que o volume default boss music");
        }

        bossMusicAudioSource.volume = newVolume;
    }


    public void SetMusicVolume(float newVolume, float minVolumeDifference = 0.03f)
    {
        if (newVolume > defaultMusicVolume)
        {
            newVolume = defaultMusicVolume;
            Debug.LogWarning("Volume nao deveria ser maior que o volume default de ambience");
        }

        if (Mathf.Abs(musicAudioSource.volume - newVolume) > minVolumeDifference)
        {
            musicAudioSource.volume = newVolume;
        }

    }


    public float GetMusicAudioSourceVolume()
    {
        return musicAudioSource.volume;
    }

    public bool IsMusicPlaying()
    {
        return musicAudioSource.isPlaying;
    }
    public float GetMusicVolume()
    {
        return musicAudioSource.volume;
    }

    public float GetBossMusicVolume()
    {
        return bossMusicAudioSource.volume;
    }
}


