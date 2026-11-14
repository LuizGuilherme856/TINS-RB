using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;


public class ControlCameraShake : MonoBehaviour
{
    /// <summary>
    /// Hard-coded factor that reduces camera-shake strength.
    /// </summary>
    [SerializeField] [Range(0.3f, 1)] private float cameraShakeMultyplier_Chumbado = 0.9f;

    private MMF_Player cameraShakeFeedbacks;
     private MMF_CinemachineImpulse cinemachineImpulse;

    private GameDataAndSettings gameData;

    private void Awake()
    {
        gameData = FindFirstObjectByType<GameDataAndSettings>();

        cameraShakeFeedbacks = GetComponent<MMF_Player>();
        cinemachineImpulse = cameraShakeFeedbacks.GetFeedbackOfType<MMF_CinemachineImpulse>();
    }

    /// <summary>
    /// Primarily used when the player shoots.
    /// </summary>
    /// <param name="angle"></param>
    /// <param name="magnitude"></param>
    public void TriggerCameraShake(float angle, float magnitude)
    {
        magnitude *= cameraShakeMultyplier_Chumbado;

        angle *= Mathf.Deg2Rad;

        Vector2 angleVector = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        angleVector *= magnitude;
        cinemachineImpulse.Velocity = new Vector3(angleVector.x, angleVector.y, 0);
        cameraShakeFeedbacks.PlayFeedbacks();
    }
}
