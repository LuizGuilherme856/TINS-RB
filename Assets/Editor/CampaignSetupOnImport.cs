#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal static class CampaignSetupOnImport
{
    private const string Pending = "ProjectSettings/TINS-Campaign.pending";
    static CampaignSetupOnImport()
    {
        if (File.Exists(Pending))
            EditorApplication.delayCall += Apply;
    }
    private static void Apply()
    {
        if (!File.Exists(Pending) || EditorApplication.isPlayingOrWillChangePlaymode) return;
        try
        {
            Debug.Log(CampaignProjectInstaller.Install());
            File.Delete(Pending);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}
#endif
