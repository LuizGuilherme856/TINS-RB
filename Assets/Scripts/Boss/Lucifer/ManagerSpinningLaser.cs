using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerSpinningLaser : MonoBehaviour
{
    [SerializeField] private List<SpinningLaser> spinningLasers; // Assigned in the Editor.
    [SerializeField] private GameObject middleCircle; //Assigned in the Editor.
    [SerializeField] protected bool activateMiddleCircle = true;
    [SerializeField] LuciferData luciData; // Assigned in the Editor.

    [SerializeField] private MMF_Player rotateFeel;
    [SerializeField] private int initialLasers = 2;
    [SerializeField] private int currentLasersAmount = 2;

    private void Awake()
    {
        foreach (SpinningLaser laser in spinningLasers)
        {
            laser.OnMinionDeath += UpdateCurrentLasersAmount;
        }
        try
        {
            rotateFeel = GetComponent<MMF_Player>();
        }
        catch (System.Exception)
        {

            Debug.LogWarning("Laser não tem rotation fell");
        }

    }

    private void OnEnable()
    {
        rotateFeel.PlayFeedbacks();
        foreach (SpinningLaser laser in spinningLasers)
        {
            laser.gameObject.SetActive(true);
            laser.StartCoroutine(laser.StartingLaser());
        }
        StartCoroutine(StartingLaser());
    }
    private IEnumerator StartingLaser()
    {
        yield return new WaitForSeconds(luciData.delayBeforeLaserS2);
        if (activateMiddleCircle)
        {
            middleCircle.SetActive(true);
        }

    }


    public void UpdateCurrentLasersAmount()
    {
        currentLasersAmount--;
        if (currentLasersAmount <= 0)
        {
            DeactivateAndReset();
        }
    }

    public void DeactivateAndReset()
    {
        currentLasersAmount = initialLasers;
        middleCircle.SetActive(false);
        this.gameObject.SetActive(false);
    }
}
