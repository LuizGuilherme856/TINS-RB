using UnityEngine;

public class ChangeUICursorOnControllerConection : MonoBehaviour
{

    private PlayerInputHandler playerInput;

    private void Awake()
    {
        playerInput = FindFirstObjectByType<PlayerInputHandler>(); 
    }

    void Start()
    {
        playerInput.OnControllerChange += SetCursorVisible;
    }

    private void SetCursorVisible()
    {
        if(playerInput.isUsingController) Cursor.visible = false;
        else Cursor.visible = true;

    }
}
