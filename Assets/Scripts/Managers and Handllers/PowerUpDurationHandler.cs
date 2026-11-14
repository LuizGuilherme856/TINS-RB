using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PowerUp;

public class PowerUpDurationHandler : MonoBehaviour
{
    [SerializeField] private GameObject iconGameObject;
    [SerializeField] private Transform iconParent;

    [SerializeField] private Sprite instaKillMeleeSprite, instaKillSprite, doublePointsSprite; 

    public void SetIconDuration(float time, PowerUpType powerUpType )
    {
        var icon = Instantiate( iconGameObject, iconParent );
        PowerUpDurationIcon powerUpDurationIcon = icon.GetComponent<PowerUpDurationIcon>();
        
        powerUpDurationIcon.SetLifeTime(time);

        switch (powerUpType)
        {
            case PowerUpType.instaKillMelee:
                powerUpDurationIcon.SetIcon(instaKillMeleeSprite);
                break;
            case PowerUpType.instaKill:
                powerUpDurationIcon.SetIcon(instaKillSprite);
                break;
            case PowerUpType.doublePoints:
                powerUpDurationIcon.SetIcon(doublePointsSprite);
                break;
            default:
                break;
        }
    }
}
