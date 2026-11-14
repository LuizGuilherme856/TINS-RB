using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GivePlayerPointsOnStart : MonoBehaviour
{
    [SerializeField] private PointManager pointManager;
    [SerializeField] private int points = 10000;
    void Start()
    {
        pointManager.AddPoint(points);
    }


}
