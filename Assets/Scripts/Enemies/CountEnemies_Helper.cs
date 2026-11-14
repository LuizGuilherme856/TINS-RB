using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Marker identifying an enemy as part of a wave.
//Count living enemies independently of EnemyHealth, which is also used by bullets.
public class CountEnemies_Helper : MonoBehaviour
{
    public bool isBoss = false;

}
