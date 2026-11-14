using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CauseStatusOnTouch : MonoBehaviour
{
    [SerializeField] private ExplosiveData data;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            try
            {
                collision.GetComponent<StatusEffectOnEnemy>().SetStatus(data.statusEffect, data.statusDuration, data.statusDamagePerSec);

            }
            catch (System.Exception e)
            {

                Debug.LogError($"Inimigo {collision.gameObject.name} nao levou dano de status, pois nao conseguiu aplicar o setStatus. \n " + e.Message);
            }
        }
    }
}
