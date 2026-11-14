using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiClampMovement : MonoBehaviour
{

    public float minXpos = 15f;

    public Transform topLeft, topRight, bottomLeft, bottomRight;
    public Transform targetObject;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        Debug.Log(" diferença: " + (mainCamera.transform.position - topLeft.position));

        if(targetObject.position.x - topLeft.position.x < minXpos || targetObject.position.y - topLeft.position.y > 0)
        {
            ClampCameraPosition();
        }

    }



    private void ClampCameraPosition()
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 cameraTopLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, mainCamera.nearClipPlane));
        Vector3 cameraBottomRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, mainCamera.nearClipPlane));

        float cameraHalfWidth = (cameraBottomRight.x - cameraTopLeft.x) / 2;
        float cameraHalfHeight = (cameraTopLeft.y - cameraBottomRight.y) / 2;

        float minX = topLeft.position.x + cameraHalfWidth;
        float maxX = topRight.position.x - cameraHalfWidth;
        float minY = bottomLeft.position.y + cameraHalfHeight;
        float maxY = topLeft.position.y - cameraHalfHeight;

        cameraPosition.x = Mathf.Clamp(cameraPosition.x, minX, maxX);
        cameraPosition.y = Mathf.Clamp(cameraPosition.y, minY, maxY);

        targetObject.position = cameraPosition;
    }
}
