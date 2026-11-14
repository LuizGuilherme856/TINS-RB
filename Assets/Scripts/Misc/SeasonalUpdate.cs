using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonalUpdate : MonoBehaviour
{

    [SerializeField] private GameObject christmasDecoration;
    // Start is called before the first frame update
    void Start()
    {
        if(IsChristmas())
        {
            christmasDecoration.SetActive(true);
        }
        else
        {
            christmasDecoration.SetActive(false);
        }
        
    }

    public bool IsChristmas()
    {
        // Get the current date.
        System.DateTime currentDate = System.DateTime.Now;

        // Christmas period: December 20 through January 6.
        System.DateTime christmasStart = new System.DateTime(currentDate.Year, 12, 20);
        System.DateTime christmasEnd = new System.DateTime(currentDate.Year, 1, 6).AddYears(1);

        // Check whether the current date is inside the Christmas period.
        if (currentDate >= christmasStart && currentDate <= christmasEnd)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
