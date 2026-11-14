using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeDetonation : MonoBehaviour
{

    [SerializeField] private GameObject explosionFX, debris, bridge;

    [SerializeField] private Transform explosionPos, debrisPos;

    [SerializeField] private BuyTrap buyTrap;



    private void OnEnable()
    {
        buyTrap.OnActivateTrap += Detonate;
    }

    private void Detonate()
    {
        Instantiate(debris, debrisPos.transform.position, transform.rotation);

        var explosion = Instantiate(explosionFX, explosionPos.position, transform.rotation);
        explosion.GetComponentInChildren<Explosion>().TriggerExplosion();
        Destroy(bridge);
    }
}
