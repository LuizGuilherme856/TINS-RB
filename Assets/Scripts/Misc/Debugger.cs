///Display debug information onscreen.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//using static UnityEditor.Rendering.CameraUI;

public class Debugger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textBox, consoleBox;
    private GunManager gunManager;

    static string myLog = "";
    private string output;
    private string stack;

    private void Awake()
    {
        gunManager = FindFirstObjectByType<GunManager>();
    }

    void Update()
    {
        //textBox.text = gunManager.GetEquipedGun().GetName(Language.english);
    }

    void OnEnable()
    {
        Application.logMessageReceived += Log;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }

    //Capture console output.
    public void Log(string logString, string stackTrace, LogType type)
    {
        output = logString;
        stack = stackTrace;
        myLog = output + "\n" + myLog;
        if (myLog.Length > 5000)
        {
            myLog = myLog.Substring(0, 4000);
        }

        consoleBox.text = myLog;
    }
}
