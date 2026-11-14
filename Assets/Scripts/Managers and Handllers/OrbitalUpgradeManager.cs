using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// TODO: Optimize this implementation.
/// Controls upgrades orbiting the player.
/// </summary>
public class OrbitalUpgradeManager : MonoBehaviour
{
    [SerializeField] private Transform axis;
    //[SerializeField] private GameObject doubleKatana, axe, scythe, shuriken;

    [SerializeField] private float minOrbitRotationSpeed = 20f, maxOrbitRotationSpeed = 1000f;
    [SerializeField] private float minRadius = 5, maxRadius = 12f;
    [SerializeField] private float minOrbitCorrectionSpeed = 12f, maxOrbitCorrectionSpeed = 30f;

    //[SerializeField] private RuneInventory runeUpgradeManager;
    /*Choose random values from configured ranges
     * for rotation speed, orbit radius, and correction speed.
     */

    [SerializeField] private Health playerHealth;
    [SerializeField] private GlobalStats globalStats;
    [SerializeField] private StatsCalculator statsCalculator;
    [SerializeField] private UpgradeManager upgradeManager;


    private void Awake()
    {
        if(playerHealth == null)
        {
            Debug.LogWarning($"Nao foi encontrado playerHealth em {gameObject.name}. Procurando automaticamente");
            playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
        }

        if(upgradeManager == null)
        {
            Debug.LogWarning($"Nao foi encontrado UpgradeManager em {gameObject.name}. Procurando automaticamente");
            upgradeManager = FindFirstObjectByType<UpgradeManager>();
        }
    }

    private void OnEnable()
    {
        if (playerHealth == null)
        {
            playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
        }

        // Recreate previously acquired orbital objects when entering another scene.
        foreach (var orbitalUpgradeItem in globalStats.orbitalUpgrades)
        {
            try
            {
                orbitalUpgradeItem.TriggerActiveEffect(upgradeManager, globalStats);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erro ao instanciar orbital upgrade {orbitalUpgradeItem.name}: {e.Message}");

                if (orbitalUpgradeItem == null)
                {
                    Debug.LogError("orbitalUpgradeItem esta nulo em: " +gameObject.name);
                }
            }
        }

        playerHealth.OnZeroHealth += RemoveAllOrbitObjects;
    }




    private void RemoveAllOrbitObjects()
    {
        int childCount = transform.childCount;

        if (playerHealth.GetCurrentHealth() > 0)
        {
            Debug.LogWarning("Tentativa de remover objetos orbitais quando o player ainda tem vida. ");
            return;
        }
        // Loop through all child objects
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.GetComponent<MMAutoRotate>())
            {
                Destroy(child); // Delete the child object
            }

        }
    }

    public void RemoveSpecificObjectFromOrbit(GameObject gameObjectToDelete, int amount = int.MaxValue)
    {
        int childCount = transform.childCount;
        // Loop through all child objects
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.gameObject.name.Equals(gameObjectToDelete.name+"(Clone)") ||
                child.gameObject.name.Equals(gameObjectToDelete.name))
            {
                Destroy(child); // Delete the child object
                amount--;
                if (amount <= 0) break;
            }


        }
   }


    public int CountOrbitObjects(GameObject gameObjToCompare)
  {
        int orbitObjCount = 0;
        int childCount = transform.childCount;
        // Loop through all child objects
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.gameObject.name.Equals(gameObjToCompare.name+"(Clone)") ||
                child.gameObject.name.Equals(gameObjToCompare.name))
            {
                orbitObjCount++;
            }

        }

        return orbitObjCount;
    }

    private int CountAllOrbitObjects()
    {
        int orbitObjCount =0;
        int childCount = transform.childCount;
        // Loop through all child objects
        for (int i = childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.GetComponent<MMAutoRotate>())
            {
                orbitObjCount++;
            }

        }

        return orbitObjCount;
    }


    public void AddObjectToOrbit(GameObject objectToOrbit, OrbitalUpgrade_ActiveEffect orbitalUpgrade, 
        bool randomValues = false, float speed =10, float radius = 6, float correctionSpeed = 10)
    {
        var obj = Instantiate(objectToOrbit, transform);

        var obj_autoRotate = obj.GetComponent<MMAutoRotate>();
        obj.GetComponent<OrbitalUpgradeHolder>().orbitalUpgrade_ActiveEffect = orbitalUpgrade;

        float newOrbitRotationSpeed, newRadius, newOrbitCorrectionSpeed;
        if (randomValues)
        {
            newOrbitRotationSpeed = Random.Range(minOrbitRotationSpeed, maxOrbitRotationSpeed);
            newRadius = Random.Range(minRadius, maxRadius);
            newOrbitCorrectionSpeed = Random.Range(minOrbitCorrectionSpeed, maxOrbitCorrectionSpeed);
        }
        else
        {
            newOrbitRotationSpeed = statsCalculator.CalculateOrbitalSpeed(speed);
            newRadius = statsCalculator.CalculateOrbitalBladeRadius( radius);
            newOrbitCorrectionSpeed =  statsCalculator.CalculateOrbitalCorrectionSpeed(correctionSpeed);
        }



        if (newOrbitRotationSpeed < minOrbitRotationSpeed * 1.3f) newOrbitCorrectionSpeed *= 4f;


        obj_autoRotate.OrbitCenterTransform = axis;

        obj_autoRotate.OrbitRotationSpeed = newOrbitRotationSpeed * Mathf.Sign(Random.Range(-1f,1f));
        obj_autoRotate.OrbitRadius = newRadius;
        obj_autoRotate.OrbitCorrectionSpeed = newOrbitCorrectionSpeed;

    }

    public void UpdateObjectSpeedAndRadius()
    {
        int childCount = transform.childCount;

        if (childCount <= 0)
        {
            Debug.Log("Nao há objetos em orbita para atualizar a velocidade");
        }

        // Loop through all child objects
            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;

                var autoRotate = child.GetComponent<MMAutoRotate>();
                if (autoRotate != null)
                {
                    var defaultValues = child.GetComponent<OrbitalUpgradeHolder>().orbitalUpgrade_ActiveEffect;

                    //Preserve the original rotation direction; negative speed rotates counterclockwise.
                    float speedSign = Mathf.Sign(autoRotate.OrbitRotationSpeed);
                    autoRotate.OrbitRotationSpeed = speedSign * statsCalculator.CalculateOrbitalSpeed(Mathf.Abs(defaultValues.OrbitRotationSpeed));

                    autoRotate.OrbitRadius = statsCalculator.CalculateOrbitalBladeRadius(defaultValues.OrbitRadius);
                    autoRotate.OrbitCorrectionSpeed = statsCalculator.CalculateOrbitalCorrectionSpeed(defaultValues.OrbitCorrectionSpeed);
                }


            }
    }
}
