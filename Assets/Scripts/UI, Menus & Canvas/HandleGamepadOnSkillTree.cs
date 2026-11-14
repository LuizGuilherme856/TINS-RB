using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandleGamepadOnSkillTree : MonoBehaviour
{
    public float zoomSpeed = 1f;
    public PlayerHolder playerHolder;
    private PlayerInputHandler playerInputHandler;
    public Button closeButton, ResetButton;
    public SkillTreeZoom skillTreeZoom;

    private bool isRightShoulderPressed = false;
    private bool isLeftShoulderPressed = false;

    private void Awake()
    {
        playerInputHandler = playerHolder.GetPlayerInputHandler();
    }

    private void OnEnable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnMenuB += OnClose;
            playerInputHandler.OnMenuY += OnReset;
            playerInputHandler.OnRightShoulderDown += OnRightShoulderDown;
            playerInputHandler.OnRightShoulderUp += OnRightShoulderUp;
            playerInputHandler.OnLeftShoulderDown += OnLeftShoulderDown;
            playerInputHandler.OnLeftShoulderUp += OnLeftShoulderUp;
        }
        else
        {
            Debug.LogWarning("PlayerInputHandler não está configurado.");
            return;
        }
    }

    private void OnDisable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnMenuB -= OnClose;
            playerInputHandler.OnMenuY -= OnReset;
            playerInputHandler.OnShootButtonDown -= OnRightShoulderDown;
            playerInputHandler.OnShootButtonUp -= OnRightShoulderUp;
            playerInputHandler.OnMeleeButtonDown -= OnLeftShoulderDown;
            playerInputHandler.OnMeleeButtonUp -= OnLeftShoulderUp;
        }
    }

    private void Update()
    {
        if (isRightShoulderPressed)
        {
            skillTreeZoom.AddToZoom(zoomSpeed * Time.deltaTime);
        }
        if (isLeftShoulderPressed)
        {
            skillTreeZoom.AddToZoom(-zoomSpeed * Time.deltaTime);
        }
    }

    private void OnRightShoulderDown()
    {
        isRightShoulderPressed = true;
    }

    private void OnRightShoulderUp()
    {
        isRightShoulderPressed = false;
    }

    private void OnLeftShoulderDown()
    {
        isLeftShoulderPressed = true;
    }

    private void OnLeftShoulderUp()
    {
        isLeftShoulderPressed = false;
    }

    private void OnClose()
    {
        closeButton.onClick.Invoke();
    }

    private void OnReset()
    {
        ResetButton.onClick.Invoke();
    }
}
