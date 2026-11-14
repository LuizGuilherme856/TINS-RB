using ND_VariaBULLET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariaAutomatorSetter : MonoBehaviour
{

    // Changes VariaBULLET automator speed.

    //Linear attack automator used to change the attack's rotation speed.
    [SerializeField] protected AutomateLinear variaAutomate;

    public void SetAutomateSpeed(float tagetSpeed)
    {
        variaAutomate.Speed = tagetSpeed;
    }


}
