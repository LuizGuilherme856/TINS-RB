using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Development-only behavior: this component has no build guard. Verify scene/prefab activation
// and exclude or guard it explicitly before shipping, even when the main cheat action is disabled.
public class ActivateOrbitalUpgrade_Debug : MonoBehaviour
{

    [SerializeField] private GameObject orbitalUpgrade;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Backspace)) 
        {
            if (orbitalUpgrade.activeInHierarchy)
            {
                orbitalUpgrade.SetActive(false);
            }
            else
            {
                orbitalUpgrade.SetActive(true);
            }
        }
    }
}
