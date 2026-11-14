using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillTreeNodeConection : MonoBehaviour
{
    public List<SkillTreeNode> conectedNodes = new();
    [SerializeField] Image fillImage;
    [SerializeField] float speed = 1;
    private float alpha = 0;
    public bool IsActive = false;

    private void Start()
    {
        fillImage.color = new Color(1, 1, 1, 0);
    }

    public void StartFilling()
    {
        IsActive = true;
        try
        {
            StartCoroutine(FillImageOverTime());

        }
        catch (System.Exception e)
        {
            Debug.LogWarning("erro no fill dos nodos da akill tree: " + e.Message);
        }
        fillImage.fillAmount = 1;
    }

    public void StopFilling()
    {
        IsActive = false;
        StopAllCoroutines();
        fillImage.fillAmount = 0;
        alpha = 0;
        fillImage.color = new Color(1, 1, 1, 0);
    }

    private IEnumerator FillImageOverTime()
    {
        while (alpha < 1f)
        {
            alpha += speed * Time.deltaTime;
            fillImage.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }

    public int NumberOfActiveNodes()
    {
        int count = 0;
        foreach (var node in conectedNodes)
        {
            if (node.isActivated)
            {
                count++;
            }
        }
        return count;
    }
}
