using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Development-only behavior: this component has no build guard. Verify scene/prefab activation
// and exclude or guard it explicitly before shipping, even when the main cheat action is disabled.
public class ChangeSpecialAttack_Debug : MonoBehaviour
{
    [SerializeField] private MeleeManager specialMeleeManager;

    [SerializeField] private MeleeData[] meleeDatas;

    private int currentIndex=0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            NextGun(false);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            NextGun(true);
        }
    }

    private void NextGun(bool increase)
    {
        if (increase)
        {
            currentIndex++;
            if (currentIndex >= meleeDatas.Length) currentIndex = 0;
        }
        else
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = meleeDatas.Length - 1;
        }

        specialMeleeManager.SetMeleeWeapon(meleeDatas[currentIndex]);
    }
}
