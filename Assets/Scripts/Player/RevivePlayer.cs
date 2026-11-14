using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevivePlayer : MonoBehaviour
{
    private HandlePowerUpEffects powerUpEffects;
    private Health playerHealth;
    [SerializeField] private GameObject reviveFx;


    // Start is called before the first frame update
    void Start()
    {
        powerUpEffects = GetComponent<HandlePowerUpEffects>();
    }


    public void Revive()
    {
        Instantiate(reviveFx, transform.position, transform.rotation);
        powerUpEffects.Shield();
        powerUpEffects.ExtraLife();
    }
}
