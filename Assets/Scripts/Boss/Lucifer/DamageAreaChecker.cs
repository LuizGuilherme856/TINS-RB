using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageAreaChecker : MonoBehaviour
{
    public bool inSafeArea = false;
    public bool inDamageArea = false;

    public void Reset()
    {
        inDamageArea = false;
        inSafeArea = false;
    }

    private void OnDisable()
    {
        Reset();
    }

}
