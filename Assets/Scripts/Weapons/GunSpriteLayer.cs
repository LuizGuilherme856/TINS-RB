///Place the weapon behind or in front of the player
///according to upward or downward facing.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GunSpriteLayer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private int behindPlayerLayer = 5, inFrontPlayerLayer =5;
    private Aim aim;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        aim = GetComponentInParent<Aim>();
    }

    private void OnEnable()
    {
        aim.onAimingNorthAction += BehindThePlayer;
        aim.onNotAimingNorthAction += InFrontOfThePlayer;
    }

    private void OnDisable()
    {
        aim.onAimingNorthAction -= BehindThePlayer;
        aim.onNotAimingNorthAction -= InFrontOfThePlayer;
    }

    private void InFrontOfThePlayer()
    {
        spriteRenderer.sortingOrder = playerSprite.sortingOrder + inFrontPlayerLayer;
    }
    private void BehindThePlayer()
    {
        spriteRenderer.sortingOrder = playerSprite.sortingOrder + behindPlayerLayer;
    }
}
