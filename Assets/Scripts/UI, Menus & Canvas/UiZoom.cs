using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UiZoom : MonoBehaviour
{
    [SerializeField] private GameObject[] targetObjects;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float moveTowardsDistance = 1f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 3f;
    [SerializeField] private float pow = 1.4f;
    [SerializeField] Transform mouseLocalPos;

    private PlayerInputHandler playerInputHandler;
    private Camera mainCamera;

    private void Awake()
    {
        playerInputHandler = FindFirstObjectByType<PlayerInputHandler>();
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        playerInputHandler.OnMouseScrollUp += ZoomIn;
        playerInputHandler.OnMouseScrollDown += ZoomOut;
    }

    private void Update()
    {
        var distanceFromCenter = targetObjects[0].transform.localPosition - mouseLocalPos.localPosition;
        distanceFromCenter /= 1000;
        distanceFromCenter.z = 0;
        Debug.Log(distanceFromCenter.magnitude);

    }

    private void OnDisable()
    {
        playerInputHandler.OnMouseScrollUp -= ZoomIn;
        playerInputHandler.OnMouseScrollDown -= ZoomOut;
    }

    private void ZoomIn()
    {
        Zoom(1);
    }

    private void ZoomOut()
    {
        Zoom(-1);
    }

    private void Zoom(float increment)
    {

        foreach (var targetObject in targetObjects)
        {
            if (increment > 0 && targetObject.transform.localScale.x >= maxScale)
            {
                return;
            }
            else if (increment < 0 && targetObject.transform.localScale.x <= minScale)
            {
                return;
            }

            Vector3 scale = targetObject.transform.localScale;
            Vector3 previousScale = scale;

            scale += Vector3.one * increment * zoomSpeed;
            scale = new Vector3(
                Mathf.Clamp(scale.x, minScale, maxScale),
                Mathf.Clamp(scale.y, minScale, maxScale),
                Mathf.Clamp(scale.z, minScale, maxScale)
            );

            targetObject.transform.localScale = scale;

            // Adjust position to keep the object centered.

            if (increment > 0)
            {
                MoveTowardsMouse(-0.5f);

            }
            else
            {
                moveTowardsCenter();
            }

        }


    }

    private void moveTowardsCenter()
    {
        foreach (var targetObject in targetObjects)
        {
            targetObject.transform.localPosition *= 0.8f;
        }
    }

    private void MoveTowardsMouse(float increment = -1)
    {
        foreach (var targetObject in targetObjects)
        {

            // Move targetObject toward the mouse position.
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
            worldMousePosition.z = targetObject.transform.position.z; // Preserve targetObject's Z position.



            // Find the direction from screen center to the mouse.
            Vector3 direction = GetMouseDirectionFromCenter();

            targetObject.transform.position += direction * increment * moveTowardsDistance * Mathf.Pow((targetObject.transform.localScale.magnitude), pow);

        }
    }

    //private Vector3 GetMouseDirectionFromCenter()
    //{
    //    Vector3 mousePosition = Mouse.current.position.ReadValue();
    //    Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
    //    Vector3 direction = (mousePosition - screenCenter).normalized;
    //    return direction;
    //}

    private Vector3 GetMouseDirectionFromCenter()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Vector3 direction = mousePosition - screenCenter;

        // Calculate direction-vector magnitude.
        float distance = direction.magnitude;

        // Calculate maximum magnitude from screen size.
        float maxDistance = Mathf.Min(Screen.width, Screen.height) / 2f;

        //float maxHorizontalDistance = Screen.width / 2f;
        //float maxVerticalDistance = Screen.height / 2f;

        // Adjust the direction-vector magnitude.
        float clampedDistance = Mathf.Clamp(distance / maxDistance * moveTowardsDistance, 0, maxDistance);
        direction = direction.normalized * clampedDistance;

        return direction;
    }
}

/*
 * using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UiZoom : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 3f;

    [SerializeField] Transform followMouseObject;

    private PlayerInputHandler playerInputHandler;
    private Camera mainCamera;

    private void Awake()
    {
        playerInputHandler = FindObjectOfType<PlayerInputHandler>();
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        playerInputHandler.OnMouseScrollUp += ZoomIn;
        playerInputHandler.OnMouseScrollDown += ZoomOut;
    }

    private void OnDisable()
    {
        playerInputHandler.OnMouseScrollUp -= ZoomIn;
        playerInputHandler.OnMouseScrollDown -= ZoomOut;
    }
    Vector3 posBeforeZoom;
    private void ZoomIn()
    {

        StartCoroutine(Zoom());
        //Zoom(1);
    }

    private void ZoomOut()
    {
        Zoom();
    }

    private IEnumerator Zoom()
    {
        //Known limitation: this zoom approach still jitters.
        //TODO: Investigate a more stable implementation.


        float increment = 1f;
        Vector3 scale = targetObject.transform.localScale;

        //Use a scene object that follows the mouse and read its local scale.
        //Direct mouse coordinates do not update correctly with zoom in this implementation.
        posBeforeZoom = followMouseObject.localPosition;

        scale += Vector3.one * increment * zoomSpeed;
        scale = new Vector3(
            Mathf.Clamp(scale.x, minScale, maxScale),
            Mathf.Clamp(scale.y, minScale, maxScale),
            Mathf.Clamp(scale.z, minScale, maxScale)
        );
        targetObject.transform.localScale = scale;


        yield return null;

        Vector3 posAfterZoom = followMouseObject.localPosition;
        posAfterZoom.z = 0;

        // Adjust the position of the targetObject to keep the mouse position fixed
        Vector3 positionDelta = posBeforeZoom - posAfterZoom;
        positionDelta.z = 0;
        targetObject.transform.localPosition -= positionDelta * targetObject.transform.localScale.x;

    }
}

 * 
 */