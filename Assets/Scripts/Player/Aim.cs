using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Aim : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInput;
    [SerializeField] private float angleThreshold = 70f; //Angle threshold between facing right and facing up.
   // [SerializeField] private float flipSpriteAngleOffset = 20f; //Require crossing this threshold before changing the current aiming direction.
    [SerializeField] private PlayerAnimation playerAnimation;

    [SerializeField] private bool showDebug= false;

    //Events used to update weapon sprite sorting.
    public event Action onAimingNorthAction, onNotAimingNorthAction;

    

    void Update()
    {

        float angle = playerInput.GetAimAngle();
        Aux.Print(angle.ToString(), showDebug);

     
        if(angle>-90 && angle <= 90)
        {
            playerAnimation.SetFlipSprite(1);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            playerAnimation.SetFlipSprite(-1);
            transform.rotation = Quaternion.Euler(0, 0, 180+ angle);
        }

        
    }

    public bool GetIsAimingRight()
    {
        var angle = playerInput.GetAimAngle();
        if (angle > -90 && angle <= 90)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public float GetAimAngle()
    {
        return playerInput.GetAimAngle();
    }

    
    public Direction AimDirection()
    {
        var angle = playerInput.GetAimAngle();
        if(angle > - angleThreshold && angle < angleThreshold)
        {
            Aux.Print("East", showDebug);
            onNotAimingNorthAction?.Invoke();
            return Direction.East;   
        }
        else if(angle >= angleThreshold && angle <= 110)
        {
            Aux.Print("North", showDebug);
            onAimingNorthAction?.Invoke();

            return Direction.North;
        }
        else if (angle <= -angleThreshold && angle >= -110)
        {
            Aux.Print("South", showDebug);
            onNotAimingNorthAction?.Invoke();
            return Direction.South;
        }
        else
        {
            Aux.Print("West", showDebug);
            onNotAimingNorthAction?.Invoke();
            return Direction.West;
        }
    }


    public Transform GetAimTransform()
    {
        return transform;
    }
    
}


