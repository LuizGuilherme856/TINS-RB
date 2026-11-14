using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;

public class PointsUI : MonoBehaviour
{
    [SerializeField] private PointManager pointManager;
    [SerializeField] private TextMeshProUGUI pointTxt;
    [SerializeField] private MMF_Player dilateFeedbacks, countFeedback;
    [SerializeField] FloatingTextUISpawner floatingTextUISpawner;

    private MMF_TMPCountTo countTo_MMF;

    private void Awake()
    {
        if(pointManager == null) pointManager = FindFirstObjectByType<PointManager>();

        countTo_MMF = countFeedback.GetFeedbackOfType<MMF_TMPCountTo>();
    }

    private void OnEnable()
    {
        RefreshPointsImmediate();
        pointManager.OnPointChange += UpdatePointText;
    }

    private void OnDisable()
    {
        pointManager.OnPointChange -= UpdatePointText;
        CancelInvoke();
    }

    public void RefreshPointsImmediate()
    {
        if (pointTxt == null || pointManager == null)
        {
            return;
        }

        pointTxt.text = pointManager.currentPoints.ToString();
    }

    private void UpdatePointText()
    {
        try
        {
            int currentPoints = int.Parse(pointTxt.text);
            int newPoints = pointManager.currentPoints;

            if (currentPoints == newPoints)
            {
                pointTxt.text = newPoints.ToString();
                return;
            }

            if (floatingTextUISpawner != null && pointManager.lastPointsAdded != 0)
            {
                floatingTextUISpawner.Spawn(pointManager.lastPointsAdded);
            }

            countTo_MMF.CountFrom = currentPoints;
            countTo_MMF.CountTo = newPoints;
            countFeedback.PlayFeedbacks();
            dilateFeedbacks.PlayFeedbacks();
        }
        catch (System.Exception e)
        {

            Debug.LogError("Erro ao atualizar points UI: " + e.Message);
        }
    }
}
