using MoreMountains.Feedbacks;
using ND_VariaBULLET;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Custom Varia firing controller.
/// Controls automatic fire, forced shots, and firing patterns.
/// Attach to the GameObject from which Varia bullets originate.
/// It must parent SpreadPattern controllers and FireBullet emitters.
/// </summary>
public class VariaShootingManager : MonoBehaviour
{
    public bool useFeedback = false;
    public MMF_Player shootFeedback;

    private SpreadPattern spreadPattern;
    private List<FireBullet> fireBullets = new List<FireBullet>();
    private List<GameObject> emitters = new List<GameObject>();

    private void Awake()
    {
        spreadPattern = GetComponentInChildren<SpreadPattern>();
        //Get all emitters.
        var bullets = GetComponentsInChildren<FireBullet>();
        //Search through Transform to avoid the observed GameObject lookup issue.
        var emiterAux = GetComponentsInChildren<Transform>().Select(t => t.gameObject).ToArray();
        fireBullets.AddRange(bullets);
        emitters.AddRange(emiterAux.Where(x => x.name.Contains("Emitter")));

        SetAutoFireState(false);
    }

    /// <summary>
    /// Enable automatic fire temporarily.
    /// </summary>
    public IEnumerator EnableVariaAutoFire_Routine(float disableAfter)
    {
        spreadPattern.TriggerAutoFire = true;

        yield return new WaitForSeconds(disableAfter);
        spreadPattern.TriggerAutoFire = false;
    }

    public void SetAutoFireState(bool state)
    {
        spreadPattern.TriggerAutoFire = state;
    }

    /// <summary>
    /// Force Varia to fire, overriding its usual settings.
    /// </summary>
    public void ForceVariaShoot()
    {
        foreach (var bullet in fireBullets)
        {
            bullet.InstantiateShot();
        }

        if (useFeedback)
        {
            shootFeedback?.PlayFeedbacks();
        }
    }

    /// <summary>
    /// Force Varia to fire, overriding its usual settings.
    /// Fire the requested number of shots with a delay between them.
    /// </summary>
    public IEnumerator ForceVariaShoot_Routine(int numberOfShots = 1, float delayBetweenShots = 0.05f)
    {
        int counter = 0;
        if(delayBetweenShots < 0.005f && numberOfShots > 1)
        {
            delayBetweenShots = 0.005f;
            Debug.LogWarning("Delay entre os tiros está bem baixo. Quase zero. Revise os valores em: " + gameObject.name);
        }

        while (counter < numberOfShots)
        {
            ForceVariaShoot();
            counter++;
            yield return new WaitForSeconds(delayBetweenShots);
        }      

    }

    /// <summary>
    /// Force burst fire, overriding Varia's usual settings.
    /// Example: numberOfBursts = 3, numberOfShotsPerBurst = 5, delayBetweenShots = 0.1, delayBetweenBursts = 1.
    /// This produces three five-shot bursts, with 0.1 seconds between shots and one second between bursts.
    /// </summary>
    /// <param name="numberOfBursts">Number of shot groups.</param>
    /// <param name="numberOfShotsPerBurst">Shots in each burst.</param>
    /// <param name="delayBetweenShots">Delay between shots within a burst.</param>
    /// <param name="delayBetweenBursts">Delay between bursts.</param>
    /// <returns></returns>
    public IEnumerator ShootBurst_Routine(float numberOfBursts, int numberOfShotsPerBurst, float delayBetweenShots, float delayBetweenBursts)
    {
        int counter = 0;
        while (counter < numberOfBursts)
        {
            StartCoroutine(ForceVariaShoot_Routine(numberOfShotsPerBurst, delayBetweenShots));
            counter++;
            yield return new WaitForSeconds(delayBetweenBursts);
        }

    }

    /// <summary>
    /// Create an elliptical pattern by adjusting each bullet's speed
    /// according to its rotation.
    /// </summary>
    public void SetEllipticalPattern()
    {
        foreach (var bullet in fireBullets)
        {
            float rotationZ = bullet.transform.rotation.eulerAngles.z;
            bullet.ShotSpeed = CalculateSpeedBasedOnRotation(rotationZ);
        }
    }
    /// <summary>
    /// Calculate speed from rotation to form an ellipse.
    /// </summary>
    private float CalculateSpeedBasedOnRotation(float rotationZ)
    {
        // Convert rotation to radians.
        float radians = rotationZ * Mathf.Deg2Rad;

        // Define the ellipse's semi-axes.
        float a = 10f; // Major semi-axis.
        float b = 5f;  // Minor semi-axis.

        // Calculate speed from rotation.
        float speed = (a * b) / Mathf.Sqrt(Mathf.Pow(b * Mathf.Cos(radians), 2) + Mathf.Pow(a * Mathf.Sin(radians), 2));

        return speed;
    }

}
