using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BossEnemy;

public class VariaCicler : MonoBehaviour
{
    [SerializeField]protected List<GameObject> variaSpawners;
    [SerializeField]protected float timeToSwich = 3;
    protected  float currentVariaTimer= 0;
    // Start is called before the first frame update

    private int currentIndex = 0;

    void Start()
    {
        if (variaSpawners.Count > 0)
        {
            ActivateObject(currentIndex);
        }
    }

    void Update()
    {
        currentVariaTimer += Time.deltaTime;
        if (currentVariaTimer >= timeToSwich)
        {
            currentVariaTimer = 0f;
            currentIndex = (currentIndex + 1) % variaSpawners.Count;
            ActivateObject(currentIndex);
        }
    }

    private void ActivateObject(int index)
    {
        for (int i = 0; i < variaSpawners.Count; i++)
        {
            variaSpawners[i].SetActive(i == index);
        }
    }
}
