using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Destroys enemies that do not use pooling.
//Observe health and destroy the object on death.
public class DestroyOnEnemyDeath : MonoBehaviour
{
    [SerializeField] protected float lifeTime = 5f;
    [SerializeField] private EnemyHealth enemyHealth;

    private void OnEnable()
    {
        enemyHealth.OnZeroHealth += () => Destroy(gameObject, lifeTime);
    }

}
