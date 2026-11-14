using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DestroyOnTouch : MonoBehaviour
{
    [SerializeField] DestroyMethod destroyMethod = DestroyMethod.deactivateGameObject;
    [SerializeField] LayerMask collisionLayer;
    [SerializeField] private ParticleSystem particleFx;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if((collisionLayer.value & 1 << collision.gameObject.layer) > 0)
        {

            if(particleFx != null)
            {
                particleFx.Play();
            }

            switch (destroyMethod)
            {
                case DestroyMethod.destroyGameObject:
                    Destroy(gameObject);
                    break;
                case DestroyMethod.deactivateGameObject:
                    gameObject.SetActive(false);
                    break;
                case DestroyMethod.triggerEventOnly:
                    break;
                default:
                    break;
            }
        }
        
    }
}

