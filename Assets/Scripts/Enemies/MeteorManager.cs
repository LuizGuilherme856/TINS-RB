using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores meteors and coordinates their falls.
/// </summary>
public class MeteorManager : MonoBehaviour
{
    [SerializeField] private float delayBetweenSpawn = 0.2f; // Delay between meteor falls.
    [SerializeField] private List<Meteor> meteors; // Meteor list.
    [SerializeField] private float meteorHeight = 10f; // Initial meteor height.
    [SerializeField] private List<Transform> targetPositions; // Destination Y positions.

    [Tooltip("Número do índice do meteoro que vai cair na posição do player. Para nenhum cair no player, coloque um numero negativo")]
    public int meteorToFallOnPlayer = 0; 
    public Transform playerTransform; // Player transform.

    public MMF_Player meteorFallFeedback;

    private List<Vector3> spawnPositions = new(); // Spawn positions.

    // Start is called before the first frame update
    void Start()
    {
        //Spawn each meteor at its target X and the configured height above its target Y.
        foreach (var tPos in targetPositions)
        {
            var spawnPos = new Vector3(tPos.position.x, tPos.position.y + meteorHeight, tPos.position.z);
            spawnPositions.Add(spawnPos);
        }

    }



    /// <summary>
    /// Start meteor falls toward their destinations.
    /// </summary>
    public IEnumerator ActivateMeteorFall_Routine()
    {
        meteorFallFeedback.PlayFeedbacks();
        for (int i = 0; i < meteors.Count; i++)
        {
            //Choose a random position.
            int index = Random.Range(0, targetPositions.Count);
            meteors[i].SetTargetTransform(targetPositions[index]);

            if(meteorToFallOnPlayer == i) //If this is the meteor assigned to target the player,
            {
                //set its destination to the player position.
                var playerPos = new Vector3(playerTransform.position.x, playerTransform.position.y + meteorHeight, playerTransform.position.z);
                meteors[i].transform.position = playerPos;
            }
            else
            {
                meteors[i].transform.position = spawnPositions[index]; 
            }

            meteors[i].StartFalling();
            

            yield return new WaitForSeconds(delayBetweenSpawn);
        }
    }
}
