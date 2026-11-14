using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Randomly choose a room variant to spawn.

public class SpawnRoomHere : MonoBehaviour
{
    [SerializeField] GameObject[] rooms;
    [SerializeField] GameObject roomExample; //This child serves only as an example and is destroyed.

    private void Awake()
    {
        Destroy(roomExample);
        Instantiate(rooms[Random.Range(0, rooms.Length)], transform.position, transform.rotation);
    }

}
