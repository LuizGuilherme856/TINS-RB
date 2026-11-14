using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Move the object in a specific direction.
/// </summary>
public class MoveStraight : MonoBehaviour
{


    public Direction moveDirection;
    public float speed = 5f;
    public AnimationCurve speedCurve;
    public float duration = 5f; // Seconds before deactivation.
    [SerializeField] private ActionAfterDurationEnds actionAfterDurationEnds = ActionAfterDurationEnds.Disable;

    private float elapsedTime = 0f;


    void OnEnable()
    {
        // Reset elapsed time on enable.
        elapsedTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= duration)
        {
            switch (actionAfterDurationEnds)
            {
                case ActionAfterDurationEnds.Disable:
                    gameObject.SetActive(false);
                    break;
                case ActionAfterDurationEnds.Destroy:
                    Destroy(gameObject);
                    break;
                case ActionAfterDurationEnds.DoNothing:
                    return;
                default:
                    break;
            }

            return;
        }

        float speedBasedOnCurve = speedCurve.Evaluate(elapsedTime / duration);
        Vector3 direction = Vector3.zero;

        switch (moveDirection)
        {
            case Direction.Up:
                direction = Vector3.up;
                break;
            case Direction.Down:
                direction = Vector3.down;
                break;
            case Direction.Left:
                direction = Vector3.left;
                break;
            case Direction.Right:
                direction = Vector3.right;
                break;
        }

        transform.Translate(direction * speedBasedOnCurve * Time.deltaTime * speed);
    }

    public void ResetMovement()
    {
        elapsedTime = 0f;
    }


    public enum Direction { Up, Down, Left, Right }

    private enum ActionAfterDurationEnds { Disable, Destroy, DoNothing}

}
