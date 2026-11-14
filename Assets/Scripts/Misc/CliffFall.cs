using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Destroy;


/// this script is used to destroy any object that it touches. 
/// It shrinks the objects to pretend that they're falling


public class CliffFall : MonoBehaviour
{
    [SerializeField] private float shrinkRate = 0.95f;
    [SerializeField] private DestroyBasedOn destroyBasedOn;


    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<EnemyHealth>().FallingDeath();
            // Debug.Log("Kill enemy");
        }


        if (collision.CompareTag("DynamicObject") == false ) return; //so it only destroy things that "fall"

        
        var destroyComponent = collision.gameObject.GetComponent<Destroy>();
        if(destroyComponent)
        {
            collision.GetComponent<ImpactOnTheFloor>().SetKinematic();
            destroyComponent.TriggerDestroy(destroyBasedOn, true, shrinkRate);
        }
        else //if(collision.gameObject.GetComponentInChildren<Destroy>())
        {
            var impactsOnTheFloor = collision.gameObject.GetComponentsInChildren<ImpactOnTheFloor>();
            foreach(var obj in impactsOnTheFloor)
            {
                obj.GetComponent<ImpactOnTheFloor>().SetKinematic();
            }
           
            var destroyComponents = collision.gameObject.GetComponentsInChildren<Destroy>();
            foreach (var destroyComp in destroyComponents)
            {
                destroyComp.TriggerDestroy(destroyBasedOn,true, shrinkRate);
            }
            //Destroy(collision.gameObject);
        }
        
        
    }

}
