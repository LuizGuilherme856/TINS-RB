using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class ChangeChainMaterial : MonoBehaviour
{
    [SerializeField] private ManageSacrifice sacrifice;

    private Material material;

    private void Awake()
    {
        material = GetComponent<SpriteShapeRenderer>().materials[1];
    }

    private void Start()
    {
        sacrifice.OnSacrificeComplete += ChangeMaterial;
    }

    private void ChangeMaterial()
    {
        material.SetFloat("_SineGlowFade", 1);
        material.SetFloat("_BurnFade", 0);
    }
}
