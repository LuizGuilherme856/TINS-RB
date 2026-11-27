using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TINS.Campaign
{
    public sealed class CampaignMainMenu : MonoBehaviour
    {
        [SerializeField] private CanvasGroup overlay;
        [SerializeField] private Button explore;
        [SerializeField] private Button missions;
        [SerializeField] private Button skills;
        [SerializeField] private Button quit;

        private bool entering;
        private static bool shownAtBoot;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() => shownAtBoot = false;

        internal static bool ClaimTitleScreen()
        {
            if (shownAtBoot) return false;
            shownAtBoot = true;
            return true;
        }

        internal void Bind(Action leave, Action openMissions, Action openSkills)
        {
            explore.onClick.AddListener(() => leave());
            missions.onClick.AddListener(() => openMissions());
            skills.onClick.AddListener(() => openSkills());
#if UNITY_WEBGL && !UNITY_EDITOR
            quit.gameObject.SetActive(false);
#else
            quit.onClick.AddListener(Application.Quit);
#endif
        }

        private void OnEnable()
        {
            overlay.alpha = 0f;
            entering = true;
            EventSystem.current?.SetSelectedGameObject(explore.gameObject);
        }

        private void Update()
        {
            if (!entering) return;
            overlay.alpha = Mathf.MoveTowards(overlay.alpha, 1f, Time.unscaledDeltaTime / .26f);
            entering = overlay.alpha < 1f;
        }
    }
}
