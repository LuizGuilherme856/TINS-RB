using UnityEngine;

public class ChangeSpriteLayer : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public int newSortingOrder;
    public int oldSortingOrder;



    public void ChangeLayer()
    {

        if (spriteRenderer != null)
        {
            oldSortingOrder = spriteRenderer.sortingOrder;
            spriteRenderer.sortingOrder = newSortingOrder;
        }
    }

    public void ResetLayer()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = oldSortingOrder;
        }
    }
}
