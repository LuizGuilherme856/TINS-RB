using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldPriestBossManager : MonoBehaviour
{
    public GameObject[] objectsToRotate;
    public float rotationSpeed = 10f;
    public float offSet = 90f;
    float timer = 0;


    private void Update()
    {
        timer += Time.deltaTime;
        int i = 0;
        foreach (GameObject obj in objectsToRotate)
        {
            obj.transform.rotation = Quaternion.Euler(0, 0, timer * rotationSpeed + i*offSet);
            //obj.transform.Rotate(0, 0,  i*offSet + rotationSpeed * Time.deltaTime );
            i++;
        }
    }

    public void EnableShield()
    {
        foreach (GameObject obj in objectsToRotate)
        {
            obj.GetComponent<ShieldPriestBoss>().SetAlive();
        }
    }
}
