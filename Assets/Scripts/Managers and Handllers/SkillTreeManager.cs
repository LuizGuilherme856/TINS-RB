using System.Collections.Generic;
using UnityEngine;
using TINS.Campaign;

public class SkillTreeManager : MonoBehaviour
{
    [SerializeField] private SkillManager skillManager;
    //[SerializeField] private List<UpgradeData> upgradeDatas;
    [SerializeField] private PlayerData playerData;
    public RuneInventory runeInventory;
    public GlobalStats globalStats;
    public PlayerHolder playerHolder;

    private PlayerInputHandler playerInputHandler;

    private List<SkillTreeNode> nodes;

    private void Awake()
    {
        nodes = new List<SkillTreeNode>();
        foreach (var node in GetComponentsInChildren<SkillTreeNode>())
        {
            nodes.Add(node);
        }

        if (runeInventory == null)
        {
            Debug.Log($"Rune Inventory esta nulo em {gameObject.name}. Procurando pelo AssetFinder...");
            runeInventory = AssetFinder.GetRuneInventory();
        }

        playerInputHandler = playerHolder.GetPlayerInputHandler();

        Debug.Log("SkillTreeManager: " + nodes.Count + " nodes encontrados");
    }

    private void OnEnable()
    {
        playerInputHandler.OnEscape += ApplySkillsAndRunes;
        playerInputHandler.OnInventory += ApplySkillsAndRunes;
    }

    public void AddSkill(SkillUpgrade newSkill)
    {
        if (newSkill.upgradeData != null)
        {
            skillManager.AddSkillUpgrade(newSkill);
        }
        else
        {
            Debug.LogError("Nova habilidade Nula!");
        }

    }

    public void AddSkill(List<SkillUpgrade> newSkills)
    {

        foreach (SkillUpgrade skill in newSkills)
        {
            skillManager.AddSkillUpgrade(skill);
        }

    }

    public void RemoveSkill(SkillUpgrade skill)
    {
        skillManager.RemoveSkillUpgrade(skill);
    }

    public List<SkillTreeNode> GetNodes()
    {
        return nodes;
    }

    public void ResetSkillTree()
    {
        foreach (var node in nodes)
        {
            if (node.isActivated)
            {
                node.RemoveNode(true);
            }
        }

        if ((CampaignBaseController.Active == null || !CampaignBaseController.Active.IsReady) && playerData.GetSkillPoints() < playerData.level)
        {
            Debug.LogError("O numero de skill points é inferior ao nivel do jogador. Forçando a ser = ao nivel");
            playerData.SetSkillPoints(playerData.level);
        }
    }

    public void ApplySkillsAndRunes()
    {
        Debug.Log("Iniciou aplicação da skill tree");
        List<RuneQuantityClass> currentRunesQuantities = new();

        // 1. Preserve the current runes.
        try
        {
            Debug.Log("Salvando as runas atuais...");
            currentRunesQuantities.AddRange(runeInventory.GetUpgradesQuantityClasses());
            Debug.Log($"Quantidade de runas salvas: {currentRunesQuantities.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao salvar as runas atuais: " + e);
        }

        // 2. Reset all stats.
        try
        {
            Debug.Log("Resetando todos os stats...");
            globalStats.ResetValues();
            Debug.Log("Stats resetados com sucesso.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao resetar stats: " + e);
        }

        // 3. Attempt to reset runes.
        try
        {
            Debug.Log("Resetando runas...");
            if (runeInventory == null)
            {
                Debug.Log($"Rune Inventory está nulo em {gameObject.name}. Procurando pelo AssetFinder...");
                runeInventory = AssetFinder.GetRuneInventory();
            }
            runeInventory.ResetUpgrades(); //Call this first.
            Debug.Log("Runas resetadas com sucesso.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao resetar runas: " + e);
        }

        // 4. Reapply all owned runes.
        try
        {
            Debug.Log("Reaplicando runas...");
            foreach (var runeQuantity in currentRunesQuantities)
            {
                Debug.Log($"Reaplicando {runeQuantity.quantity}x {runeQuantity.rune}");
                for (int i = 0; i < runeQuantity.quantity; i++)
                {
                    runeInventory.AddUpgrade(runeQuantity.rune);
                }
            }
            Debug.Log("Runas reaplicadas com sucesso.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao reaplicar runas: " + e);
        }

        // 5. Reapply all owned skills.
        try
        {
            Debug.Log("Executando SkillUpgrades...");
            skillManager.ExecuteSkillUpgrades();
            Debug.Log("SkillUpgrades executadas com sucesso.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao executar SkillUpgrades: " + e);
        }
    }

}
