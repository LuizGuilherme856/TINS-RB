using TMPro;
using UnityEngine;

namespace TINS.Campaign
{
    public enum BaseStationType { MissionBoard, SkillTree }

    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class BaseStationInteractable : MonoBehaviour
    {
        [SerializeField] private BaseStationType stationType;
        [SerializeField] private CampaignBaseController baseController;
        [SerializeField] private TMP_Text prompt;
        [SerializeField, Min(0.1f)] private float interactionRadius = 1.65f;

        public float InteractionRadiusSquared => interactionRadius * interactionRadius;

        private void Awake()
        {
            if (baseController == null) baseController = GetComponentInParent<CampaignBaseController>();
            if (baseController == null) baseController = FindFirstObjectByType<CampaignBaseController>();
            if (prompt != null)
                prompt.text = stationType == BaseStationType.MissionBoard ? "F / A  ·  MISSÕES" : "F / A  ·  HABILIDADES";
            SetPromptVisible(false);
        }

        public bool BelongsTo(CampaignBaseController controller) => baseController == controller;

        public float DistanceSquaredTo(Vector2 position) => (position - (Vector2)transform.position).sqrMagnitude;

        public void SetPromptVisible(bool visible)
        {
            if (prompt != null) prompt.gameObject.SetActive(visible);
        }

        public void Open()
        {
            if (baseController == null) return;
            if (stationType == BaseStationType.MissionBoard) baseController.ShowMissionBoard();
            else baseController.ShowSkillTree();
        }

        private void OnDisable() => SetPromptVisible(false);
    }
}
