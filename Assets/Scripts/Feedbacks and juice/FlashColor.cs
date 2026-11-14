using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FlashColor : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private string flashName;
    private Material flashMaterial;

    private float t;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        flashMaterial = spriteRenderer.material;
    }

    private void OnEnable()
    {
        //GetComponent<HandleAttackOnEnemy>().OnEnemyTakeHit += TriggerFlash; //Use Health.OnTakeDamage instead.
    }


    // Update is called once per frame
    void Update()
    {
        if(t >= 1)
        {
            flashMaterial.SetFloat(flashName, (float)Math.Pow(0.5f, t));
            t -= Time.deltaTime;
        }
        
    }

    private void TriggerFlash()
    {
        t = 1;
    }
}
