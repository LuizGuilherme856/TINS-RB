using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Controller navigation requires a default selected button.
 * Select the appropriate death-screen button when needed.
 * Notify the selection manager when a new selection is required.
 * */



public class SelectButtonOnGameOver : MonoBehaviour
{
    [SerializeField] private Button restartButton, blessingButton;

    //Disable buttons belonging to unused panels.
    //[SerializeField] private GameObject statisticsScreenButtons, blessingScreenButtons; 
    private HandleGameOver handleGameOver;
    [SerializeField] private PlayerHolder playerHolder;
    private Health playerHealth;
    private SelectEventSystemButton selectEventSystemButton;

    private void Awake()
    {
        playerHealth = playerHolder.GetPlayerHealth();
        handleGameOver = FindFirstObjectByType<HandleGameOver>();


            
        selectEventSystemButton = FindFirstObjectByType<SelectEventSystemButton>();
    }

    private void OnEnable()
    {
        if (playerHealth == null)
        {
            playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
            Debug.LogError("PLAYER HEALTH NAO ENCONTRADO no "+gameObject.name);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        playerHealth.OnZeroHealth += CallSelectStatisticsScreenButton;
    }

    //Split into two methods to avoid coroutine handling.
    private void CallSelectStatisticsScreenButton()
    {
        Invoke(nameof(SelectStatisticsScreenButton), handleGameOver.statisticsScreenDelay + 0.1f);
    }

    public void SelectStatisticsScreenButton()
    {
       // statisticsScreenButtons.SetActive(true);
       // blessingScreenButtons.SetActive(false);
        selectEventSystemButton.SetButtonToSelect(restartButton.gameObject);
    }

    public void SelectBlessingScreenButton()
    {
        //statisticsScreenButtons.SetActive(false);
        //blessingScreenButtons.SetActive(true);
        selectEventSystemButton.SetButtonToSelect(blessingButton.gameObject);
    }


}
