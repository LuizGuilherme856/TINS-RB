using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] private MMF_Player deathFeedbacks;
    [SerializeField] private Health playerHealth;

    [SerializeField] private GameObject azausCorpse, ninaCorpse, hanbaCorpse, deniroCorpse, yodinhaCorpse;
    [SerializeField] private SpriteRenderer[] playerSprites;

    private Rigidbody2D rb;
    private RunData runData;
    private RuneInventory runeUpgradeManager;

    private void Awake()
    {
        rb= GetComponent<Rigidbody2D>();
        runData = GetComponent<RunDataManager>().GetRunData();
        runeUpgradeManager = FindFirstObjectByType<RuneInventory>();
    }

    private void OnEnable()
    {
        playerHealth.OnZeroHealth += Die;
    }

    private void Die()
    {
        //Disable every player sprite.
        foreach (var sprite in playerSprites)
        {
            sprite.enabled = false;
        }

        rb.bodyType = RigidbodyType2D.Static; //Prevent movement after death.

        playerHealth.SetInvulnerableTime(60);//Temporary invulnerability prevents post-death damage.

        try
        {
            Instantiate(GetPlayerCorpse(), transform.position, transform.rotation);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro: Nao conseguiu instanciar o corpse do player. verifique se nao esta nulo no CharacterData. Exception: "+e.Message);
        }
        deathFeedbacks.PlayFeedbacks();
    }

    /// <summary>
    /// Similar to Die, but used while revival is still possible.
    /// </summary>
    public void KnockDown()
    {
        //Disable every player sprite.
        foreach (var sprite in playerSprites)
        {
            sprite.enabled = false;
        }

        Instantiate(GetPlayerCorpse(), transform.position, transform.rotation);
        deathFeedbacks.PlayFeedbacks();
    }

    public void ReEnablePlayerSprites()
    {
        //Disable every player sprite.
        foreach (var sprite in playerSprites)
        {
            sprite.enabled = true;
        }

    }

    private GameObject GetPlayerCorpse()
    {
        return runData.currentCharacter.corpse;

    }
}
