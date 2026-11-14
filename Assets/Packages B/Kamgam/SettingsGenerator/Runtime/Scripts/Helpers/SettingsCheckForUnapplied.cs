using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Serialization;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kamgam.SettingsGenerator
{
    /// <summary>
    /// Helper to auto-detect unapplied settings in OnDisable(). Handy for showing confirmation dialogs.
    /// </summary>
    public class SettingsCheckForUnapplied : MonoBehaviour
    {
        [FormerlySerializedAs("Enabled")]
        public bool CheckOnDisable = true;
        
        /// <summary>
        /// Don't forget to hook this up with the right provider.
        /// </summary>
        [Tooltip("Don't forget to hook this up with the right provider.")]
        public SettingsProvider Provider;

        // public delegate void OnUnappliedSettingsDetectedDelegate(List<ISetting> unappliedSettings);
        public UnityEvent<List<ISetting>> OnUnappliedSettingsDetected;

        /// <summary>
        /// Useful for showing modal confirm dialogs after settings UI has been disabled.
        /// </summary>
        [Tooltip("Useful for showing modal confirm dialogs after settings UI has been disabled.")]
        public List<GameObject> ObjectsToShowOnUnapplied;

        [System.NonSerialized]
        public List<ISetting> _unappliedSettings = new List<ISetting>(10);

        public void OnDisable()
        {
            if (CheckOnDisable)
                Check();
        }
        
        public void Check()
        {
            if (Provider == null || !Provider.HasSettings())
                return;

            Provider.Settings.GetUnappliedSettings(_unappliedSettings);
            if(_unappliedSettings.Count > 0)
            {
                OnUnappliedSettingsDetected?.Invoke(_unappliedSettings);

                if (ObjectsToShowOnUnapplied != null && ObjectsToShowOnUnapplied.Count > 0)
                {
                    foreach (var gameObject in ObjectsToShowOnUnapplied)
                    {
                        if (gameObject != null)
                            gameObject.SetActive(true);
                    }
                }
            }
        }

        public void LogSettings(List<ISetting> settings)
        {
            if (settings == null || settings.Count == 0)
            {
                Debug.Log("SettingsCheckForUnapplied: Settings is null!");
                return;
            }
            
            Debug.Log("SettingsCheckForUnapplied: Unapplied settings found:");
            foreach (var setting in settings)
            {
                Debug.Log( " * " + setting.GetID());
            }
        }

        #region Editor Stuff
#if UNITY_EDITOR
        public void Reset()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            editorAutoSelectProvider();
        }

        bool _editorOnValidated;

        public void OnValidate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var prefabStatus = PrefabUtility.GetPrefabInstanceStatus(this.gameObject);
            if (prefabStatus != PrefabInstanceStatus.Connected)
                return;

            if (!_editorOnValidated)
            {
                _editorOnValidated = true;
                editorAutoSelectProvider();
            }
        }

        protected void editorAutoSelectProvider()
        {
            if (Provider == null)
            {
                var provider = SettingsProvider.LastUsedSettingsProvider;

                if (provider == null)
                {
                    var providerGuids = AssetDatabase.FindAssets("t:SettingsProvider");
                    if (providerGuids.Length > 0)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(providerGuids[0]);
                        if (path != null)
                        {
                            provider = AssetDatabase.LoadAssetAtPath<SettingsProvider>(path);
                        }
                    }
                }

                if (provider != null)
                {
                    Provider = provider;
                    EditorUtility.DisplayDialog($"Using provider \"{provider.name}\"",
                        $"SettingsCheckOnDisable has automatically chosen the provider: \"{provider.name}\".\n\n" +
                        $"If that is not the provider you want then please assign it in the inspector.",
                        "Ok");
                    EditorUtility.SetDirty(this);
                    EditorUtility.SetDirty(this.gameObject);
                }
            }
        }
#endif
        #endregion
    }
}
