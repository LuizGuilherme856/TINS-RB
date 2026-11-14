using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinematicTextSequence cinematicTextSequence;
    [SerializeField] private Transform objectToMove;
    [SerializeField] private Transform destination;
    [SerializeField] private float moveSpeed = 2f;



    public SceneLoader sceneLoader;


    public HandleGameOver handleGameOver;
    public Button continueButton;
    public CanvasGroup canvasGroup;
    public List<GameObject> objToDisable;

    private Coroutine moveCoroutine;

    void Awake()
    {
        if (cinematicTextSequence != null)
        {
            cinematicTextSequence.OnSequenceEnd += HandleCinematicEnd;
        }
    }

    private void OnDestroy()
    {
        if (cinematicTextSequence != null)
        {
            cinematicTextSequence.OnSequenceEnd -= HandleCinematicEnd;
        }
    }

    private void HandleCinematicEnd()
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveObjectCoroutine());
    }

    private IEnumerator MoveObjectCoroutine()
    {
#if UNITY_EDITOR
        Time.timeScale = 25f;
        #endif
        if (objectToMove == null || destination == null)
            yield break;

        while (objectToMove.position != destination.position)
        {
            objectToMove.position = Vector3.MoveTowards(
                objectToMove.position,
                destination.position,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        foreach (var obj in objToDisable) obj.SetActive(false);
        canvasGroup.alpha = 0;
        
        #if UNITY_EDITOR
        Time.timeScale = 1;
        #endif

        handleGameOver.PlayEndScreen();

        yield return new WaitForSeconds(15);

        continueButton.onClick.Invoke();

        //sceneLoader.LoadSpecificScene("Main menu");
    }
}
