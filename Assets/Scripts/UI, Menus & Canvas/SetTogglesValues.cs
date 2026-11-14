using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class SetTogglesValues : MonoBehaviour
{
    [SerializeField] private Toggle fullScreenToggle, postProcessingToggle, vSyncToggle;
    [SerializeField] private GameObject framecapGameObj;

    [SerializeField] private bool setVsync = true;

    private GameDataAndSettings gameData;

    private void Awake()
    {
        gameData = GameObject.Find("Game Session").GetComponent<GameDataAndSettings>();
    }


    private void Start()
    {
        if (gameData.fullScreen)
        {
            fullScreenToggle.isOn = true;
            fullScreenToggle.isOn = true; //Legacy workaround.
        }
        else
        {
            fullScreenToggle.isOn = false;
            fullScreenToggle.isOn = false;
        }

        if (gameData.postProcessing)
        {
            postProcessingToggle.isOn = true;
            postProcessingToggle.isOn = true;
        }
        else
        {
            postProcessingToggle.isOn = false;
            postProcessingToggle.isOn = false;
        }

        if (setVsync == false) return;

        if(gameData.isVsyncOn)
        {
            Debug.Log("Aqui diz que a vsync ta on");
            vSyncToggle.isOn = true;
            vSyncToggle.isOn = true;
        }
        else
        {
            Debug.Log("Aqui diz que a vsync ta off");
            vSyncToggle.isOn = false;
            vSyncToggle.isOn = false;
        }

       // ShowHideFrameCap();
        //vSyncToggle.onValueChanged.AddListener(delegate {ShowHideFrameCap();}); 
    }

    private void ShowHideFrameCap()
    {
        framecapGameObj.SetActive(!vSyncToggle.isOn);
    }
}
