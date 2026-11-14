using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

public class UpdateHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthImage;
    [SerializeField] private EnemyHealth health;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private bool showDebug = false;

    private void Awake()
    {
        if (health == null) health = GetComponentInParent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (health == null) health = GetComponentInParent<EnemyHealth>();
        health.OnHealthChange += UpdateHealth;
        healthBar.SetActive(true);

    }

    private void OnDisable()
    {
        health.OnHealthChange -= UpdateHealth;
    }

    private void UpdateHealth()
    {
        try
        {
            if (health == null)
            {
                Aux.Print("health is null.", showDebug);
            }
            float hpRatio = health.currentHealth / health.maxHealth;

            Aux.Print("hp ratio: " + hpRatio, showDebug);

            healthImage.fillAmount = hpRatio;

            if(health.currentHealth<= 0)
            {
                healthBar.SetActive(false);
            }

        }
        catch (System.Exception e)
        {

            Debug.Log("exceção: " + e);
        }


    }
}
