using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//Development logging for player position and gameplay state.
public class Debug_Info : MonoBehaviour
{
    void Start()
    {

        Debug.Log("carregou a cena: " + SceneManager.GetActiveScene().name);
    }

}
