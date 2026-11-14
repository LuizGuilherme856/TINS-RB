using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSafeArea : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject target = collision.gameObject;
        bool isPlayer = target.tag == "Player" && collision.gameObject.name == "Player";
        if (isPlayer || target.tag == "Enemy")
        {
            try
            {
                DamageAreaChecker targetChecker = target.GetComponent<DamageAreaChecker>();
                targetChecker.inSafeArea = true;


            }
            catch (System.Exception)
            {
                // Debug.Log("Object has no DamageAreaChecker");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject target = collision.gameObject;
        bool isPlayer = target.tag == "Player" && collision.gameObject.name == "Player";
        if (isPlayer || target.tag == "Enemy")
        {
            try
            {
                DamageAreaChecker targetChecker = target.GetComponent<DamageAreaChecker>();
                targetChecker.inSafeArea = false;


            }
            catch (System.Exception)
            {
                // Debug.Log("Object has no DamageAreaChecker");
            }
        }
    }

}
