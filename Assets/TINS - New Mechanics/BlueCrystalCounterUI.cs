using TMPro;
using UnityEngine;

/// <summary>Displays how many blue crystals the player is carrying.</summary>
public sealed class BlueCrystalCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText;
    private BlueCrystalInventory inventory;

    private void OnEnable()
    {
        // Crystal count is represented by the floating world-space followers now.
        // Hide the old HUD element at runtime without requiring a scene hierarchy edit.
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.OnCountChanged -= HandleCountChanged;
        inventory = null;
    }

    private void LateUpdate()
    {
        // The player may be instantiated after the HUD becomes active.
        if (inventory == null)
        {
            BindToPlayerInventory();
            RefreshText();
        }
    }

    private void BindToPlayerInventory()
    {
        var found = FindFirstObjectByType<BlueCrystalInventory>();
        if (found == inventory) return;

        if (inventory != null)
            inventory.OnCountChanged -= HandleCountChanged;

        inventory = found;
        if (inventory != null)
            inventory.OnCountChanged += HandleCountChanged;
    }

    private void HandleCountChanged(int count) => SetCount(count);

    private void RefreshText() => SetCount(inventory != null ? inventory.Count : 0);

    private void SetCount(int count)
    {
        if (counterText != null)
            counterText.text = $"CRISTAIS: {count}";
    }
}
