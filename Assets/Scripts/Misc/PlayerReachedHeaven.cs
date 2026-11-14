using UnityEngine;

public class PlayerReachedHeaven : MonoBehaviour
{
    [SerializeField] PlayerData playerData;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(playerData.playerReachedHeaven <=1)
        {
            playerData.playerReachedHeaven = 1;
        }
    }
}
