using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugStatistics : MonoBehaviour
{
    [SerializeField] private RunData runData;

    public int enemiesAlive;
    public int pointsInThisRun;
    public int xp;
    public int enemiesKilled;
    public float timeElapsed_currentWave;

    [SerializeField] private TextMeshProUGUI enemiesAliveText;
    [SerializeField] private TextMeshProUGUI pointsInThisRunText;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private TextMeshProUGUI enemiesKilledText;
    [SerializeField] private TextMeshProUGUI timeElapsed_currentWaveText;
    [SerializeField] private TextMeshProUGUI powerUpsCollected;
    [SerializeField] private TextMeshProUGUI console;

    private List<string> errorMessages = new List<string>();

    private void OnEnable()
    {
        UpdateTextMeshProVariables();
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void UpdateTextMeshProVariables()
    {
        enemiesAlive = runData.enemiesAlive;
        pointsInThisRun = runData.pointsInThisRun;
        xp = runData.xp;
        enemiesKilled = runData.enemiesKilled;
        timeElapsed_currentWave = runData.timeElapsed_currentWave;

        enemiesAliveText.text = $"Inimigos vivos: {enemiesAlive}";
        pointsInThisRunText.text = $"Pontos na run: {pointsInThisRun}";
        xpText.text = $"XP: {xp}";
        enemiesKilledText.text = $"Inimigos mortos: {enemiesKilled}";
        timeElapsed_currentWaveText.text = $"Tempo nessa wave: {timeElapsed_currentWave:F2} s";

        powerUpsCollected.text = $"PowerUps coletados: {runData.numberOfPowerUpsCollected}";
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            errorMessages.Add(logString);
            if (errorMessages.Count > 3)
            {
                errorMessages.RemoveAt(0);
            }
            UpdateConsoleText();
        }
    }

    private void UpdateConsoleText()
    {
        console.text = string.Join("\n", errorMessages);
    }
}
