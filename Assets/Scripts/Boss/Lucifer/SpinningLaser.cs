using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpinningLaser : MonoBehaviour
{
    [SerializeField] EnemyHealth minionsHealth;
    [SerializeField] LuciferData luciData; // Assigned in the Editor.
    [SerializeField] Animator laserAnimator; // Assigned in the Editor.
    public virtual event Action OnMinionDeath;
    private void Awake()
    {
        try
        {
            //Pass true to include inactive objects.
            minionsHealth = GetComponentInChildren<EnemyHealth>(true);
            minionsHealth.OnZeroHealth += MinionDeath;
        }
        catch (System.Exception)
        {

            Debug.LogWarning("Spinning Laser não tem Enemy Health");
        }
       
    }

    private void OnEnable()
    {
        Reset();
        //StartCoroutine(StartingLaser());
    }



    void Update()
    {
        
    }

    public IEnumerator StartingLaser()
    {
        laserAnimator.SetTrigger("start");
        yield return new WaitForSeconds(luciData.delayBeforeLaserS2);
        laserAnimator.SetTrigger("damage");
    }

    public void MinionDeath()
    {
        OnMinionDeath?.Invoke();
        this.gameObject.SetActive(false);
    }

    public void Reset()
    {
        minionsHealth.ReplenishHealth();
        minionsHealth.gameObject.SetActive(true);
    }
}
