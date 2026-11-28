using System;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TINS.Campaign
{
    public sealed class CampaignPlanetMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private string planetId;
        [SerializeField] private UnityEngine.UI.Button button;
        [SerializeField] private UnityEngine.UI.Image planetArt;
        [SerializeField] private Sprite detailImage;
        [SerializeField] private MMF_Player hoverEnter;
        [SerializeField] private MMF_Player hoverExit;
        [SerializeField] private TMP_Text statusLabel;

        private Vector3 restScale;
        private TMP_Text caption;
        private float captionSize;

        public string PlanetId => planetId;
        public UnityEngine.UI.Button Button => button;
        public UnityEngine.UI.Image PlanetArt => planetArt;
        public bool UsesPlanetArtInDetails => detailImage == null;
        public Sprite DetailImage => UsesPlanetArtInDetails ? planetArt.sprite : detailImage;
        internal CampaignDestinationState State { get; private set; }
        internal int Completed { get; private set; }
        internal int Goal { get; private set; }
        internal bool IsHovered { get; private set; }
        internal bool IsFocused { get; private set; }
        internal event Action<CampaignPlanetMarker> DirectionalFocus;

        private void Awake()
        {
            restScale = planetArt.rectTransform.localScale;
            caption = button.targetGraphic as TMP_Text;
            if (caption != null) captionSize = caption.fontSize;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsHovered = true;
            hoverExit.StopFeedbacks();
            hoverEnter.PlayFeedbacks();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsHovered = false;
            if (!isActiveAndEnabled) return;
            hoverEnter.StopFeedbacks();
            hoverExit.PlayFeedbacks();
        }

        public void OnSelect(BaseEventData eventData)
        {
            IsFocused = true;
            if (eventData is AxisEventData) DirectionalFocus?.Invoke(this);
        }

        public void OnDeselect(BaseEventData eventData) => IsFocused = false;

        internal void UpdateMapLabels(float zoom)
        {
            if (caption != null) caption.fontSize = Mathf.Max(captionSize, 15f / zoom);
            if (statusLabel != null) statusLabel.enabled = zoom >= .7f;
        }

        internal void ShowProgress(string displayName, CampaignDestinationState state, Color tint, int completed, int goal)
        {
            if (caption != null) caption.text = displayName;
            State = state;
            Goal = Mathf.Max(1, goal);
            Completed = Mathf.Clamp(completed, 0, Goal);
            planetArt.color = state == CampaignDestinationState.Locked ? new Color32(203, 206, 227, 255) : Color.white;
            if (statusLabel == null) return;

            statusLabel.color = tint;
            statusLabel.text = state switch
            {
                CampaignDestinationState.Locked => "Bloqueado",
                CampaignDestinationState.Purified => "Purificado",
                _ => $"Disponível  {Completed}/{Goal}"
            };
        }

        private void OnDisable()
        {
            IsHovered = false;
            IsFocused = false;
            planetArt.rectTransform.localScale = restScale;
        }
    }
}
