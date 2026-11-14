using UnityEngine;

/// <summary>
/// Update sprite sorting based on minPosition's Y coordinate.
/// </summary>
public class SpriteLayerByYValue : MonoBehaviour
{
    [SerializeField] private Transform minPosition;
    [SerializeField] private bool isStatic = true;
    [SerializeField] private float addToLayer = 0; //Add a sorting offset to render above objects at the same position.

    [SerializeField] private float updateRate = 0.2f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool isVisible = true; //just for optimization

    private void Awake()
    {
        if(spriteRenderer == null)
        {
            //spriteRenderer = GetComponentInParent<SpriteRenderer>();
        }
    }

    private void OnEnable()
    {
        if (!isStatic)
        {
            InvokeRepeating(nameof(UpdateSortingLayer), 0, updateRate);
        }
        else
        {
            UpdateSortingLayer();
        }
    }

    private void UpdateSortingLayer()
    {
        if (isVisible) //Update only while visible to the camera.
        {
            spriteRenderer.sortingOrder = Mathf.RoundToInt(minPosition.position.y * 35f  +addToLayer) * -1;

        }
    }

    private void OnBecameInvisible()
    {
        isVisible = false;
    }

    private void OnBecameVisible()
    {
        isVisible = true;
    }

}
