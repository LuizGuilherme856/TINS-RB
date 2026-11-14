using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePosWithController : MonoBehaviour
{
    [Tooltip("Referência ao objeto cuja posição será alterada.")]
    [SerializeField] private Transform targetObject;

    [Tooltip("Velocidade de movimento do objeto.")]
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private PlayerHolder playerHolder;
    [Tooltip("Referência ao PlayerInputHandler para ler o input do controle.")]
    private PlayerInputHandler playerInputHandler;

    private void Awake()
    {
        playerInputHandler = playerHolder.GetPlayerInputHandler();
    }

    private void Update()
    {
        if(playerInputHandler.isUsingController == false)
        {
            return;
        }

        if (playerInputHandler == null || targetObject == null)
        {
            Debug.LogWarning("PlayerInputHandler ou TargetObject não está configurado.");
            return;
        }

        // Read controller input.
        Vector2 aimInput = playerInputHandler.GetLeftAnalogInput();

        // Convert to a movement vector while preserving Z; negate to invert direction.
        Vector3 movement = new Vector3(-aimInput.x, -aimInput.y, 0f);

        // Move according to input.
        targetObject.position += movement * moveSpeed * Time.deltaTime;
    }
}
