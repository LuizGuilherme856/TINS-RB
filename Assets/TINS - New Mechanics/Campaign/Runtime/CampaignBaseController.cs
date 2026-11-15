using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TINS.Campaign
{
    public sealed class CampaignBaseController : MonoBehaviour
    {
        public static CampaignBaseController Active { get; private set; }
        public event Action<int> SoulsChanged;
        private enum Panel { None, Missions, Skills }

        [SerializeField] private CampaignCatalog catalog;
        [SerializeField] private PlayerInputHandler playerInput;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private GameObject missionPanel;
        [SerializeField] private GameObject skillPanel;
        [SerializeField] private TMP_Text soulsLabel;
        [SerializeField] private TMP_Text resourceLabel;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private Button[] planetButtons;
        [SerializeField] private TMP_Text[] planetLabels;
        [SerializeField] private CampaignPurificationMapMask[] planetPurificationMasks;
        [SerializeField] private RectTransform planetMap;
        [SerializeField] private Button[] skillButtons;
        [SerializeField] private TMP_Text[] skillLabels;
        [SerializeField] private Button resetSkillsButton;
        [SerializeField] private Button closeButtonsMission;
        [SerializeField] private Button closeButtonsSkill;
        [SerializeField] private ShowSkillTreeManager originalSkillTree;
        [SerializeField] private Button originalSkillTreeButton;
        private BaseStationInteractable[] stations;
        private BaseStationInteractable nearbyStation;
        private CampaignProfile profile;
        private Panel currentPanel;
        private string statusMessage;

        public CampaignProfile Profile => profile;
        public CampaignCatalog Catalog => catalog;
        public bool IsReady => profile != null;

        public bool TrySpendSoulsForTree(int amount, string nodeId)
        {
            if (profile == null || amount < 0 || profile.souls < amount) return false;
            profile.souls -= amount;
            profile.soulsInvested += amount;
            profile.RecordTreeSkillPurchase(nodeId);
            profile.Save();
            Refresh();
            return true;
        }

        public void RefundSoulsForTree(int amount, string nodeId)
        {
            if (profile == null || amount <= 0 || !profile.RecordTreeSkillRefund(nodeId)) return;
            profile.souls += amount;
            profile.soulsInvested = Mathf.Max(0, profile.soulsInvested - amount);
            profile.Save();
            Refresh();
        }
        private void Awake()
        {
            Active = this;
            profile = CampaignProfile.Load();
        }

        private void Start()
        {
            if (catalog == null) catalog = Resources.Load<CampaignCatalog>("CampaignCatalog");
            if (catalog == null)
            {
                Debug.LogError("The Base cannot open its stations because the campaign catalog is missing.", this);
                enabled = false;
                return;
            }
            EnsureMapLayers();
            if (playerInput == null) playerInput = FindFirstObjectByType<PlayerInputHandler>();
            if (playerMovement == null && playerInput != null) playerMovement = playerInput.GetComponent<PlayerMovement>();
            stations = FindObjectsByType<BaseStationInteractable>(FindObjectsSortMode.None);
            if (playerInput != null) playerInput.OnInteract += InteractWithNearestStation;
            if (missionPanel != null) missionPanel.SetActive(false);
            if (skillPanel != null) skillPanel.SetActive(false);
            if (closeButtonsMission != null) closeButtonsMission.onClick.AddListener(ClosePanels);
            if (closeButtonsSkill != null) closeButtonsSkill.onClick.AddListener(ClosePanels);
            if (resetSkillsButton != null) resetSkillsButton.onClick.AddListener(ResetSkills);
            for (int i = 0; i < planetButtons.Length; i++)
            {
                int index = i;
                planetButtons[i].onClick.AddListener(() => LaunchMission(index));
            }
            for (int i = 0; i < skillButtons.Length; i++)
            {
                int index = i;
                skillButtons[i].onClick.AddListener(() => BuySkill(index));
            }
            Refresh();
        }

        private void EnsureMapLayers()
        {
            if (planetMap == null || catalog == null) return;
            if (planetPurificationMasks != null && planetPurificationMasks.Length == catalog.planets.Length &&
                System.Array.TrueForAll(planetPurificationMasks, mask => mask != null)) return;

            Sprite ocean = Resources.Load<Sprite>("PlanetMap/Planet map - ocean");
            Sprite corruption = Resources.Load<Sprite>("PlanetMap/Planet map - corrupted");
            Sprite purified = Resources.Load<Sprite>("PlanetMap/Planet map - purified");
            Sprite circle = Resources.Load<Sprite>("PlanetMap/Planet purification circle");
            if (ocean == null || corruption == null || purified == null || circle == null)
            {
                Debug.LogError("The campaign map sprites could not be loaded.", this);
                return;
            }

            planetMap.GetComponent<Image>().color = Color.white;
            CreateMapImage("Map Ocean Background", planetMap, ocean).transform.SetSiblingIndex(0);
            CreateMapImage("Map Corrupted Overlay", planetMap, corruption).transform.SetSiblingIndex(1);
            planetPurificationMasks = new CampaignPurificationMapMask[catalog.planets.Length];
            for (int i = 0; i < catalog.planets.Length; i++)
            {
                CampaignPlanet planet = catalog.planets[i];
                Vector2 anchor = new Vector2(planet.mapX, planet.mapY);
                Image circleImage = CreateMapImage("Purification - " + planet.id, planetMap, circle);
                circleImage.transform.SetSiblingIndex(2 + i);
                RectTransform circleRect = circleImage.rectTransform;
                circleRect.anchorMin = circleRect.anchorMax = anchor;
                circleRect.anchoredPosition = Vector2.zero;
                circleRect.sizeDelta = Vector2.zero;
                circleImage.gameObject.AddComponent<Mask>().showMaskGraphic = false;

                Image greenImage = CreateMapImage("Green Map Texture", circleRect, purified);
                RectTransform greenRect = greenImage.rectTransform;
                Vector2 mapSize = planetMap.rect.size;
                greenRect.anchorMin = greenRect.anchorMax = Vector2.one * 0.5f;
                greenRect.sizeDelta = mapSize;
                greenRect.anchoredPosition = -Vector2.Scale(anchor - Vector2.one * 0.5f, mapSize);

                CampaignPurificationMapMask mask = circleImage.gameObject.AddComponent<CampaignPurificationMapMask>();
                mask.Configure(planetMap, circleRect);
                planetPurificationMasks[i] = mask;
            }
        }

        private static Image CreateMapImage(string name, RectTransform parent, Sprite sprite)
        {
            var layer = new GameObject(name, typeof(RectTransform), typeof(Image));
            layer.transform.SetParent(parent, false);
            var image = layer.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return image;
        }

        private void Update()
        {
            UpdateNearbyStation();
            if (currentPanel == Panel.None) return;
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true || Gamepad.current?.buttonEast.wasPressedThisFrame == true)
                ClosePanels();
        }

        private void UpdateNearbyStation()
        {
            BaseStationInteractable nearest = null;
            float nearestDistance = float.PositiveInfinity;
            if (playerInput != null && currentPanel == Panel.None &&
                (originalSkillTree == null || !originalSkillTree.IsSkillTreeOpen()))
            {
                foreach (BaseStationInteractable station in stations)
                {
                    if (station == null || !station.isActiveAndEnabled || !station.BelongsTo(this)) continue;
                    float distance = station.DistanceSquaredTo(playerInput.transform.position);
                    if (distance > station.InteractionRadiusSquared || distance >= nearestDistance) continue;
                    nearest = station;
                    nearestDistance = distance;
                }
            }

            if (nearbyStation == nearest) return;
            if (nearbyStation != null) nearbyStation.SetPromptVisible(false);
            nearbyStation = nearest;
            if (nearbyStation != null) nearbyStation.SetPromptVisible(true);
        }

        private void InteractWithNearestStation()
        {
            if (!isActiveAndEnabled) return;
            UpdateNearbyStation();
            if (nearbyStation != null) nearbyStation.Open();
        }

        private void OnDestroy()
        {
            if (Active == this) Active = null;
            if (playerInput != null) playerInput.OnInteract -= InteractWithNearestStation;
        }

        public void ShowMissionBoard()
        {
            if (catalog == null || currentPanel != Panel.None) return;
            currentPanel = Panel.Missions;
            if (missionPanel != null) missionPanel.SetActive(true);
            if (skillPanel != null) skillPanel.SetActive(false);
            SetPlayerPaused(true);
            Refresh();
            SelectFirst(planetButtons);
        }

        public void ShowSkillTree()
        {
            if (currentPanel != Panel.None || originalSkillTree == null || originalSkillTree.IsSkillTreeOpen()) return;
            playerMovement?.StopImmediately();
            originalSkillTree.ShowSkillTree();
        }

        public void ClosePanels()
        {
            currentPanel = Panel.None;
            if (missionPanel != null) missionPanel.SetActive(false);
            if (skillPanel != null) skillPanel.SetActive(false);
            statusMessage = string.Empty;
            SetPlayerPaused(false);
            EventSystem.current?.SetSelectedGameObject(null);
            Refresh();
        }

        private void LaunchMission(int index)
        {
            if (catalog == null || profile == null || index < 0 || index >= catalog.planets.Length || !profile.IsUnlocked(catalog, index))
            {
                statusMessage = "Purifique o planeta anterior para liberar este destino.";
                Refresh();
                return;
            }

            CampaignTravel.QueueMission(catalog.planets[index].id);
            if (playerInput != null) playerInput.SetCanMove(true);
            SceneManager.LoadScene(CampaignTravel.MissionScene);
        }

        private void BuySkill(int index)
        {
            if (catalog == null || profile == null || index < 0 || index >= catalog.skills.Length) return;
            CampaignSkill skill = catalog.skills[index];
            if (!profile.TryBuy(skill))
            {
                statusMessage = $"Recursos insuficientes para {skill.displayName}.";
                Refresh();
                return;
            }
            profile.Save();
            statusMessage = $"{skill.displayName} melhorado.";
            Refresh();
        }

        private void ResetSkills()
        {
            if (profile == null) return;
            profile.ResetSkills();
            profile.Save();
            statusMessage = "Melhorias reiniciadas; recursos devolvidos.";
            Refresh();
        }

        private void Refresh()
        {
            if (profile == null || catalog == null) return;
            if (soulsLabel != null) soulsLabel.text = $"ALMAS  {profile.souls:N0}";
            SoulsChanged?.Invoke(profile.souls);
            if (resourceLabel != null) resourceLabel.text = FormatResources();
            if (statusLabel != null) statusLabel.text = statusMessage;

            for (int i = 0; i < planetButtons.Length && i < catalog.planets.Length; i++)
            {
                CampaignPlanet planet = catalog.planets[i];
                bool unlocked = profile.IsUnlocked(catalog, i);
                bool purified = profile.IsPurified(planet);
                int done = profile.MissionsOn(planet.id);
                float purificationProgress = Mathf.Clamp01(done / (float)Mathf.Max(1, planet.missionsToPurify));
                if (planetLabels[i] != null)
                    planetLabels[i].text = $"{planet.displayName}\n{(purified ? "PURIFICADO" : unlocked ? $"MISSÕES {done}/{planet.missionsToPurify}" : "BLOQUEADO")}";
                if (planetPurificationMasks != null && i < planetPurificationMasks.Length && planetPurificationMasks[i] != null)
                    planetPurificationMasks[i].SetProgress(purificationProgress);
                planetButtons[i].interactable = unlocked;
                Image image = planetButtons[i].GetComponent<Image>();
                if (image != null)
                {
                    Color corrupted = unlocked ? new Color(.72f, .37f, .18f) : new Color(.25f, .25f, .3f);
                    Color restored = new Color(.18f, .68f, .45f);
                    image.color = Color.Lerp(corrupted, restored, purificationProgress);
                }
                RectTransform marker = planetButtons[i].GetComponent<RectTransform>();
                if (planetMap != null)
                {
                    marker.anchorMin = marker.anchorMax = new Vector2(planet.mapX, planet.mapY);
                    marker.anchoredPosition = Vector2.zero;
                }
            }

            for (int i = 0; i < skillButtons.Length && i < catalog.skills.Length; i++)
            {
                CampaignSkill skill = catalog.skills[i];
                int rank = profile.Rank(skill.id);
                bool maxed = rank >= skill.maximumRank;
                int resourceCost = skill.ResourceCostForRank(rank);
                int materialCount = string.IsNullOrWhiteSpace(skill.requiredResourceId) ? 0 : profile.Resource(skill.requiredResourceId);
                string materialText = string.IsNullOrWhiteSpace(skill.requiredResourceId) ? string.Empty : $" + {resourceCost} {DisplayResource(skill.requiredResourceId)}";
                if (skillLabels[i] != null)
                    skillLabels[i].text = $"{skill.displayName}  {rank}/{skill.maximumRank}\n{skill.description}\n{(maxed ? "MÁXIMO" : $"{skill.CostForRank(rank)} almas{materialText}")}";
                skillButtons[i].interactable = !maxed && profile.souls >= skill.CostForRank(rank) && materialCount >= resourceCost;
            }

            if (resetSkillsButton != null) resetSkillsButton.interactable = profile.SoulsInvested > 0;
        }

        private string FormatResources()
        {
            if (profile.resources == null || profile.resources.Count == 0) return "MATERIAIS  —";
            System.Text.StringBuilder text = new System.Text.StringBuilder("MATERIAIS  ");
            bool first = true;
            foreach (CampaignResourceAmount item in profile.resources)
            {
                if (item == null || item.amount <= 0) continue;
                if (!first) text.Append("   ·   ");
                text.Append(DisplayResource(item.resourceId)).Append(' ').Append(item.amount);
                first = false;
            }
            return first ? "MATERIAIS  —" : text.ToString();
        }

        private static string DisplayResource(string id) => string.Equals(id, "ether", System.StringComparison.OrdinalIgnoreCase) ? "Éter" : id;

        private void SetPlayerPaused(bool paused)
        {
            if (playerInput != null) playerInput.SetCanMove(!paused);
            if (paused && playerMovement != null) playerMovement.StopImmediately();
        }

        private static void SelectFirst(Button[] buttons)
        {
            if (EventSystem.current == null || buttons == null) return;
            foreach (Button button in buttons)
                if (button != null && button.gameObject.activeInHierarchy && button.interactable)
                {
                    EventSystem.current.SetSelectedGameObject(button.gameObject);
                    return;
                }
        }
    }
}
