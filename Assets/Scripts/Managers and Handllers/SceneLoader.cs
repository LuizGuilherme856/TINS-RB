using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //Scene-management API.

/// <summary>
/// Scene-loading helpers.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [SerializeField] float transitionTime = 2f;
    private static bool isLoadingScene = false;
    //GameSession myGameSession;
    private void Awake()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
       // myGameSession = FindObjectOfType<GameSession>().GetComponent<GameSession>();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadNextScene()
    {
        if (TryBeginSceneLoad() == false) return;
        int indiceDeCenaAtual = SceneManager.GetActiveScene().buildIndex;
        StartCoroutine(SceneWithTransition(indiceDeCenaAtual +1, true));

    }

    public void LoadPreviousScene()
    {
        if (TryBeginSceneLoad() == false) return;
        int indiceDeCenaAtual = SceneManager.GetActiveScene().buildIndex;
        StartCoroutine(SceneWithTransition(indiceDeCenaAtual -1, true));

    }

    /*
    public void LoadMenu()
    {
        //DontDestroyOnLoad[] dontDestroy = FindObjectsOfType<DontDestroyOnLoad>();
        foreach (var obj in dontDestroy) //Destroy these persistent prefabs when returning to the menu.
        {
            obj.gameObject.SetActive(false);
              Destroy(obj.gameObject);
        }
        StartCoroutine(SceneWithTransition(0));
    }
    */

    public void QuitGame()
    {
        if (TryBeginSceneLoad() == false) return;
        TryFinalizeRunBeforeLeavingGameplay();
        Application.Quit(); //Quit the application.
    }

    public void LoadSpecificScene(int cena)
    {
        if (TryBeginSceneLoad() == false) return;
        StartCoroutine(SceneWithTransition(cena));
    }

    public void LoadSpecificScene(string name)
    {
        if (TryBeginSceneLoad() == false) return;
        StartCoroutine(SceneWithTransition(name));
    }

    public void LoadSpecificScene(string name, float time)
    {
        if (TryBeginSceneLoad() == false) return;
        StartCoroutine(SceneWithTransition(name, true, time));
    }


    public void LoadSpecificScene(int cena, float time)
    {
        if (TryBeginSceneLoad() == false) return;
        StartCoroutine(SceneWithTransition(cena, true, time));
    }

    public void LoadSpecificSceneNow(int cena)
    {
        if (TryBeginSceneLoad() == false) return;
        SceneManager.LoadScene(cena);
    }

    public void LoadCurrentScene()
    {
        if (TryBeginSceneLoad() == false) return;
        TryFinalizeRunBeforeLeavingGameplay();
        int indiceDeCenaAtual = SceneManager.GetActiveScene().buildIndex;
        StartCoroutine(SceneWithTransition(indiceDeCenaAtual, true));
    }

    private void TryFinalizeRunBeforeLeavingGameplay()
    {
        var scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.FinalizeCurrentRunScore();
        }
    }

    /// <summary>
    /// TechBooh:
    /// Use this method only when a scene change ends the current attempt,
    /// such as returning to the Hub or menu. Internal level/safe-zone transitions
    /// must not finalize the score.
    /// </summary>
    public void FinalizeRunAndLoadSpecificScene(string name)
    {
        if (TryBeginSceneLoad() == false) return;
        TryFinalizeRunBeforeLeavingGameplay();
        StartCoroutine(SceneWithTransition(name));
    }

    public void FinalizeRunAndLoadSpecificScene(string name, float time)
    {
        if (TryBeginSceneLoad() == false) return;
        TryFinalizeRunBeforeLeavingGameplay();
        StartCoroutine(SceneWithTransition(name, true, time));
    }

    public void FinalizeRunAndLoadSpecificScene(int cena)
    {
        if (TryBeginSceneLoad() == false) return;
        TryFinalizeRunBeforeLeavingGameplay();
        StartCoroutine(SceneWithTransition(cena));
    }

    public void FinalizeRunAndLoadSpecificScene(int cena, float time)
    {
        if (TryBeginSceneLoad() == false) return;
        TryFinalizeRunBeforeLeavingGameplay();
        StartCoroutine(SceneWithTransition(cena, true, time));
    }

    // Shared re-entry guard across SceneLoader instances. It is released by sceneLoaded;
    // a failed load needs an explicit recovery path or further transition requests remain blocked.
    private bool TryBeginSceneLoad()
    {
        if (isLoadingScene)
        {
            return false;
        }

        isLoadingScene = true;
        return true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isLoadingScene = false;
    }



    private IEnumerator SceneWithTransition(int index, bool transition = true, float time = -1)
    {
        Time.timeScale = 1;
        // if (transition)
        // {
        if (time == -1) time = transitionTime;
            yield return new WaitForSeconds(time);
       // }
        SceneManager.LoadScene(index);
    }

    // This restores timeScale before a scaled delay and then loads synchronously. New blocking modals
    // or platform interruptions must coordinate with this path rather than assume pause remains unchanged.
    private IEnumerator SceneWithTransition(string sceneName, bool transition = true, float time = -1)
    {
        Time.timeScale = 1;
        // if (transition)
        // {
        if (time == -1) time = transitionTime;
        yield return new WaitForSeconds(time);
        // }
        SceneManager.LoadScene(sceneName);
    }


}
