using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillDescription : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI skillDescription;
    [SerializeField] private SkillTreeNode skillTreeNode;
    [SerializeField] private LanguageData languageData;

    private SkillUpgrade skillUpgradeData;

    private void Awake()
    {
        skillUpgradeData = skillTreeNode.GetUpgradeData();
    }
    private void OnEnable()
    {
        if(skillUpgradeData.upgradeData == null) skillUpgradeData = skillTreeNode.GetUpgradeData();

        skillName.text = skillUpgradeData.upgradeData.GetName(languageData.currentLanguage);
        skillDescription.text = skillUpgradeData.upgradeData.GetDescription(languageData.currentLanguage);

    }

}
