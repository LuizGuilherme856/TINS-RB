
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using TechBooh;
using UnityEngine;

/// <summary>
/// Change materials according to the damage type received.
/// For example, fire damage uses the burning material.
/// </summary>

public class ShaderReplacer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject burnShaderController;
    [SerializeField] private GameObject hitFeedbackCorpse;

    [SerializeField] private bool changeMaterialOnChild = false, isCorpse;
    private Material material;

    /// <summary>
    /// Shader name.
    /// </summary>
    private const string BURN_FADE_NAME = "_BurnFade", FROZEN_FADE_NAME = "_FrozenFade",
        POISON_FADE_NAME = "_PoisonFade", WATER_SATURATION = "_Saturation";

    //Electricity effect.
    private string[] ELECTRICITY_FADE_NAMES = {"_SineGlowFade", "_SineMoveFade", "_VibrateFade"};

    private void Awake()
    {
        CheckSpriteRenderers();
    }

    private void CheckSpriteRenderers()
    {
        //Detect SpriteRenderer automatically instead of maintaining a separate Inspector flag.
        var sprite = GetComponent<SpriteRenderer>();

        if (sprite != null) //Check whether the GameObject has no sprite.
        {
            material = sprite.material;
        }
    }

    public void SetShaderValue(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Puncture:
                break;
            case DamageType.Slash:
                break;
            case DamageType.Impact:
                break;
            case DamageType.Explosive:
                break;
            case DamageType.Fire:
                if (isCorpse) SetBurnedCorpseShader();
                else SetBurningShader();
                break;
            case DamageType.Ice:
                SetFloatShader(damageType, FROZEN_FADE_NAME, 1);
                break;
            case DamageType.Poison:
                SetFloatShader(damageType, POISON_FADE_NAME, 1);
                break;
            case DamageType.Water:
                material.SetFloat(WATER_SATURATION, 0.5f);
                break;
            case DamageType.Electricity:
                SetElectricityShader(1);
                break;
            case DamageType.Ether:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Iterate all electricity effects.
    /// </summary>
    /// <param name="fadeValue">0 disables the effect; 1 is full strength.</param>
    private void SetElectricityShader(float fadeValue)
    {
        if(material == null)
        {
            Debug.LogWarning("Material nulo no '" + name + "'. Procurando com get component...");
            material = Try.GetComponent<SpriteRenderer>(this.gameObject).material;
        }

        foreach (var fadeName in ELECTRICITY_FADE_NAMES)
        {
            material.SetFloat(fadeName, fadeValue);
        }
    }

    private void SetBurningShader()
    {

    }

    private void SetBurnedCorpseShader()
    {
        hitFeedbackCorpse.SetActive(false);
        spriteRenderer.material?.SetFloat(BURN_FADE_NAME, 1f);
        burnShaderController.SetActive(true);

        float swirlValue = Random.Range(-5f, 5f); //Randomize generated embers.
        spriteRenderer.material.SetFloat("_BurnSwirlFactor", swirlValue);
    }

    private void SetFloatShader(DamageType damageType, string name, float parameter)
    {
        if (changeMaterialOnChild)
        {
            ChangeShaderValueOnEveryChild(damageType);
        }
        else
        {
            material.SetFloat(name, parameter);
            //spriteRenderer.material = frozenShaderMaterial;
        }
    }
    /// <summary>
    /// Reset shader values to zero.
    /// </summary>
    public void SetDefaultShaderValue()
    {
        // spriteRenderer.material = defaultMaterial;
        SetElectricityShader(0);
        material.SetFloat(FROZEN_FADE_NAME, 0f);
        material.SetFloat(BURN_FADE_NAME, 0f);
        material.SetFloat(POISON_FADE_NAME, 0f);
        material.SetFloat(WATER_SATURATION, 1f);

    }



    private void ChangeShaderValueOnEveryChild(DamageType damageType)
    {
        foreach (Transform child in transform)
        {
            //child.gameObject.GetComponent<SpriteRenderer>().material = newMaterial;
            if(damageType == DamageType.Ice)
            {
                child.gameObject.GetComponent<SpriteRenderer>().material.SetFloat("_FrozenFade", 1f);
            }
            else if (damageType == DamageType.Fire)
            {
                child.gameObject.GetComponent<SpriteRenderer>().material.SetFloat("_BurnFade", 1f);
            }
        }
    }

}
