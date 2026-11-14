using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("")]
public class LuciferFightManager : MonoBehaviour
{
    [SerializeField] public LuciferBoss luciStage1;
    [SerializeField] public LuciferData luciData; //Assigned in the Editor.
    [SerializeField] public GameObject luci2Stage; //Assigned in the Editor.

    protected void Awake()
    {
		try
		{
			luciStage1 = GetComponentInChildren<LuciferBoss>();
            luciStage1.Stage1Death += ChangeForm;

        }
		catch (System.Exception)
		{

			throw;
		}
    }

    public void ChangeForm() 
    {
        StartCoroutine(ChangeForm_Coroutine());
    }
    private IEnumerator ChangeForm_Coroutine()
    {
        luciStage1.gameObject.SetActive(false);
        yield return new WaitForSeconds(luciData.goTo2StageDelay);
        luci2Stage.gameObject.SetActive(true);

    }
}
