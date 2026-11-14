///Hide the gun sprite during melee attacks.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideGunSprite : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void EnableGunSprite(bool SetEnable)
    {
        spriteRenderer.enabled = SetEnable;
    }

}
