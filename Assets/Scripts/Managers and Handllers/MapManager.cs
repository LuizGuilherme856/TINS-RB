using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    /*      Map start uses coordinates A and map end uses coordinates B.
     *      Apply the same start/end mapping to world-space level coordinates.
     *      -------B
     *      |      |
     *      |      |
     *      A-------
     * */

    [SerializeField] private Transform mapStart, mapEnd;
    [SerializeField] private Transform levelStart, levelEnd;
    [SerializeField] private PlayerHolder playerHolder;
    [SerializeField] Transform playerIcon;

    private Transform playerTransform;


    private void Awake()
    {

        playerTransform = playerHolder.GetPlayerTransform();
        if(playerTransform == null) playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

    }

    private Vector2 CalculateAbsPos() // Return normalized progress: 0 is the start and 1 is the end.
    {
        var posX = (playerTransform.position.x - levelStart.position.x) / (levelEnd.position.x - levelStart.position.x);
        var posY = (playerTransform.position.y - levelStart.position.y) / (levelEnd.position.y - levelStart.position.y);

        return new Vector2(posX, posY);
    }


    private void FixedUpdate()
    {
        var mapCoordX = (1 - CalculateAbsPos().x)* mapStart.localPosition.x + mapEnd.localPosition.x * CalculateAbsPos().x;
        var mapCoordY = (1 - CalculateAbsPos().y) * mapStart.localPosition.y + mapEnd.localPosition.y * CalculateAbsPos().y;
        playerIcon.localPosition = new Vector3(mapCoordX , mapCoordY, 0);
    }


}
