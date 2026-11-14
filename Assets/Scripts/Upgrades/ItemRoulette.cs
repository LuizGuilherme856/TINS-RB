using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Animate a casino-style roulette selection.
/// </summary>

public class ItemRoulette : MonoBehaviour
{
    public SpriteRenderer spriteRenderer; // SpriteRenderer to update.
    public List<Sprite> itemSprites = new(); // Weapon sprites.

    [SerializeField] private float initialSpeed = 0.05f; // Initial sprite-switch speed.
    [SerializeField] private float slowDownFactor = 1.05f; // Deceleration factor.
    [SerializeField] private float finalDelay = 0.5f; // Pause duration on the selected item.


    [SerializeField] MMF_Player startSpinningFeedbacks, spinningFeedbacks, finishFeedbacks;

    private int finalIndex; // Selected item index.
    public event Action OnFinishRoulette;
    public event Action OnStatusChange;

    private void OnEnable()
    {
        spriteRenderer.sprite = null;
    }

    public void StartRoulette(int preselectedIndex)
    {
        startSpinningFeedbacks.PlayFeedbacks();
        
        finalIndex = preselectedIndex;
        StartCoroutine(RouletteSpin());
        OnStatusChange?.Invoke();
    }

    private IEnumerator RouletteSpin()
    {
        float speed = initialSpeed;
        int currentIndex = 0;
        spriteRenderer.gameObject.SetActive(true);

        // Decelerating animation.
        for (int i = 0; i < 50; i++)
        {
            // Cycle through the sprites.
            spriteRenderer.sprite = itemSprites[currentIndex];
            currentIndex = (currentIndex + 1) % itemSprites.Count;

        spinningFeedbacks.PlayFeedbacks();
            yield return new WaitForSeconds(speed);
            speed *= slowDownFactor;
        }

        // Display the previously selected final sprite.
        spriteRenderer.sprite = itemSprites[finalIndex];
        
        finishFeedbacks.PlayFeedbacks();
        yield return new WaitForSeconds(finalDelay);

        OnFinishRoulette?.Invoke();
        OnStatusChange?.Invoke();

    }

    public void SetSprites(List<Sprite> sprites)
    {
        itemSprites = sprites;
    }

    public void ActivateRouletteItem(bool activate = false)
    {
        spriteRenderer.gameObject.SetActive(activate); 
    }

    public IEnumerator BlinkSpriteRenderer(float duration)
    {
        float elapsedTime = 0f;
        float blinkInterval = 0.5f; // Initial flashing interval.

        while (elapsedTime < duration)
        {
            spriteRenderer.gameObject.SetActive(!spriteRenderer.gameObject.activeSelf);
            yield return new WaitForSeconds(blinkInterval);

            elapsedTime += blinkInterval;
            // Reduce the flashing interval over time.
            blinkInterval = Mathf.Lerp(0.5f, 0.05f, elapsedTime / duration);
        }

        // Ensure the object is inactive at completion.
        spriteRenderer.gameObject.SetActive(false);
    }
}
