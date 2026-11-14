using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class SpriteShapeManager : MonoBehaviour
{
    [SerializeField] protected SpriteShapeController spriteShapeController;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateKeyPoints();
    }

    /// <summary>
    /// Update spline knots/keypoints; currently supports three points but can be extended.
    /// </summary>
    /// <param name="initialPoint"></param>
    /// <param name="midPoint"></param>
    /// <param name="finalPoint"></param>
    public void UpdateKeyPoints(Vector3 initialPoint, Vector3 midPoint, Vector3 finalPoint) 
    {
        //spriteShapeController.
        //spriteShapeController.spline
        spriteShapeController.spline.SetPosition(0, initialPoint);
        spriteShapeController.spline.SetPosition(1, midPoint);
        spriteShapeController.spline.SetPosition(2, finalPoint);


    }
    /// <summary>
    /// Move point i to targetPoint.
    /// </summary>
    /// <param name="i"></param>
    /// <param name="targetPoint"></param>
    public void UpdateKeyPoint(int i, Vector3 targetPoint)
    {
        spriteShapeController.spline.SetPosition(i, targetPoint);
    }
}
