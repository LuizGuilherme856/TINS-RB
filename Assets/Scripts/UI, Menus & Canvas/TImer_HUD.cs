using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TImer_HUD : MonoBehaviour
{
    [SerializeField] private bool showTimer = true;
    [SerializeField] private RunData runData;
    [SerializeField] private float updateRate = 1f;

    private TextMeshProUGUI timer_TMP;
    private void Awake()
    {
        timer_TMP = GetComponent<TextMeshProUGUI>();
    }
    void Start()
    {
        timer_TMP.text = MMTime.FloatToTimeString(0);

        if (showTimer == false)
        {
            timer_TMP.text = "";
            return;
        }
        InvokeRepeating(nameof(UpdateTimer), 1f, updateRate);

        ResetTimer();
        Invoke(nameof(ResetTimer), 0.95f);
    }

    private void ResetTimer()
    {
        //Reset at run start.
        if (runData.timeElapsed < 5)
        {
            timer_TMP.text = MMTime.FloatToTimeString(0);
            runData.timeElapsed = 0;
        }
    }

    private void UpdateTimer()
    {
        timer_TMP.text = MMTime.FloatToTimeString(runData.timeElapsed);
    }
}
