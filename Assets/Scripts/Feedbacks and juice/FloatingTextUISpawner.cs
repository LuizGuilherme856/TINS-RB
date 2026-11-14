using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class FloatingTextUISpawner : MonoBehaviour
{
    //[SerializeField] private Transform spawnTransform;
    [SerializeField] private RectTransform referenceTransform;
    [SerializeField] private MMMiniObjectPooler pooler;
    [SerializeField] private Vector2 speed, xRandomInterval, yRandomInterval;
    [SerializeField] private float minTextSize, maxTextSize;

    //[SerializeField] private float minTimeBetween2Spawns = 0.03f;
    [SerializeField] PointData pointData;

    private float timer = 0;

    private void Update()
    {
        timer += Time.deltaTime;
    }

    public void Spawn(int points)
    {
        //if (timer < minTimeBetween2Spawns) return;

        var obj = pooler.GetPooledGameObject();

        obj.transform.localScale = new Vector3(minTextSize, minTextSize, minTextSize);


        if(points + minTextSize > maxTextSize || points < 0)
        {
            obj.transform.localScale = new Vector3(maxTextSize, maxTextSize, maxTextSize);
        }
        else if(points > 0)
        {
            obj.transform.localScale = new Vector3(minTextSize + points, minTextSize + points, minTextSize);
        }
        else
        {
            obj.transform.localScale = referenceTransform.localScale;
        }


        var popUpText = obj.gameObject.GetComponent<PopUpTextHandler>();

        float xSpeed = speed.x;
        float ySpeed = speed.y;

        float xRandom = Random.Range(xRandomInterval.x, xRandomInterval.y);
        float yRandom = Random.Range(yRandomInterval.x, yRandomInterval.y);

        xSpeed += xRandom;
        ySpeed += yRandom;

        if(points <= 5 && points > 0)
        {
            ySpeed /= 3;
        }

        if (points > 0) 
        {
            popUpText.SetSpeed(xSpeed, ySpeed);
        }
        else
        {
            popUpText.SetSpeed(xSpeed, -ySpeed);
        }

        obj.gameObject.GetComponent<TextMeshProUGUI>().text = points.ToString();

        obj.transform.position = referenceTransform.position;
        obj.SetActive(true);

        timer = 0;
    }


}
