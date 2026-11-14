using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class PlayFeedbacks : MonoBehaviour
{
    [SerializeField] MMF_Player mmfPlayer;
    [SerializeField] private bool playOnEnable = false;
    public float startDelay = 0f; //Minimum delay before feedbacks can play, including immediately after OnEnable.
    [Header("TechBooh")]
    [SerializeField] private bool blockPauseWhilePlaying_TechBooh = false;
    [SerializeField] private float pauseBlockDuration_TechBooh = 1.5f;

    private float timer = 0f;
    private float enabledAtUnscaledTime_TechBooh = float.MinValue;
    private Coroutine pauseBlockCoroutine_TechBooh;
    private bool isBlockingPause_TechBooh = false;
    private static int activePauseBlockers_TechBooh = 0;

    public static bool IsPauseBlockedByFeedback_TechBooh => activePauseBlockers_TechBooh > 0;

    private void Update()
    {
        // TechBooh:
        // UI feedbacks must also work while the game is paused.
        // Options menus commonly run with timeScale = 0; deltaTime
        // would leave the timer stopped and block feedbacks using startDelay.
        timer += Time.unscaledDeltaTime;
    }

    private void OnEnable()
    {
        // TechBooh:
        // Measure startDelay from the latest enable time.
        // Without resetting the timer, a long-lived object could
        // play feedback immediately on re-enable, bypassing the requested delay.
        timer = 0f;
        enabledAtUnscaledTime_TechBooh = Time.unscaledTime;

        if (playOnEnable)
        {
            PlayAllFeedbacks();
        }
    }

    public void PlayAllFeedbacks()
    {
        if (HasReachedStartDelaySinceEnable_TechBooh() == false) return;
        mmfPlayer.PlayFeedbacks();
        StartPauseBlockIfNeeded_TechBooh();
    }

    private void OnDisable()
    {
        StopPauseBlockIfNeeded_TechBooh();
    }

    /// <summary>
    /// TechBooh:
    /// Some feedbacks represent scene transitions. While they play,
    /// block pause input to prevent duplicate menu and transition flows.
    /// Enable this per instance so ordinary feedbacks remain unaffected.
    /// </summary>
    private void StartPauseBlockIfNeeded_TechBooh()
    {
        if (blockPauseWhilePlaying_TechBooh == false)
        {
            return;
        }

        if (pauseBlockCoroutine_TechBooh != null)
        {
            StopCoroutine(pauseBlockCoroutine_TechBooh);
        }

        if (isBlockingPause_TechBooh == false)
        {
            isBlockingPause_TechBooh = true;
            activePauseBlockers_TechBooh++;
        }

        pauseBlockCoroutine_TechBooh = StartCoroutine(ReleasePauseBlock_TechBooh());
    }

    private IEnumerator ReleasePauseBlock_TechBooh()
    {
        float duration = Mathf.Max(0f, pauseBlockDuration_TechBooh);
        yield return new WaitForSecondsRealtime(duration);

        pauseBlockCoroutine_TechBooh = null;
        StopPauseBlockIfNeeded_TechBooh();
    }

    private void StopPauseBlockIfNeeded_TechBooh()
    {
        if (pauseBlockCoroutine_TechBooh != null)
        {
            StopCoroutine(pauseBlockCoroutine_TechBooh);
            pauseBlockCoroutine_TechBooh = null;
        }

        if (isBlockingPause_TechBooh == false)
        {
            return;
        }

        isBlockingPause_TechBooh = false;
        activePauseBlockers_TechBooh = Mathf.Max(0, activePauseBlockers_TechBooh - 1);
    }

    private bool HasReachedStartDelaySinceEnable_TechBooh()
    {
        if (startDelay <= 0f)
        {
            return true;
        }

        if (enabledAtUnscaledTime_TechBooh == float.MinValue)
        {
            enabledAtUnscaledTime_TechBooh = Time.unscaledTime;
        }

        float timeSinceEnable = Time.unscaledTime - enabledAtUnscaledTime_TechBooh;
        return timeSinceEnable >= startDelay;
    }
}
