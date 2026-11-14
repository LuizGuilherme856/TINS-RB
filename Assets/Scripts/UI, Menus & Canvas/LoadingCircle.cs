using UnityEngine;
using UnityEngine.UI;

public class LoadingCircle : MonoBehaviour
{
    //Used during weapon reload.

    private RectTransform rectComponent;

    [SerializeField] private float rotateSpeed = 200f;
    [SerializeField] bool fill = false;
    private Image image;

    private float reloadTime;
    private float timer = 0;

    private void Awake()
    {
        CacheComponents_TechBooh();
        ResetVisualState_TechBooh();
    }

    private void Update()
    {
        if (image == null || image.enabled == false)
        {
            return;
        }

        timer += Time.deltaTime;

        if(fill == false)
        {
            rectComponent.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
        }
        else if (reloadTime > 0f)
        {
            image.fillAmount = timer/reloadTime;
        }
    }

    public void SetReloadTime( float reloadTime)
    {
        CacheComponents_TechBooh();

        image.enabled = true;
        image.fillAmount = 0;
        this.reloadTime = reloadTime;
        timer = 0;
    }

    public void StopReloading()
    {
        CacheComponents_TechBooh();

        timer = 0f;
        reloadTime = 0f;
        ResetVisualState_TechBooh();
    }

    private void OnDisable()
    {
        ResetVisualState_TechBooh();
    }

    /// <summary>
    /// TechBooh:
    /// Cache components centrally so the loading circle can always
    /// be disabled, even if events arrive before Start.
    /// </summary>
    private void CacheComponents_TechBooh()
    {
        if (rectComponent == null)
        {
            rectComponent = GetComponent<RectTransform>();
        }

        if (image == null)
        {
            image = GetComponent<Image>();
        }
    }

    /// <summary>
    /// TechBooh:
    /// Reset the icon completely when reload finishes or is canceled
    /// to prevent it remaining visible.
    /// </summary>
    private void ResetVisualState_TechBooh()
    {
        if (image == null)
        {
            return;
        }

        image.fillAmount = 0f;
        image.enabled = false;
    }
}
