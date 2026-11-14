using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILander 
{
    public event Action OnPrepareToDie;
    public Vector2 GetLandingSpot();
}
