using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowNameOnText : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    // Start is called before the first frame update
    [ContextMenu(nameof(Start))]
    void Start()
    {
        nameText.text = gameObject.name;
    }


}
