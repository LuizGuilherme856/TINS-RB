// TECHBOOH_YODINHA_CONSOLE_REMOVED_V1: applied to a dedicated CONSOLE project copy.
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Entitlement lookup currently uses Steam. A console port needs a platform-specific ownership
// provider before enabling the corresponding content; a disconnected service is not a successful check.
public class DlcManager : MonoBehaviour
{
     private uint dlcAppId = 3869040;

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