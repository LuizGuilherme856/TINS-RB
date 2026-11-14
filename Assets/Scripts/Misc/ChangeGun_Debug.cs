using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Development tool for changing player weapons while recording the trailer.
// Development-only behavior: this component has no build guard. Verify scene/prefab activation
// and exclude or guard it explicitly before shipping, even when the main cheat action is disabled.
public class ChangeGun_Debug : MonoBehaviour
{

    [SerializeField] private GunData[] gunDatas;
    [SerializeField] GunInventory gunInventory;

    private int currentIndex = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            NextGun(false);
        }
        else if(Input.GetKeyDown(KeyCode.K)) 
        {
            NextGun(true);
        }
    }

    private void NextGun(bool increase)
    {
        if (increase)
        {
            currentIndex++;
            if(currentIndex >= gunDatas.Length) currentIndex= 0;
        }
        else
        {
            currentIndex--;
            if(currentIndex < 0) currentIndex = gunDatas.Length -1;
        }

        gunInventory.AddGunToInventory(gunDatas[currentIndex]); 
    }
}
