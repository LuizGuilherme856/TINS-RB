using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Development-only behavior: this component has no build guard. Verify scene/prefab activation
// and exclude or guard it explicitly before shipping, even when the main cheat action is disabled.
public class ChangeScene_Debug : MonoBehaviour
{
    [SerializeField] private SceneLoader sceneLoader;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            sceneLoader.LoadPreviousScene();
        }
        else if (Input.GetKeyDown(KeyCode.Equals))
        {
            sceneLoader.LoadNextScene();
        }
    }
}
