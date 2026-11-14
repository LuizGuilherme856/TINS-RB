using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Receive a special-attack activation, identify the ability,
//and trigger its associated effects.

public class HandleSpecialAttack : MonoBehaviour
{
    [SerializeField] private MeleeData intangibleMeleeData;


    [SerializeField] private SetIntangible setIntangible;
    public bool TriggerSpecialAttack(MeleeData meleeData)
    {
        if(meleeData == intangibleMeleeData)
        {
            return HandleIntangible();
        }

        return true;
    }

    private bool HandleIntangible()
    {
        // Do not spend another Hanba charge while intangibility
        // is already active.
        if (setIntangible.IsIntangibleActive())
        {
            return false;
        }

        setIntangible.ActivateIntangible(intangibleMeleeData.specialFxDuration);
        return true;
    }
}
