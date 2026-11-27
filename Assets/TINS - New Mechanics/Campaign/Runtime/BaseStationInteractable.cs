using UnityEngine;

namespace TINS.Campaign
{
    public enum BaseStationType { MissionBoard, SkillTree }

    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class BaseStationInteractable : MonoBehaviour
    {
        private static readonly int OutlineFade = Shader.PropertyToID("_InnerOutlineFade");

        [SerializeField] private BaseStationType stationType;
        [SerializeField] private CampaignBaseController baseController;
        [SerializeField, Min(0.1f)] private float interactionRadius = 1.65f;
        [SerializeField] private SpriteRenderer outlinedSprite;
        [SerializeField] private SpriteRenderer promptIcon;
        [SerializeField] private Sprite keyboardIcon;
        [SerializeField] private Sprite xboxIcon;
        [SerializeField] private Sprite playStationIcon;
        [SerializeField] private Sprite switchIcon;

        private MaterialPropertyBlock outlineProperties;
        private Vector3 promptCenter;

        public float InteractionRadiusSquared => interactionRadius * interactionRadius;

        private void Awake()
        {
            if (baseController == null) baseController = GetComponentInParent<CampaignBaseController>();
            if (baseController == null) baseController = FindFirstObjectByType<CampaignBaseController>();
            if (promptIcon != null) promptCenter = promptIcon.transform.localPosition;
            SetPromptVisible(false);
        }

        public bool BelongsTo(CampaignBaseController controller) => baseController == controller;

        public float DistanceSquaredTo(Vector2 position) => (position - (Vector2)transform.position).sqrMagnitude;

        public void SetPromptVisible(bool visible)
        {
            if (promptIcon != null) promptIcon.enabled = visible;
            if (outlinedSprite == null) return;

            outlineProperties ??= new MaterialPropertyBlock();
            outlinedSprite.GetPropertyBlock(outlineProperties);
            outlineProperties.SetFloat(OutlineFade, visible ? 1f : 0f);
            outlinedSprite.SetPropertyBlock(outlineProperties);
        }

        public void SetPromptDevice(InputPromptDeviceType device)
        {
            if (promptIcon == null) return;

            Sprite icon = device switch
            {
                InputPromptDeviceType.KeyboardMouse => keyboardIcon,
                InputPromptDeviceType.PlayStation => playStationIcon,
                InputPromptDeviceType.NintendoSwitch => switchIcon,
                _ => xboxIcon
            };
            promptIcon.sprite = icon;
            var anchor = promptIcon.transform;
            Vector3 offset = icon != null ? Vector3.Scale(icon.bounds.center, anchor.localScale) : Vector3.zero;
            anchor.localPosition = promptCenter - anchor.localRotation * offset;
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
