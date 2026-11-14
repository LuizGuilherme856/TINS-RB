using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IronMaidensManager : MonoBehaviour
{
    public int numberOfKeys;
    public List<SpawnObjectOnInteraction> ironMaidens = new();

    public Action OnTriggerIronACH; //achievment related
    private void Awake()
    {
        if(numberOfKeys > ironMaidens.Count)
        {
            Debug.LogError("O numero de chaves nao deveria ser maior que o numero de IronMaidens");
        }

        //Prevent all Iron Maidens from spawning.
        foreach (var ironM in ironMaidens)
        {
            ironM.canSpawn = false;
            //Achievment related

            ironM.OnInteraction += CheckIronAchievment;
        }

        //Shuffle the list.
        ironMaidens.MMShuffle();
        //Allow only the required number of keys to spawn.
        for (int i = 0; i < numberOfKeys; i++)
        {
            ironMaidens[i].canSpawn = true;
        }


    }

    public void CheckIronAchievment()
    {
        //old code
        bool callAch = false;
        foreach (var ironM in ironMaidens)
        {
            if (!ironM.GetInteractionFinished())
            {
                return;
            }
        }
        callAch = true;


        //Generated draft.
        //bool todasFinalizadas = ironMaidens.All(im => im.GetInteractionFinished());
        if (callAch)
        {
            OnTriggerIronACH?.Invoke();
        }
    }

    public void ResetAchEvents() 
    {
        OnTriggerIronACH = null;
    }

    //Register a listener that verifies interactionFinished on every object.
}
