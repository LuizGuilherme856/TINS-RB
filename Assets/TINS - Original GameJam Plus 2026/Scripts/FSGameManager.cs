using System.Collections;
using UnityEngine;

public class FSGameManager : MonoBehaviour
{

    public bool isDoingMinigame = false;
    public bool wanstToInteract = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            wanstToInteract = true;
        }
        else 
        {
            wanstToInteract = false;
        }
    }

    public void SetisDoingMinigame(bool targetBool)
    {
        isDoingMinigame = targetBool;
    }

    public void SetisDoingMinigameWithDelay(bool targetBool)
    {
        if (!isActiveAndEnabled) return;

        StartCoroutine(ChangeBoolWithDelay(0.2f, targetBool));
    }

    protected IEnumerator ChangeBoolWithDelay(float delay,bool targetBool)
    {
        yield return new WaitForSeconds(delay);
        isDoingMinigame = targetBool;
    }
}
