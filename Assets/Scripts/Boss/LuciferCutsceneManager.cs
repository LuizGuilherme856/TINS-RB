using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using TechBooh;
using UnityEngine;

public class LuciferCutsceneManager : MonoBehaviour
{

    [Header("Transforms")]
    public Transform player; // Player reference.
    public Transform lucifersSoul; // Object that emerges from the player.
    public Transform soulDestination; // Object destination.
    [Header("Time")]
    public float focusPlayerSeconds = 2f; // Time focusing on the player.
    public float objectSpawnSeconds = 2f; // Time spent focusing on the object after it spawns.
    public float moveDuration = 2f; // Travel duration to the destination.
    public float focusBothSeconds = 2f; // Time focusing on both subjects.
    [Space(10)]
    [Header("Scripts")]
    public CameraZoom cameraZoom; // CameraZoom reference.
    public BossFightArena bossFightArena; // BossFightArena reference.
    [SerializeField] private MoveObjectToPos moveCamera;
    public GameObject fakeLucifer; // Stand-in Lucifer reference.

    //private stuff
    private PlayerInputHandler playerInputHandler; // PlayerInputHandler reference.
    private bool cutsceneStarted = false;
    private PlayerAnimation playerAnimation; // Player Animator reference.

    public Vector3 chalicePosOffset; // Chalice offset relative to the player.
    public MMF_Player startFeedback;

    private void Awake()
    {
        playerInputHandler = Try.GetComponent<PlayerInputHandler>(player.gameObject);
        playerAnimation = Try.GetComponent<PlayerAnimation>(player.gameObject);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!cutsceneStarted && collision.transform == player)
        {
            startFeedback.PlayFeedbacks(); // Play cutscene-start feedbacks.
            cutsceneStarted = true;
            playerInputHandler.SetCanMove(false); // Disable player movement.
            playerAnimation.UpdateAnimation = false; // Disable player animations.
            playerAnimation.PlayIdle(); // Play the player's upward-facing idle animation.
            StartCoroutine(CutsceneSequence());
        }
    }


    private IEnumerator CutsceneSequence()
    {

        // 1. Focus on the player.
        moveCamera.StartMove(0); // Move the camera to the player.
        cameraZoom.Zoom(1f, 6, 0); // Zoom in on the player.
        yield return new WaitForSeconds(focusPlayerSeconds);

        // 2. The object emerges from the player.
        lucifersSoul.position = player.position + chalicePosOffset; // Place the object at the player position plus its offset.
        lucifersSoul.gameObject.SetActive(true);
        yield return new WaitForSeconds(objectSpawnSeconds);
        
        // 3. Follow the object and zoom out to include both subjects.
        float elapsed = 0f;
        Vector3 startPos = lucifersSoul.position;
        Vector3 endPos = soulDestination.position;


        cameraZoom.Zoom(4f, 8, 0); // Zoom out to frame both the object and player.
        moveCamera.StartMove(1); // Move the camera to the player-boss midpoint.
        while (elapsed < moveDuration)
        {
            float t = elapsed / moveDuration;
            lucifersSoul.position = Vector3.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        lucifersSoul.position = endPos;
        lucifersSoul.GetComponent<Animator>().SetTrigger("Start"); // Play the object's spawn animation.

        // 4. Frame the player and object before starting the fight.
        yield return new WaitForSeconds(focusBothSeconds);

        lucifersSoul.gameObject.SetActive(false); // Deactivate the object that emerged from the player.
        fakeLucifer.SetActive(true); // Activate stand-in Lucifer.
        StartCoroutine(bossFightArena.StartShowOff());
    }
}
