///Camera animation code used for the trailer.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraDistanceTrailer : MonoBehaviour
{
    [SerializeField] private  CinemachineCamera virtualCam;
    [SerializeField] private float distanceOverTime = 0.1f, timeToStartDistance = 4f;

    private float timer = 0;
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if(timer > timeToStartDistance)
        {
            virtualCam.Lens.OrthographicSize += Time.deltaTime*distanceOverTime;  
        }
        
    }
}
