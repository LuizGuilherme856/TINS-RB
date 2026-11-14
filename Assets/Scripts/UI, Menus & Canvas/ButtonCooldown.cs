using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonCooldown : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private float cooldown = 1f;

    private bool onCooldown = false;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    public bool TryUse()
    {
        if (onCooldown)
            return false;

        StartCoroutine(CooldownRoutine());
        return true;
    }

    private IEnumerator CooldownRoutine()
    {
        onCooldown = true;

        if (button != null)
            button.interactable = false;

        yield return new WaitForSecondsRealtime(cooldown);

        if (button != null)
            button.interactable = true;

        onCooldown = false;
    }
}