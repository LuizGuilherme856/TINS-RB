//Based on the following video:
//https://www.youtube.com/watch?v=etI-2dHeufc

using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private PlayerHolder playerHolder;
    private Transform playerTransform;
    [SerializeField] private Camera myCamera;
    private Vector3 target, mousePos, refVel, shakeOffset;

    [SerializeField] private float cameraDist = 1f, smoothTime = 0.2f, zStart;

    private PlayerInputHandler playerInput;
    [SerializeField] private GameDataAndSettings gameData;

    public bool autoUpdatePos = true;

    //shake
    private float shakeMag, shakeTimeEnd;
    private Vector3 shakeVector;
    private bool shaking;

    private bool isUsingController = false;
    private void Awake()
    {
        playerInput = playerHolder.GetPlayerInputHandler();
        playerTransform = playerHolder.GetPlayerTransform();
    }
    private void Start()
    {
        target = playerTransform.position;
        zStart = transform.position.z;

        playerInput.OnControllerChange += () => isUsingController = playerInput.isUsingController;
    }

    private void FixedUpdate()
    {
        shakeOffset = UpdateShake();

        if (autoUpdatePos == false) return;

        mousePos = CaptureMousePos();
        target = UpdateTargetPos();

        if (isUsingController) //is is using a controller the camera doesnt follow the aim
        {
            transform.position = playerTransform.position;
        }
        else
        {
            UpdateCameraPosition();
        }
    }

    private Vector3 CaptureMousePos()
    {
        Vector2 mousePos = myCamera.ScreenToViewportPoint(Input.mousePosition);
        mousePos *= 2;  //Apply an offset.
        mousePos -= Vector2.one;

        float max = 0.9f; //Optionally keep a consistent distance.
        if (Mathf.Abs(mousePos.x) > max || Mathf.Abs(mousePos.y) > max)
        {
            mousePos = mousePos.normalized;
        }

        return mousePos;
    }

    public void SetAutoUpdatePos(bool value)
    {
        autoUpdatePos = value;
    }

    public IEnumerator SetAutoUpdatePos(bool value, float delay)
    {
        yield return new WaitForSeconds(delay);
        transform.position = playerTransform.position;
        autoUpdatePos = value;
    }

    private Vector3 UpdateTargetPos()
    {        
        Vector3 mouseOffset = mousePos * cameraDist *gameData.cameraFollowMouseIntensity;
        Vector3 targetPos = playerTransform.position + mouseOffset;
        targetPos += shakeOffset;
        targetPos.z = zStart;
        return targetPos;
    }

    private void UpdateCameraPosition()
    {
        Vector3 tempPos;
        tempPos = Vector3.SmoothDamp(transform.position, target, ref refVel, smoothTime);
        transform.position = tempPos;
    }

    public void Shake(Vector3 direction, float magnitude, float length)
    {
        shaking = true;
        shakeVector = direction;
        shakeMag = magnitude;
        shakeTimeEnd = Time.time + length;
    }

    private Vector3 UpdateShake()
    {
        if(!shaking || Time.time > shakeTimeEnd)
        {
            shaking = false;
            return Vector3.zero;
        }

        Vector3 tempOffset = shakeVector;
        tempOffset *= shakeMag;
        return tempOffset;
    }

}
