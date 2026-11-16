// TECHBOOH_YODINHA_CONSOLE_REMOVED_V1: applied to a dedicated CONSOLE project copy.
using UnityEngine;

public class DlcManager : MonoBehaviour
{
    public bool hasDlc = false;

    private void Start()
    {
        hasDlc = CheckDlc();
    }

    public bool CheckDlc()
    {
        // TechBooh console copy: ownership can never enable removed content.
        hasDlc = false;
        return false;
    }

}