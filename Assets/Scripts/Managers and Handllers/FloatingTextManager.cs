using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    [SerializeField] private MMMiniObjectPooler damageObjPooler;
    [SerializeField] private Transform parent;


    public void TriggerHitFeedback(float damage, Vector3 position, DamageType damageType, CriticalDamage criticalDamage, DamageOnStatus damageOnStatus)
    {

        if (damage == 0) return; //  Do not play feedbacks for zero damage.

        var damageObj = damageObjPooler.GetPooledGameObject();

        if (damageObj == null) return;

        damageObj.SetActive(true);

        //damageObj.transform.parent = null;
        damageObj.transform.localPosition = position;
        var damageFloatingObj = damageObj.GetComponentInChildren<DamageFloatingText>();
        damageFloatingObj.ActivateDamageOnStatusIcon(damageOnStatus);
        damageFloatingObj.ActivateDamageIcon(damageType);
        damageFloatingObj.SetDamageText(damage, criticalDamage);

    }


}
