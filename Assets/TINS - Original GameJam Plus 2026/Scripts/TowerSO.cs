using UnityEngine;

[CreateAssetMenu(fileName = "TowerSO", menuName = "FallenStar/TowerSO")]
public class TowerSO : ScriptableObject
{
    public int maxLife = 10;
    public int baseLife = 10;
    public float immunityAfterDamage = 1.0f;

}
