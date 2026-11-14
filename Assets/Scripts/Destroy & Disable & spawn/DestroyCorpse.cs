using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyCorpse : Destroy
{
    [SerializeField] private bool useLifeTimeData = true;
    [SerializeField] private DestroyLifeTimeData lifeTimeData;
    //[SerializeField] private RunData runData;


    protected override void OnEnable()
    {

        base.OnEnable();

        if(useLifeTimeData)
        {
            if(runData != null )
            {
                lifeTime = lifeTimeData.corpseLifeTime - runData.currentWaveNumber*2;
                maxLifeTime = lifeTimeData.corpseMaxLifeTime - runData.currentWaveNumber * 2;
            }
            else
            {
                Debug.Log("Objeto sem o RunData: " + name);
                lifeTime = 10;
            }

            if (lifeTime < 5)
            {
                lifeTime = 5;
            }

            if(maxLifeTime < 10)
            {
                maxLifeTime = 10;
            }

        }

        //Defensive check.
        if(lifeTime > maxLifeTime)
        {
            Debug.LogWarning("LifeTime é menor que max life time em: " + gameObject.name);
            maxLifeTime = lifeTime + 1;
        }

    }

}
