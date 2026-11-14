using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObjectRandomPos : MonoBehaviour
{

    [SerializeField] private GameObject[] objectsToSpawn;

    [SerializeField] private Transform[] transforms;

    private List<int> usedIndexes = new List<int>();
    

    private void Start()
    {
        int index;
        usedIndexes.Add(-1); //Provide a non-null fallback.
        foreach (var obj in objectsToSpawn)
        {
            do //Avoid spawning two objects at the same position.
            {
                index = Random.Range(0, transforms.Length);
            }
            while (usedIndexes.Contains(index));

            usedIndexes.Add(index);
            Instantiate(obj, transforms[index].position, transforms[index].rotation);
        }


    }
}
