using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using MoreMountains.Feedbacks;

namespace TINS.Campaign
{
    public sealed class CampaignSkillTreeNodeView : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private CampaignSkill skill;
        [SerializeField] private string skillId;
        [SerializeField] private UnityEngine.UI.Button button;
        [SerializeField] private TMP_Text label;
        [SerializeField] private TMP_Text rankLabel;
        [SerializeField] private UnityEngine.UI.Image icon;
        [SerializeField] private UnityEngine.UI.Image frame;
        [SerializeField] private UnityEngine.UI.Image selectionRing;
        [SerializeField] private Transform art;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private MMF_Player revealFeedback;

        private static readonly Color Dim = new Color32(126, 107, 136, 255);
        private static readonly Color Ready = new Color32(244, 116, 146, 255);
        private static readonly Color Owned = new Color32(106, 215, 190, 255);
        private static readonly Color Maxed = new Color32(247, 193, 112, 255);

        public string SkillId => skill != null ? skill.id : skillId;
        public CampaignSkill SkillAsset => skill;
        public UnityEngine.UI.Button NodeButton => button;
        public bool IsPurchased { get; private set; }

        private Action<CampaignSkillTreeNodeView> selectHandler;
        private Action hoverHandler;
        private bool hasBeenRevealed;
        private bool hovered;
        private float reveal = 1f;
        private float pulse;

        private void Awake()
        {
            if (button != null) button.onClick.AddListener(Click);
        }

        public void Bind(CampaignSkill data, CampaignProfile profile, Action<CampaignSkillTreeNodeView> select,
            Action hover, bool visible, bool canPurchase, bool isSelected)
        {
            bool newlyRevealed = data != null && visible && !hasBeenRevealed && !data.visibleAtStart;
            hasBeenRevealed = visible;
            if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
            if (!visible || data == null) return;

            skill = data;
            skillId = data.id;
            selectHandler = select;
            hoverHandler = hover;
            int rank = profile.Rank(skillId);
            IsPurchased = rank > 0;
            if (label != null) label.text = skill.displayName;
            if (rankLabel != null) rankLabel.text = rank + "/" + skill.maximumRank;
            if (icon != null)
            {
                icon.sprite = skill.icon;
                icon.enabled = skill.icon != null;
            }
            SetSelected(rank, canPurchase, isSelected);

            if (newlyRevealed)
            {
                if (revealFeedback != null) revealFeedback.PlayFeedbacks();
                else
                {
                    reveal = 0f;
                    if (canvasGroup != null) canvasGroup.alpha = 0f;
                }
            }
        }

        public void SetSelected(int rank, bool canPurchase, bool selected)
        {
            Color accent = rank >= skill.maximumRank ? Maxed : rank > 0 ? Owned : canPurchase ? Ready : Dim;
            if (frame != null) frame.color = accent;
            if (rankLabel != null) rankLabel.color = accent;
            if (icon != null) icon.color = rank > 0 || canPurchase ? Color.white : new Color(.7f, .65f, .76f);
            if (selectionRing != null)
            {
                selectionRing.enabled = selected;
                selectionRing.color = Color.Lerp(accent, Color.white, .55f);
            }
        }

        public void Pulse(bool success)
        {
            pulse = success ? .22f : -.10f;
        }

        private void Update()
        {
            float delta = Time.unscaledDeltaTime;
            reveal = Mathf.MoveTowards(reveal, 1f, delta * 4f);
            pulse = Mathf.MoveTowards(pulse, 0f, delta * .9f);
            if (canvasGroup != null && canvasGroup.alpha != reveal) canvasGroup.alpha = reveal;
            if (art == null) return;
            float target = Mathf.Lerp(.68f, hovered ? 1.12f : 1f, reveal) + pulse;
            float scale = Mathf.Lerp(art.localScale.x, target, 1f - Mathf.Exp(-22f * delta));
            if (!Mathf.Approximately(scale, art.localScale.x)) art.localScale = Vector3.one * scale;
        }

        private void OnDisable()
        {
            hovered = false;
            reveal = 1f;
            pulse = 0f;
            if (art != null) art.localScale = Vector3.one;
            if (canvasGroup != null) canvasGroup.alpha = 1f;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            hoverHandler?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData) => hovered = false;

        public void OnSelect(BaseEventData eventData)
        {
            if (!(eventData is PointerEventData)) selectHandler?.Invoke(this);
        }

        private void Click()
        {
            Pulse(true);
            selectHandler?.Invoke(this);
        }

        public void SetSkillId(string id) => skillId = id;
        public void SetSkill(CampaignSkill data)
        {
            skill = data;
            skillId = data != null ? data.id : string.Empty;
        }
    }
}
