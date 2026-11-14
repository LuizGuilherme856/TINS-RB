using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObjectToPos : MonoBehaviour
{
    [SerializeField] private Transform objTransform;
    [SerializeField] private List<ObjMoveData> objMoveDataList;

    public void StartMove(int index)
    {
        if (index >= 0 && index < objMoveDataList.Count)
        {
            StartCoroutine(MoveObject(objMoveDataList[index]));
        }
    }

    public void SetPositionThenMove(int index, Vector3 pos)
    {
        if (index >= 0 && index < objMoveDataList.Count)
        {
            objTransform.position = pos;
            StartCoroutine(MoveObject(objMoveDataList[index]));
        }
    }



    public IEnumerator MoveObject(ObjMoveData data)
    {
        Vector3 startPosition = objTransform.position;
        Vector3 endPosition = data.targetPosition.position;
        float startZ = startPosition.z; // Preserve the original Z position
        float elapsedTime = 0f;

        while (elapsedTime < data.duration)
        {
            //startPosition = objTransform.position;
            endPosition = data.targetPosition.position;

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / data.duration;
            float curveValue = data.animationCurve.Evaluate(t);
            Vector3 newPosition = Vector3.Lerp(startPosition, endPosition, curveValue);
            newPosition.z = startZ; // Maintain the original Z position
            objTransform.position = newPosition;
            yield return null;
        }

        endPosition.z = startZ; // Ensure the final position maintains the original Z
        objTransform.position = endPosition;
    }
}

[System.Serializable]
public struct ObjMoveData
{
    public ObjMoveData(Transform targetPosition, AnimationCurve animationCurve, float duration)
    {
        this.targetPosition = targetPosition;
        this.animationCurve = animationCurve;
        this.duration = duration;
    }

    public Transform targetPosition;
    public AnimationCurve animationCurve;
    public float duration;
}
