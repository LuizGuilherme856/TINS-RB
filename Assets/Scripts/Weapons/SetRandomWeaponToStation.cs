using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRandomWeaponToStation : MonoBehaviour
{
    [SerializeField] private MeleeData[] meleeDatas;

    private BuyWeapon buyWeaponStation;

    private void Awake()
    {
        buyWeaponStation= GetComponent<BuyWeapon>();
        int index = Random.Range(0,meleeDatas.Length);

        buyWeaponStation.SetMeleeData(meleeDatas[index]);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
