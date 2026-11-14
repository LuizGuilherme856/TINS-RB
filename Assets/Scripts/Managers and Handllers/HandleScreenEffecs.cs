using System;
using System.Collections.Generic;
using UnityEngine;

public class HandleScreenEffecs : MonoBehaviour
{
    [SerializeField] protected List<ScreenEffect> screenEffects;
    [Serializable]
    public struct ScreenEffect
    {
        public string name;
        public GameObject effectGameObject;
    }




    public void ResetAllEffecs()
    {
        foreach (ScreenEffect effectI in screenEffects)
        {

            effectI.effectGameObject.SetActive(false);

        }

    }

    /// <summary>
    /// Set the selected effect flag by name.
    /// Activate or deactivate that effect.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="targetBool"></param>
    public void SetEffectByName(string name, bool targetBool)
    {
        foreach (ScreenEffect effectI in screenEffects)
        {
            if (name == effectI.name)
            {
                effectI.effectGameObject.SetActive(targetBool);
            }
        }
    }



    public GameObject GetEffectByName(string name)
    {
        foreach (ScreenEffect effectI in screenEffects)
        {
            if (name == effectI.name)
            {
                return effectI.effectGameObject;
            }
        }
        return null;
    }


    /// <summary>
    /// Set the selected effect flag by index.
    /// Activate or deactivate that effect.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="targetBool"></param>
    public void SetEffectByIndex(int index, bool targetBool)
    {
        screenEffects[index].effectGameObject.SetActive(targetBool);
    }
}
