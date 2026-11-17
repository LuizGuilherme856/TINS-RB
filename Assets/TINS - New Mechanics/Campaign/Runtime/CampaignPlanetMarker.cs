using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TINS.Campaign
{
    public sealed class CampaignPlanetMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private string planetId;
        [SerializeField] private UnityEngine.UI.Button button;
        [SerializeField] private UnityEngine.UI.Image planetArt;
        [SerializeField] private Sprite detailImage;
        [SerializeField] private MMF_Player hoverEnter;
        [SerializeField] private MMF_Player hoverExit;

        private Vector3 restScale;

        public string PlanetId => planetId;
        public UnityEngine.UI.Button Button => button;
        public UnityEngine.UI.Image PlanetArt => planetArt;
        public Sprite DetailImage => detailImage != null ? detailImage : planetArt.sprite;

        private void Awake() => restScale = planetArt.rectTransform.localScale;

        public void OnPointerEnter(PointerEventData eventData)
        {
            hoverExit.StopFeedbacks();
            hoverEnter.PlayFeedbacks();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isActiveAndEnabled) return;
            hoverEnter.StopFeedbacks();
            hoverExit.PlayFeedbacks();
        }

        private void OnDisable() => planetArt.rectTransform.localScale = restScale;
    }
}
