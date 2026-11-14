using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Controls meteor behavior.
/// A meteor moves from a higher to a lower Y position
/// and triggers its impact animation on reaching the destination Y.
/// </summary>
public class Meteor : MonoBehaviour
{
    public Transform targetTransform; // Target transform.
    public bool canMove = false; // Whether the meteor may move.
    public GameObject warningPrefab; // Impact-warning prefab.

    [SerializeField] private float fallSpeed = 5f; // Falling speed.
    public MMF_Player impactFeedback;

    private Animator animator; // Animator reference.
    private PolygonCollider2D polygonCollider2D;

    private readonly string IMPACT_TRIGGER = "Impact"; // Impact animation trigger name.
    private readonly string METEOR_LOOP_NAME = "meteor loop idle"; // Meteor loop animation name.

    private void Awake()
    {
        animator = GetComponent<Animator>(); // Get the Animator component.
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        polygonCollider2D.enabled = false;
    }

    private void FixedUpdate()
    {
        if (canMove == false) return;
        // Move the meteor downward.
        transform.position += fallSpeed * Time.fixedDeltaTime * Vector3.down;

        // Check whether the meteor reached the destination Y.
        if (transform.position.y <= targetTransform.position.y)
        {
            StartCoroutine(TriggerImpactAnimation_Routine());
        }
    }

    public void SetTargetTransform(Transform target)
    {
        targetTransform = target;
        warningPrefab.transform.parent = targetTransform;
        warningPrefab.transform.localPosition = Vector3.zero;
        warningPrefab.SetActive(false);
    }

    public void StartFalling()
    {
        animator.Play(METEOR_LOOP_NAME); // Start the falling animation.
        canMove = true; // Enable this script to begin falling.
        warningPrefab.SetActive(true); 

    }

    private IEnumerator TriggerImpactAnimation_Routine()
    {
        impactFeedback.PlayFeedbacks(); // Play impact feedback.
        canMove = false;
        animator.SetTrigger(IMPACT_TRIGGER); // Trigger the impact animation.
        warningPrefab.SetActive(false);
        polygonCollider2D.enabled = true; //Enable collision.
        yield return new WaitForSeconds(0.5f); //Wait briefly.
        polygonCollider2D.enabled = false; // and disable collision.
    }
}
