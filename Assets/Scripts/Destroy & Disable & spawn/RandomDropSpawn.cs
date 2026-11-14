using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Spawn a random item at the start of the run.

public class RandomDropSpawn : MonoBehaviour
{
    [SerializeField] [Range(0,100)] private int chanceInterval;
    [SerializeField] private GameObject[] drops;
    [SerializeField] private Transform[] positions;


    private void OnEnable()
    {
        int number = Random.Range(0, chanceInterval);
        if(number < drops.Length)
        {
            var pos = positions[Random.Range(0, positions.Length)];

            Instantiate(drops[number], pos.position, pos.rotation);
        }
    }
}
