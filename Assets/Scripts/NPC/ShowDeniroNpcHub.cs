using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Choose De Niro's Hub representation according to unlock state.
public class ShowDeniroNpcHub : MonoBehaviour
{
    [SerializeField] GameObject deniroLocked, deniroAvailableToBuy;
    [SerializeField] PlayerData playerData;

    
    void Awake()
    {
        if(playerData.GetTotalKills() >= 10000)
        {
            deniroLocked.SetActive(false);
            deniroAvailableToBuy.SetActive(true);
        }
        else
        {
            deniroLocked.SetActive(true);
            deniroAvailableToBuy.SetActive(false);
        }
        
    }

}
