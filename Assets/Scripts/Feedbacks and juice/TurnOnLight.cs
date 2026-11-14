using MoreMountains.Feedbacks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Turns on the light when the player interacts.
/// </summary>
public class TurnOnLight : InteractWithPlayer
{
    [SerializeField] private Light2D light2D;
    [SerializeField] private int turnOnCost = 500;
    [SerializeField] private GameObject lightSpriteGameObj;
    [SerializeField] private float targetIntensity = 1f; // Final light intensity.
    [SerializeField] private float intensityIncreaseDuration = 2f; // Intensity ramp duration.

    private bool lightIsOn = false;
    private Animator animator;
    public Action OnTurnOnLight;

    protected override void Awake()
    {
        base.Awake();
        if (light2D != null)
        {
            light2D.enabled = false; // The light starts off.
            light2D.intensity = 0f;  // Initialize intensity to zero.
        }

        lightSpriteGameObj.SetActive(false); // Initially disable the light sprite.
        animator = GetComponent<Animator>();
    }

    protected override void StartInteraction()
    {
        if (lightIsOn) return;

        base.StartInteraction();
        string priceColored;

        // Set price color according to the player's available points.
        if (pointManager.currentPoints >= turnOnCost)
        {
            priceColored = UiColors.green_ + turnOnCost + UiColors.endColor_;
        }
        else
        {
            priceColored = UiColors.red_ + turnOnCost + UiColors.endColor_;
        }

        switch (languageData.currentLanguage)
        {
            case Language.english:
                generalTextBoxUI.SetText($"{interactionButton} Turn On Light (Cost: {priceColored})");
                break;
            case Language.chinese:
                generalTextBoxUI.SetText($"{interactionButton} 开灯 (消耗: {priceColored})");
                break;
            case Language.portuguese:
                generalTextBoxUI.SetText($"{interactionButton} Ligar Luz (Preço: {priceColored})");
                break;
            case Language.spanish:
                generalTextBoxUI.SetText($"{interactionButton} Encender Luz (Precio: {priceColored})");
                break;
            default:
                generalTextBoxUI.SetText($"{interactionButton} Turn On Light (Cost: {priceColored})");
                break;
        }
    }

    protected override void EndInteraction()
    {
        base.EndInteraction();
        generalTextBoxUI.RemoveText();
    }

    protected override void InteractionAction()
    {
        if (lightIsOn) return;

        base.InteractionAction();

        if (pointManager.currentPoints >= turnOnCost)
        {
            pointManager.RemovePoints(turnOnCost);
            if (light2D != null)
            {
                lightSpriteGameObj.SetActive(true); // Enable the light sprite.
                light2D.enabled = true;
                lightIsOn = true;
                OnTurnOnLight?.Invoke();
                StartCoroutine(IncreaseLightIntensity()); // Start the intensity-ramp coroutine.
                animator.SetTrigger("TurnOn");
            }
            generalTextBoxUI.RemoveText();
            buyFeedback.PlayFeedbacks();
        }
        else
        {
            // Insufficient-points feedback.
            notEnoughPointsFeedbacks?.TriggerNotEnoughPointsFeedbacks();
        }
    }

    private IEnumerator IncreaseLightIntensity()
    {
        float elapsedTime = 0f;
        float startingIntensity = light2D.intensity;

        while (elapsedTime < intensityIncreaseDuration)
        {
            elapsedTime += Time.deltaTime;
            light2D.intensity = Mathf.Lerp(startingIntensity, targetIntensity, elapsedTime / intensityIncreaseDuration);
            yield return null;
        }

        light2D.intensity = targetIntensity; // Set the exact final intensity.
    }
}
