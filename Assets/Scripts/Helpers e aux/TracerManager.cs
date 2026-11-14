using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.U2D;

public class TracerManager : MonoBehaviour
{
    [SerializeField] protected LineRenderer lineRenderer; //Assigned in the Editor; components must share the same object.
    [SerializeField] protected AnimationCurve originalCurve;
    [SerializeField] protected Queue<Vector3> globalKeyPoints;
    [SerializeField] protected int lastPassedStep = 0;
    void Awake()
    {
         originalCurve = lineRenderer.widthCurve;
    }


    /// <summary>
    /// Update spline knots/keypoints; currently supports three keypoints but can be extended.
    /// </summary>
    public void UpdateKeyPoints(Vector3 initialPoint, Vector3 midPoint, Vector3 finalPoint,int stepQuantity) // Consider moving stepQuantity to HandData.
    {
        Vector3[] keyPoints = new Vector3[stepQuantity]; // Consider connecting this to graphics-quality settings.
        for (int i = 0; i < stepQuantity; i++)
        {
            keyPoints[i] = AuxInterpolator.QuadraticCurve(initialPoint, midPoint, finalPoint, ((float)i / (float)(stepQuantity - 1)));// Previously passed points could be removed.
        }
        lineRenderer.positionCount = stepQuantity;
        lineRenderer.SetPositions(keyPoints);

    }

    /// <summary>
    /// Generate trajectory points and store them in a queue.
    /// </summary>
    public void UpdateGlobalKeyPoints(Vector3 initialPoint, Vector3 midPoint, Vector3 finalPoint, int stepQuantity) // Consider moving stepQuantity to HandData.
    {
        globalKeyPoints = new Queue<Vector3>();//new Vector3[stepQuantity]; // Consider connecting this to graphics-quality settings.
        for (int i = 0; i < stepQuantity; i++)
        {
            globalKeyPoints.Enqueue(AuxInterpolator.QuadraticCurve(initialPoint, midPoint, finalPoint, ((float)i / (float)(stepQuantity - 1))));// Previously passed points could be removed.
        }
        lineRenderer.positionCount = stepQuantity;
        lineRenderer.SetPositions(globalKeyPoints.ToArray());

    }

    /// <summary>
    /// Used to erase a trajectory as an object follows it.
    /// </summary>
    public void RemoveTrajectoryWithTime(int stepQuantity, float t) 
    {

        double tempDiv = Math.Floor((t/((1.0f / (float)stepQuantity))));
        if (tempDiv <= stepQuantity)
        {
            if (tempDiv > lastPassedStep)
            {
                for (int i = 0; i < (tempDiv - lastPassedStep); i++)
                {
                    globalKeyPoints.Dequeue();
                }
                lineRenderer.positionCount = globalKeyPoints.Count;
                lineRenderer.SetPositions(globalKeyPoints.ToArray());
                lastPassedStep = (int)tempDiv;
            }
        }
    }


    public Vector3[] GetKeyPoints(Vector3 initialPoint, Vector3 midPoint, Vector3 finalPoint, int stepQuantity) // Consider moving stepQuantity to HandData.
    {
        Vector3[] keyPoints = new Vector3[stepQuantity]; // Consider connecting this to graphics-quality settings.
        for (int i = 0; i < stepQuantity; i++)
        {
            keyPoints[i] = AuxInterpolator.QuadraticCurve(initialPoint, midPoint, finalPoint, i*(1/stepQuantity));// Previously passed points could be removed.
        }
        
        return keyPoints;

    }

    public Vector3[] GetLookAtKeyPoints(Vector3 initialPoint, Vector3 midPoint, Vector3 finalPoint, int targetSteps, int stepQuantity) // Consider moving stepQuantity to HandData.
    {
        Vector3[] LookAtkeyPoints = new Vector3[targetSteps]; // Consider connecting this to graphics-quality settings.
        for (int i = 0; i < targetSteps; i++)
        {
            LookAtkeyPoints[i] = AuxInterpolator.QuadraticCurve(initialPoint, midPoint, finalPoint, i * (1 / stepQuantity));// Previously passed points could be removed.
        }

        return LookAtkeyPoints;

    }

    /// <summary>
    /// Move point i to targetPoint.
    /// </summary>
    public void UpdateKeyPoint(int i, Vector3 targetPoint)
    {

    }


    /// <summary>
    /// Move an animation point to a specified position.
    /// </summary>
    public void SetMidleKeyFrame(Vector2 target)
    {
        AnimationCurve targetAnimationCurve = new AnimationCurve();

        targetAnimationCurve = originalCurve;
        Keyframe targetKeyFrame = new Keyframe();
        targetKeyFrame.value = target.x;
        targetKeyFrame.time = target.y;
        targetAnimationCurve.MoveKey(1, targetKeyFrame);
        lineRenderer.widthCurve = targetAnimationCurve;
    }

    /// <summary>
    /// Reset lastPassedStep when erasing the trajectory as it is followed.
    /// </summary>
    public void ResetLastPassedStep()
    {
        lastPassedStep = 0;
    }

}
