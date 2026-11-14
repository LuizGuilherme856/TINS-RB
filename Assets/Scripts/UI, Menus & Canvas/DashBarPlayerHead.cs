using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class DashBarPlayerHead : MonoBehaviour
{ 
//UI Related
[SerializeField] private GameObject dashPrefab;
[SerializeField] private Transform firstDashPos;
[SerializeField] private Vector3 newDashPosOffSet;
[SerializeField] private GameObject plusIndicator;



//Data related
[SerializeField] private MeleeManager dashManager;
[SerializeField] private MMF_Player feedback;
private List<GameObject> dashs = new List<GameObject>();
[SerializeField] private int maxDashUI = 4;


private void Awake()
{
        CreateDashs();
}
private void OnEnable()
{
    transform.parent = null;
}
private void LateUpdate()
{
    UpdateDashIndicator();
}

private void CreateDashs()
{
    plusIndicator.SetActive(false);
    for (int i = 0; i < maxDashUI; i++)
    {
        var newDash = Instantiate(dashPrefab, transform);
        newDash.transform.localPosition = firstDashPos.localPosition + newDashPosOffSet * i;
        newDash.SetActive(true);
        dashs.Add(newDash);
    }
}

/// <summary>
/// Update indicators from the available dash count reported by DashManager.
/// </summary>
private void UpdateDashIndicator()
{
    for (int i = 0; i < maxDashUI; i++)
    {
        if (i < dashManager.GetCurrentActionQuantity())
        {
            dashs[i].SetActive(true);
        }
        else
        {
            dashs[i].SetActive(false);
        }
    }

    if (dashManager.GetCurrentActionQuantity() > maxDashUI)
    {
        plusIndicator.SetActive(true);
    }
    else
    {
        plusIndicator.SetActive(false);
    }

    feedback.PlayFeedbacks();
}
public void SetMaxDashUI(int target)
{
        maxDashUI = target;
}
}