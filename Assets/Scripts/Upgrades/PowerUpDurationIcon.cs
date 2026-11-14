using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpDurationIcon : MonoBehaviour
{

    /// Allow another component to supply the active duration.
    /// Start flashing feedback near expiration.
    /// Destroy the object on completion.

    private Image image;
    private void Awake()
    {
        image= GetComponent<Image>();
    }

    public void SetIcon(Sprite icon)
    {
        image.sprite= icon;
    }

    public void SetLifeTime( float lifeTime)
    {
        Destroy(gameObject, lifeTime);
    }
}
