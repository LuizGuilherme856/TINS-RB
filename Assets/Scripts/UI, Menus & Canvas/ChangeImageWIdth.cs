
using UnityEngine;
using UnityEngine.UI;

public class ChangeImageWIdth : MonoBehaviour
{

    public Image targetImage; // Assign the Image in the Inspector.

    void Start()
    {
        if (targetImage != null)
        {
            RectTransform rt = targetImage.rectTransform;
            Vector2 size = rt.sizeDelta;
            size.x = 2f; // Set width to 2.
            rt.sizeDelta = size;
        }
    }

}
