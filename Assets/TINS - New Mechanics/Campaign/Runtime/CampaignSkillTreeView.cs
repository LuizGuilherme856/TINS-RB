using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MoreMountains.Feedbacks;

namespace TINS.Campaign
{
    /// <summary>Displays authored scene nodes while keeping campaign purchases in CampaignProfile.</summary>
    public sealed class CampaignSkillTreeView : MonoBehaviour
    {
        [SerializeField] private CampaignSkillTreeNodeView[] nodes = Array.Empty<CampaignSkillTreeNodeView>();
        [SerializeField] private CampaignSkillTreeConnectionView[] connections = Array.Empty<CampaignSkillTreeConnectionView>();
        [SerializeField] private TMP_Text walletLabel;
        [SerializeField] private TMP_Text detailsLabel;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private Button buySelectedButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button zoomInButton;
        [SerializeField] private Button zoomOutButton;
        [SerializeField] private Button centerTreeButton;
        [SerializeField] private CampaignSkillTreeScroll treeScrollRect;
        [SerializeField] private TMP_Text detailTitle;
        [SerializeField] private TMP_Text detailCost;
        [SerializeField] private Image detailIcon;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private AudioClip purchaseSound;
        [SerializeField] private AudioClip deniedSound;
        [SerializeField] private MMF_Player selectionFeedback;
        [SerializeField] private MMF_Player purchaseFeedback;
        [SerializeField] private MMF_Player purchaseErrorFeedback;
        [SerializeField] private MMF_Player resetFeedback;
        [SerializeField, Min(.1f)] private float purchaseInputCooldown = .2f;

        private CampaignCatalog catalog;
        private CampaignProfile profile;
        private Func<CampaignSkill, bool> tryPurchase;
        private Action resetSkills;
        private CampaignSkillTreeNodeView selected;
        private string status;
        private float lastHoverTime = float.NegativeInfinity;
        private float lastPurchaseTime = float.NegativeInfinity;
        public Button InitialButton => selected != null ? selected.NodeButton : null;

        public void Bind(CampaignCatalog campaignCatalog, CampaignProfile campaignProfile,
            Func<CampaignSkill, bool> purchase, Action reset)
        {
            catalog = campaignCatalog;
            profile = campaignProfile;
            tryPurchase = purchase;
            resetSkills = reset;
            if (buySelectedButton != null)
            {
                buySelectedButton.onClick.RemoveListener(BuySelected);
                buySelectedButton.onClick.AddListener(BuySelected);
            }
            if (resetButton != null)
            {
                resetButton.onClick.RemoveListener(ResetSkills);
                resetButton.onClick.AddListener(ResetSkills);
            }
            BindButton(zoomInButton, ZoomIn);
            BindButton(zoomOutButton, ZoomOut);
            BindButton(centerTreeButton, CenterTree);
            status = string.Empty;
            if (treeScrollRect == null) treeScrollRect = GetComponentInChildren<CampaignSkillTreeScroll>(true);
            Refresh();
            CenterTree();
        }

        private static void BindButton(Button button, UnityAction action)
        {
            if (button == null) return;
            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        public void Refresh()
        {
            if (catalog == null || profile == null || nodes == null) return;
            foreach (CampaignSkillTreeNodeView node in nodes)
            {
                if (node == null) continue;
                CampaignSkill skill = node.SkillAsset != null ? node.SkillAsset : FindCatalogSkill(node.SkillId);
                node.Bind(skill, profile, SelectNode, PlayHover, profile.IsSkillVisible(skill), CanPurchase(skill), node == selected);
            }
            foreach (CampaignSkillTreeConnectionView connection in connections)
                if (connection != null) connection.RefreshVisibility();
            if (selected == null || !selected.gameObject.activeSelf) selected = FirstVisibleNode();
            CampaignSkill selectedSkill = selected != null ? (selected.SkillAsset != null ? selected.SkillAsset : FindCatalogSkill(selected.SkillId)) : null;
            if (selectedSkill != null) selected.SetSelected(profile.Rank(selectedSkill.id), CanPurchase(selectedSkill), true);
            RefreshDetails();
            if (walletLabel != null) walletLabel.text = $"ALMAS  {profile.souls:N0}    ·    MATERIAIS  {FormatResources()}";
            if (statusLabel != null) statusLabel.text = status;
            if (resetButton != null) resetButton.interactable = profile.skillPurchases != null && profile.skillPurchases.Count > 0;
        }

        private CampaignSkill FindCatalogSkill(string id)
        {
            if (catalog.skills == null) return null;
            foreach (CampaignSkill candidate in catalog.skills)
                if (candidate != null && string.Equals(candidate.id, id, StringComparison.OrdinalIgnoreCase)) return candidate;
            return null;
        }

        public bool CanPurchase(CampaignSkill skill) =>
            skill != null && profile != null && catalog != null && catalog.skills != null &&
            Array.IndexOf(catalog.skills, skill) >= 0 && profile.CanBuy(skill);

        private void SelectNode(CampaignSkillTreeNodeView node)
        {
            bool changed = selected != node;
            selected = node;
            status = string.Empty;
            if (changed) selectionFeedback?.PlayFeedbacks();
            PlaySound(selectSound, .42f);
            ScrollSelectedNodeIntoView();
            Refresh();
        }

        private void ScrollSelectedNodeIntoView()
        {
            if (treeScrollRect == null || treeScrollRect.viewport == null || treeScrollRect.content == null || selected == null) return;
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(treeScrollRect.viewport, selected.transform);
            Rect viewport = treeScrollRect.viewport.rect;
            Vector2 offset = Vector2.zero;
            if (bounds.min.x < viewport.min.x) offset.x = viewport.min.x - bounds.min.x;
            else if (bounds.max.x > viewport.max.x) offset.x = viewport.max.x - bounds.max.x;
            if (bounds.min.y < viewport.min.y) offset.y = viewport.min.y - bounds.min.y;
            else if (bounds.max.y > viewport.max.y) offset.y = viewport.max.y - bounds.max.y;
            treeScrollRect.content.anchoredPosition += offset;
            treeScrollRect.StopMovement();
        }

        private void ZoomIn() => treeScrollRect?.Zoom(1.16f);
        private void ZoomOut() => treeScrollRect?.Zoom(1f / 1.16f);
        private void CenterTree()
        {
            if (treeScrollRect == null) return;

            Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            foreach (CampaignSkillTreeNodeView node in nodes)
            {
                if (node == null || !node.gameObject.activeSelf) continue;
                Vector2 center = ((RectTransform)node.transform).anchoredPosition;
                min = Vector2.Min(min, center - new Vector2(95f, 103f));
                max = Vector2.Max(max, center + new Vector2(95f, 75f));
            }
            if (float.IsPositiveInfinity(min.x)) treeScrollRect.CenterGraph();
            else treeScrollRect.CenterGraph(Rect.MinMaxRect(min.x, min.y, max.x, max.y));
        }

        private void PlayHover()
        {
            if (Time.unscaledTime - lastHoverTime < .08f) return;
            lastHoverTime = Time.unscaledTime;
            PlaySound(hoverSound, .23f);
        }

        private void PlaySound(AudioClip clip, float volume)
        {
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip, volume);
        }

        private void RefreshDetails()
        {
            CampaignSkill skill = selected != null ? selected.SkillAsset : null;
            int rank = skill != null ? profile.Rank(skill.id) : 0;
            bool maxed = skill != null && rank >= skill.maximumRank;
            bool affordable = CanPurchase(skill);

            if (detailTitle != null)
                detailTitle.text = skill != null ? skill.displayName : "Selecione uma habilidade";
            if (detailIcon != null)
            {
                detailIcon.sprite = skill != null ? skill.icon : null;
                detailIcon.enabled = detailIcon.sprite != null;
            }
            if (detailsLabel != null)
            {
                detailsLabel.text = string.Empty;
                if (skill != null)
                {
                    string effect = FormatEffectProgress(skill, rank);
                    detailsLabel.text = $"<color=#DA8DAB>NÍVEL {rank}/{skill.maximumRank}</color>\n\n" + skill.description;
                    if (!string.IsNullOrEmpty(effect)) detailsLabel.text += "\n\n" + effect;
                }
            }
            if (detailCost != null)
            {
                detailCost.text = string.Empty;
                if (skill != null && !maxed)
                {
                    int materialCost = skill.ResourceCostForRank(rank);
                    detailCost.text = $"{skill.CostForRank(rank):N0} almas";
                    if (materialCost > 0)
                        detailCost.text += $"  +  {materialCost} {DisplayResource(skill.requiredResourceId)}";
                }
                else if (maxed) detailCost.text = "Nível máximo";
            }
            if (buySelectedButton != null)
            {
                // Keep the button clickable when blocked to explain the failure and play feedback.
                buySelectedButton.interactable = skill != null;
                ColorBlock colors = buySelectedButton.colors;
                colors.normalColor = affordable ? new Color32(162, 64, 100, 255) : new Color32(76, 63, 88, 255);
                colors.highlightedColor = Color.Lerp(colors.normalColor, Color.white, .24f);
                colors.selectedColor = colors.highlightedColor;
                colors.pressedColor = Color.Lerp(colors.normalColor, Color.black, .2f);
                buySelectedButton.colors = colors;
                TMP_Text label = buySelectedButton.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = maxed ? "NÍVEL MÁXIMO" : "COMPRAR";
            }
            if (statusLabel != null)
            {
                string reason = skill != null && !maxed ? GetRequirementMessage(skill) : string.Empty;
                if (skill != null && !maxed && !affordable && string.IsNullOrEmpty(reason))
                    reason = "Recursos insuficientes";
                statusLabel.text = string.IsNullOrEmpty(status) ? reason : status;
            }
        }

        private string GetRequirementMessage(CampaignSkill skill)
        {
            CampaignSkill conflictingSkill = profile.ConflictingSkill(skill);
            if (conflictingSkill != null) return $"Incompatível com {conflictingSkill.displayName}.";
            if (skill.requiresFlash && !profile.HasFlash) return "Requer: Flash de Luz";
            if (profile.MeetsSkillRequirements(skill)) return string.Empty;
            List<string> missing = new List<string>();
            foreach (CampaignSkillPrerequisite prerequisite in skill.prerequisites)
                if (prerequisite != null && prerequisite.skill != null && profile.Rank(prerequisite.skill.id) < prerequisite.minimumRank)
                    missing.Add(prerequisite.skill.displayName + " nível " + prerequisite.minimumRank);
            return missing.Count > 0 ? "Requer: " + string.Join(skill.requireAllPrerequisites ? " e " : " ou ", missing) : "Pré-requisito indisponível.";
        }

        private static string FormatEffectProgress(CampaignSkill skill, int rank)
        {
            if (skill.effectType == CampaignSkillEffectType.None) return string.Empty;
            if (skill.effectType == CampaignSkillEffectType.WeaponUnlock)
                return rank > 0 ? "Arma liberada para as próximas missões." : "Próxima compra libera uma arma.";

            if (skill.id == CampaignProfile.FlashSkillId && rank == 0)
                return "Próxima compra libera o Flash de Luz.";
            if (skill.effectType == CampaignSkillEffectType.GunCritical)
            {
                string text = $"Chance atual: {skill.EffectAtRank(rank).x:0.#}%";
                if (rank >= skill.maximumRank) return text + $"\n{skill.secondaryEffectPerRank * 100f:0.#}% dos críticos são supercríticos.";
                return text + $"\nPróximo nível: {skill.EffectAtRank(rank + 1).x:0.#}% de crítico.";
            }

            Vector2 current = skill.EffectAtRank(rank);
            bool hasNextEffect = rank < skill.maximumRank && rank + 1 >= Mathf.Max(1, skill.effectStartsAtRank);
            string currentText = FormatEffectValue(skill, current.x, current.y);
            if (string.IsNullOrEmpty(currentText)) return string.Empty;
            if (!hasNextEffect) return "Benefício atual: " + currentText;
            string nextText = FormatEffectValue(skill, skill.effectPerRank, skill.secondaryEffectPerRank);
            return "Benefício atual: " + currentText + "\nPróxima compra: " + nextText;
        }

        private static string FormatEffectValue(CampaignSkill skill, float primary, float secondary)
        {
            switch (skill.effectType)
            {
                case CampaignSkillEffectType.FlashDuration: return $"+{primary * 100f:0.#}% de duração";
                case CampaignSkillEffectType.FlashArea: return $"+{primary:0.#}° e +{secondary * 100f:0.#}% de alcance";
                case CampaignSkillEffectType.FlashCapacity: return $"+{primary:0.#} carga(s)";
                case CampaignSkillEffectType.FlashRecharge: return $"−{primary * 100f:0.#}% de recarga";
                case CampaignSkillEffectType.MovementSpeed: return $"+{primary * 100f:0.#}% de velocidade";
                case CampaignSkillEffectType.PlayerHealth: return $"+{primary:0.#} de vida";
                case CampaignSkillEffectType.ExtraDash: return $"+{primary:0.#} carga de dash";
                case CampaignSkillEffectType.CrystalMagnet: return $"+{primary:0.#} un. de alcance";
                case CampaignSkillEffectType.GuardianBlades: return $"{primary:0.#} lâmina(s)";
                case CampaignSkillEffectType.DoubleTap: return $"{primary * 100f:0.#}% de disparar outra bala";
                default: return string.Empty;
            }
        }

        private void BuySelected()
        {
            CampaignSkill skill = selected != null ? (selected.SkillAsset != null ? selected.SkillAsset : FindCatalogSkill(selected.SkillId)) : null;
            if (skill == null) return;
            if (Time.unscaledTime - lastPurchaseTime < purchaseInputCooldown) return;
            lastPurchaseTime = Time.unscaledTime;
            if (!CanPurchase(skill) || tryPurchase == null || !tryPurchase(skill))
            {
                status = profile.Rank(skill.id) >= skill.maximumRank ? "Esta habilidade já está no máximo." : GetRequirementMessage(skill);
                if (string.IsNullOrEmpty(status)) status = profile.CanBuy(skill) ? "Compra indisponível." : "Recursos insuficientes.";
                selected?.Pulse(false);
                PlaySound(deniedSound, .6f);
                purchaseErrorFeedback?.PlayFeedbacks();
                Refresh();
                return;
            }
            status = skill.displayName + " melhorado.";
            selected?.Pulse(true);
            PlaySound(purchaseSound, .62f);
            purchaseFeedback?.PlayFeedbacks();
            Refresh();
        }

        private void ResetSkills()
        {
            resetSkills?.Invoke();
            status = "Melhorias reiniciadas; recursos devolvidos.";
            PlaySound(selectSound, .45f);
            resetFeedback?.PlayFeedbacks();
            Refresh();
        }

        private CampaignSkillTreeNodeView FirstVisibleNode()
        {
            foreach (CampaignSkillTreeNodeView node in nodes)
                if (node != null && node.gameObject.activeSelf) return node;
            return null;
        }

        private string FormatResources()
        {
            if (profile.resources == null) return "—";
            List<string> amounts = new List<string>();
            foreach (CampaignResourceAmount resource in profile.resources)
                if (resource != null && resource.amount > 0) amounts.Add(DisplayResource(resource.resourceId) + " " + resource.amount);
            return amounts.Count == 0 ? "—" : string.Join("   ·   ", amounts);
        }

        private string DisplayResource(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return id;
            if (catalog.planets == null) return id;
            foreach (CampaignPlanet planet in catalog.planets)
                if (string.Equals(planet.rewardResourceId, id, StringComparison.OrdinalIgnoreCase)) return planet.rewardResourceName;
            return id;
        }
    }

}
