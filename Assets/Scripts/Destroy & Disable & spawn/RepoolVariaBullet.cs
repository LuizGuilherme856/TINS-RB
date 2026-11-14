using ND_VariaBULLET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepoolVariaBullet : MonoBehaviour
{

    private ShotBase shotBase;
    [SerializeField] protected bool manualVariaShot = false;

    private void Awake()
    {
        shotBase = GetComponent<ShotBase>();

    }


    public void Repoolbullet()
    {
        shotBase.RePool_TechBooh(manualVariaShot);
    }

}
