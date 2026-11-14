using System.Collections.Generic;
using UnityEngine;

public class BreakLights_EasterEgg : MonoBehaviour
{
    [SerializeField] private List<BreakOnTouch> breakables;
    [SerializeField] private GameObject objectToSpawn;
    private int currentIndex = 0;

    private void Start()
    {
        foreach (var breakable in breakables)
        {
            breakable.OnBreak += () => OnBreakable(breakable);
        }
    }

    private void OnBreakable(BreakOnTouch broken)
    {
        if (breakables[currentIndex] == broken)
        {
            currentIndex++;
            if (currentIndex >= breakables.Count)
            {
                Instantiate(objectToSpawn, transform.position, Quaternion.identity);
            }
        }
        else
        {
            // Reset when broken out of order.
            currentIndex = 0;
        }
    }
}