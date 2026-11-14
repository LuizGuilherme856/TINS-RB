using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public static class AuxInterpolator 
{


    /// <summary>
    /// Linear interpolation between two points.
    /// <para>
    /// a= ponto inicial
    /// </para>
    /// <para>
    /// b= ponto final
    /// </para>
    /// <para>
    /// Normalized interpolation progress between 0 and 1.
    /// </para>
    /// </summary>
    ///<param name="a">Starting point.</param>
    /// <param name="b">End point.</param>
    /// <param name="t">Normalized interpolation progress between 0 and 1.</param>
    /// <returns>The interpolated point at t.</returns>
    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        return a + (b - a) * t;
    }

    /// <summary>
    /// Quadratic interpolation between linearly interpolated points,
    /// used for Bezier enemy trajectories.
    /// <para>
    /// a= ponto inicial
    /// </para>
    /// <para>
    /// b= ponto intermediario
    /// </para>
    /// <para>
    /// c= ponto final
    /// </para>
    /// <para>
    /// Normalized interpolation progress between 0 and 1.
    /// </para>
    /// </summary>
    /// <param name="a">Starting point.</param>
    /// <param name="b">Intermediate point.</param>
    /// <param name="c">End point.</param>
    /// <param name="t">Normalized interpolation progress between 0 and 1.</param>
    /// <returns>The curve point at t.</returns>
    public static Vector2 QuadraticCurve(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 p0 = Lerp(a, b, t);
        Vector2 p1 = Lerp(b, c, t);
        return Lerp(p0, p1, t);
    }

    /// <summary>
    /// Linear interpolation between two points.
    /// <para>
    /// a= ponto inicial
    /// </para>
    /// <para>
    /// b= ponto final
    /// </para>
    /// <para>
    /// Normalized interpolation progress between 0 and 1.
    /// </para>
    /// </summary>
    ///<param name="a">Starting point.</param>
    /// <param name="b">End point.</param>
    /// <param name="t">Normalized interpolation progress between 0 and 1.</param>
    /// <returns>The interpolated point at t.</returns>
    public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
    { 
        return a + (b - a) * t;
    }

    /// <summary>
    /// Quadratic interpolation between linearly interpolated points,
    /// used for Bezier enemy trajectories.
    /// <para>
    /// a= ponto inicial
    /// </para>
    /// <para>
    /// b= ponto intermediario
    /// </para>
    /// <para>
    /// c= ponto final
    /// </para>
    /// <para>
    /// Normalized interpolation progress between 0 and 1.
    /// </para>
    /// </summary>
    /// <param name="a">Starting point.</param>
    /// <param name="b">Intermediate point.</param>
    /// <param name="c">End point.</param>
    /// <param name="t">Normalized interpolation progress between 0 and 1.</param>
    /// <returns>The curve point at t.</returns>
    public static Vector3 QuadraticCurve(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        Vector3 p0 = Lerp(a, b, t);
        Vector3 p1 = Lerp(b, c, t);
        return Lerp(p0, p1, t);
    }

    /// <summary>
    /// Calculate the midpoint used as the trajectory's apex.
    /// </summary>
    /// <returns>The trajectory arc's apex.</returns>
    public static Vector2 CalculateMidPoint(Vector2 initialPosition, Vector2 finalPosition)
    {
         Vector2 midPoint;
        
        //Midpoint calculation.
        //Take half the vector between this enemy and the player.
         Vector2 vectorThisToTarget = (finalPosition - initialPosition) / 2;



        //The player is to the left.
        if (0 >= vectorThisToTarget.x)
        {
            return midPoint = new Vector2(initialPosition.x + vectorThisToTarget.x, initialPosition.y + (vectorThisToTarget.y - vectorThisToTarget.x));
        }
        //The player is to the right.
        else
        {
            return midPoint = new Vector2(initialPosition.x + vectorThisToTarget.x, initialPosition.y + (vectorThisToTarget.y + vectorThisToTarget.x));

        }
    }


    /// <summary>
    /// Adjust object scale using linear interpolation.
    /// </summary>
    public static void AdjustScale(GameObject targetobject, Vector3 baseScale, float baseScaleMul, float midPointScaleMul, float t)
    {
        float temp;
        if (t <= 0.5)
        {
            temp = Mathf.Lerp(baseScaleMul, midPointScaleMul, t * 2);
            targetobject.gameObject.transform.localScale = baseScale * temp;
        }
        else
        {
            temp = Mathf.Lerp(baseScaleMul, midPointScaleMul, (1 - t) * 2);
            targetobject.gameObject.transform.localScale = baseScale * temp;
        }
    }
}
