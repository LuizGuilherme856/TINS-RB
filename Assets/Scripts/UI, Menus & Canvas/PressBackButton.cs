using UnityEngine;
using UnityEngine.UI;

public class PressBackButton : MonoBehaviour
{
    [SerializeField] private PlayerHolder playerHolder;
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject[] blockingChildren;

    private PlayerInputHandler playerInputHandler;
    private static int lastHandledBackFrame = -1;

    private void Awake()
    {
        if (playerHolder == null)
        {
            playerHolder = FindFirstObjectByType<PlayerHolder>();
        }

        if (playerHolder != null)
        {
            playerInputHandler = playerHolder.GetPlayerInputHandler();
        }
    }

    private void OnEnable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnMenuB += PressButton;
        }
    }

    private void OnDisable()
    {
        if (playerInputHandler != null)
        {
            playerInputHandler.OnMenuB -= PressButton;
        }
    }

    // Multiple panels can subscribe to the same back action. Handle it once per frame and only in
    // the highest-priority eligible panel so one press does not close several menu layers.
    private void PressButton()
    {
        if (lastHandledBackFrame == Time.frameCount) return;
        if (CanHandleBack() == false) return;
        if (IsTopPriorityBackHandler() == false) return;

        lastHandledBackFrame = Time.frameCount;
        backButton.onClick.Invoke();
    }

    private bool CanHandleBack()
    {
        if (!gameObject.activeInHierarchy) return false;
        if (HasBlockingChildOpen()) return false;
        if (backButton == null) return false;
        if (!backButton.gameObject.activeInHierarchy) return false;
        if (!backButton.interactable) return false;

        return true;
    }

    private bool HasBlockingChildOpen()
    {
        if (blockingChildren == null || blockingChildren.Length == 0)
        {
            return false;
        }

        foreach (var blockingChild in blockingChildren)
        {
            if (blockingChild != null && blockingChild.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsTopPriorityBackHandler()
    {
        PressBackButton[] allBackHandlers = FindObjectsByType<PressBackButton>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        PressBackButton topPriorityHandler = null;
        int highestDepth = -1;

        foreach (var backHandler in allBackHandlers)
        {
            if (backHandler == null || backHandler.CanHandleBack() == false)
            {
                continue;
            }

            int currentDepth = backHandler.GetHierarchyDepth();
            if (currentDepth > highestDepth)
            {
                highestDepth = currentDepth;
                topPriorityHandler = backHandler;
            }
        }

        return topPriorityHandler == this;
    }

    private int GetHierarchyDepth()
    {
        int depth = 0;
        Transform current = transform;

        while (current.parent != null)
        {
            depth++;
            current = current.parent;
        }

        return depth;
    }
}
