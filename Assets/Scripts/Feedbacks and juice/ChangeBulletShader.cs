using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(EmissiveShot))]

//Switch bullet effects according to whether the weapon is golden.

public class ChangeBulletShader : MonoBehaviour
{
    [SerializeField] private Color32 cyanColor = Color.cyan;
    [SerializeField] private bool changeBulletImpact = false;
    [SerializeField] private Material bulletImpactMat, blueBulletImpactMat;

    
    
    private SpriteRenderer spriteRenderer;
    private Light2D myLight;
    private Color32 defaultColor;
    private HandleBulletImpact handleBulletImpact;

    private void Awake()
    {
        spriteRenderer= GetComponent<SpriteRenderer>();
        myLight= GetComponent<Light2D>();

        defaultColor = myLight.color;
        handleBulletImpact = GetComponent<HandleBulletImpact>();
    }

    public void ChangeBulletColorToBlue()
    {
        spriteRenderer.material.SetFloat("_FrozenFade", 1);
        spriteRenderer.material.SetFloat("_AddColorFade", 0);
        myLight.color = cyanColor;

        if (changeBulletImpact) handleBulletImpact.ChangeBulletImpactMaterial(blueBulletImpactMat);
    }

    public void ChangeColorToDefault()
    {
        spriteRenderer.material.SetFloat("_FrozenFade", 0);
        spriteRenderer.material.SetFloat("_AddColorFade", 1);
        myLight.color = defaultColor;
        if (changeBulletImpact) handleBulletImpact.ChangeBulletImpactMaterial(bulletImpactMat);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
