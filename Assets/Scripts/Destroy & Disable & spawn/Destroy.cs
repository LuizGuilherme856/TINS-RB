///Destroys objects, optionally fading them out first.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Destroy : MonoBehaviour
{
    [SerializeField] private DestroyBasedOn destroyBasedOn;

    [SerializeField] protected float lifeTime = 5f, maxLifeTime = 200f;
    [SerializeField] private bool destroyWhenOnCamera = true;
    [SerializeField][Range(0, 1)] private float shrinkRate = 0.99f, fadeOutRate= 0.99f;

    [SerializeField] private bool fadeOutSpriteAlpha = false;
    [SerializeField] private DestroyMethod destroyMethod = DestroyMethod.destroyGameObject;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject spawnObjectOnDestroy;
    [SerializeField] protected RunData runData;
    [SerializeField] private bool decreaseLifeTimeBasedOnCurrentWave = false;
    [SerializeField] private bool goBackToOriginalPosition = false; 
    private float timer;
    
    private bool canStartFading = false; //Objects restricted to offscreen destruction become readyToDestroy only after leaving the camera view.
    private bool isVisible;
    private Color originalColor;
    private Vector3 originalLocalScale, originalPos; // TODO: Add a flag to reset local position.

    public event Action OnTriggerDestroy;

    public bool debug = false;

    private void Awake()
    {
        DoubleCheckRateValues();

        if (fadeOutSpriteAlpha && spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if(spriteRenderer) originalColor = spriteRenderer.color;
        originalLocalScale = transform.localScale;
       originalPos = transform.localPosition;



    }


    protected virtual void OnEnable()
    {
        ResetValues();

        //Reduce object lifetime as waves progress to prevent thousands of objects accumulating.
        if(decreaseLifeTimeBasedOnCurrentWave)
        {
            float defaultLifeTime = lifeTime;
            lifeTime -= runData.currentWaveNumber * 7;
            if(lifeTime < 10 && defaultLifeTime > 10)
            {
                lifeTime = 10;
            }
        }


    }

    private void ResetValues()
    {
        if (fadeOutSpriteAlpha) spriteRenderer.color = originalColor;
        transform.localScale = originalLocalScale;
        if (goBackToOriginalPosition) transform.localPosition = originalPos;

        //Do not begin fading or shrinking while visible.
        if (destroyWhenOnCamera) canStartFading = true; 
        else canStartFading = false;

        timer = 0;

    }

    private void FixedUpdate()
    {

        timer += Time.fixedDeltaTime;

        if (timer > maxLifeTime)
        {
            HandleDestroy(true);
        }


        if (destroyBasedOn == DestroyBasedOn.time && destroyWhenOnCamera && timer > lifeTime)
        {
            HandleDestroy();
        }


        if (destroyBasedOn == DestroyBasedOn.time) return;

        if (timer > lifeTime && canStartFading )
        {
            transform.localScale = new Vector3(transform.localScale.x*shrinkRate, transform.localScale.y*shrinkRate, transform.localScale.z*shrinkRate);

            if(destroyBasedOn == DestroyBasedOn.size && transform.localScale.x <= 0.001f)
            {
                HandleDestroy();
            }

            if (spriteRenderer == null) return;//Already destroyed.

            if (fadeOutSpriteAlpha)
            {
                var alphaColor = spriteRenderer.color.a;
                var red = spriteRenderer.color.r;
                var green = spriteRenderer.color.g;
                var blue = spriteRenderer.color.b;

                spriteRenderer.color = new Color(red, green, blue, alphaColor*fadeOutRate);

                if(spriteRenderer.color.a <= 0.001f && destroyBasedOn == DestroyBasedOn.alpha)
                {
                    HandleDestroy();
                }
            }
        }



    }

    public void TriggerDestroy(DestroyBasedOn destroyBasedOn, bool canStartFading, float shrinkRate = 0.99f)
    {
        this.canStartFading = canStartFading;
        this.shrinkRate = shrinkRate;
        this.destroyBasedOn = destroyBasedOn;
        timer = lifeTime +1;
    }

    private void OnBecameInvisible()
    {
        isVisible= false;
        canStartFading = true;
       
    }

    private void OnBecameVisible()
    {
        isVisible= true;
        if (destroyWhenOnCamera) canStartFading = true;
        else canStartFading = false;
    }


    private void HandleDestroy(bool forceDestroy = false)
    {
        if(forceDestroy == false)
        {
            if (destroyWhenOnCamera == false && isVisible) return; //Not ready for destruction yet.
        }

        OnTriggerDestroy?.Invoke();
        if(spawnObjectOnDestroy!= null ) Instantiate(spawnObjectOnDestroy, transform.position, transform.rotation);


        if (destroyMethod == DestroyMethod.deactivateGameObject)
        {
            gameObject.SetActive(false);
        }
        else if (destroyMethod == DestroyMethod.destroyGameObject)
        {
            Destroy(gameObject);
        }

    }

    private void DoubleCheckRateValues()
    {
        //Use an explicit completion condition because size-based destruction
        //would never finish with shrinkRate equal to 1.
        switch (destroyBasedOn)
        {
            case DestroyBasedOn.size:
                if (shrinkRate == 1) shrinkRate = 0.99f;
                break;
            case DestroyBasedOn.alpha:
                if (fadeOutRate >= 1)
                {
                    fadeOutRate = 0.99f;
                    if (debug)
                    {
                        Debug.LogWarning("fadeRate should be less than 1. Setting to 0.99 automatically. - Luiz");
                    }
                }
                if (fadeOutSpriteAlpha == false)
                {
                    fadeOutSpriteAlpha = true;
                    if (debug)
                    {
                        Debug.LogWarning("fadeOutSpriteAlpha should be true. Setting to true automatically. - Luiz");
                    }
                }
                break;
        }




    }


}
    public enum DestroyBasedOn { time, size, alpha};
    public enum DestroyMethod { destroyGameObject, deactivateGameObject, triggerEventOnly}
