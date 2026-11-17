using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TINS.Campaign
{
    [Serializable] public sealed class CampaignPlanetProgress { public string planetId; public int missionsCompleted; }
    [Serializable] public sealed class CampaignResourceAmount { public string resourceId; public int amount; }
    [Serializable] public sealed class CampaignSkillRank { public string skillId; public int rank; }

    [Serializable]
    public sealed class CampaignProfile
    {
        public int version = 1;
        public int souls;
        public int soulsInvested;
        public List<CampaignPlanetProgress> planetProgress = new List<CampaignPlanetProgress>();
        public List<CampaignResourceAmount> resources = new List<CampaignResourceAmount>();
        public List<CampaignSkillRank> skillRanks = new List<CampaignSkillRank>();
        public List<CampaignResourceAmount> investedResources = new List<CampaignResourceAmount>();
        public int SoulsInvested => soulsInvested;

        private static string SavePath => Path.Combine(Application.persistentDataPath, "tins-campaign.json");

        public static CampaignProfile Load()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    CampaignProfile loaded = JsonUtility.FromJson<CampaignProfile>(File.ReadAllText(SavePath));
                    if (loaded != null && loaded.version == 1)
                    {
                        loaded.planetProgress ??= new List<CampaignPlanetProgress>();
                        loaded.resources ??= new List<CampaignResourceAmount>();
                        loaded.skillRanks ??= new List<CampaignSkillRank>();
                        loaded.investedResources ??= new List<CampaignResourceAmount>();
                        return loaded;
                    }
                }
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                Debug.LogError("Não foi possível ler a campanha: " + exception.Message);
            }
            return new CampaignProfile();
        }

        public bool Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(SavePath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                File.WriteAllText(SavePath, JsonUtility.ToJson(this, true));
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                Debug.LogError("Não foi possível salvar a campanha: " + exception.Message);
                return false;
            }
        }

        public int MissionsOn(string planetId)
        {
            CampaignPlanetProgress progress = FindPlanet(planetId, false);
            return progress != null ? progress.missionsCompleted : 0;
        }

        public bool IsPurified(CampaignPlanet planet) => planet != null && MissionsOn(planet.id) >= Mathf.Max(1, planet.missionsToPurify);

        public bool IsUnlocked(CampaignCatalog catalog, int index)
        {
            if (catalog == null || catalog.planets == null || index < 0 || index >= catalog.planets.Length) return false;
            return FirstUnpurifiedPrerequisite(catalog, index) == null;
        }

        public CampaignPlanet FirstUnpurifiedPrerequisite(CampaignCatalog catalog, int index)
        {
            for (int i = 0; i < index; i++)
            {
                CampaignPlanet prerequisite = catalog.planets[i];
                if (!IsPurified(prerequisite)) return prerequisite;
            }
            return null;
        }

        public void CompleteMission(CampaignPlanet planet)
        {
            if (planet == null) return;
            CampaignPlanetProgress progress = FindPlanet(planet.id, true);
            if (progress.missionsCompleted < Mathf.Max(1, planet.missionsToPurify))
                progress.missionsCompleted++;
            AddResource(planet.rewardResourceId, planet.rewardPerMission);
        }

        public int Resource(string id)
        {
            CampaignResourceAmount value = FindResource(id, false);
            return value != null ? value.amount : 0;
        }

        public void AddResource(string id, int amount)
        {
            if (string.IsNullOrWhiteSpace(id) || amount <= 0) return;
            FindResource(id, true).amount = Mathf.Max(0, Resource(id) + amount);
        }

        public bool TrySpend(string resourceId, int amount)
        {
            if (amount <= 0) return true;
            if (string.IsNullOrWhiteSpace(resourceId))
            {
                if (souls < amount) return false;
                souls -= amount;
                return true;
            }
            CampaignResourceAmount item = FindResource(resourceId, false);
            if (item == null || item.amount < amount) return false;
            item.amount -= amount;
            return true;
        }

        public bool HasPaidTreeSkill(string nodeId) => Rank("raining-blood-tree:" + nodeId) > 0;

        public void RecordTreeSkillPurchase(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId)) return;
            FindSkill("raining-blood-tree:" + nodeId, true).rank++;
        }

        public bool RecordTreeSkillRefund(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId)) return false;
            CampaignSkillRank item = FindSkill("raining-blood-tree:" + nodeId, false);
            if (item == null || item.rank <= 0) return false;
            item.rank--;
            return true;
        }
        public int Rank(string id)
        {
            CampaignSkillRank item = FindSkill(id, false);
            return item != null ? item.rank : 0;
        }

        public bool TryBuy(CampaignSkill skill)
        {
            if (skill == null || string.IsNullOrWhiteSpace(skill.id) || Rank(skill.id) >= Mathf.Max(1, skill.maximumRank)) return false;
            int currentRank = Rank(skill.id);
            int soulCost = skill.CostForRank(currentRank);
            int resourceCost = skill.ResourceCostForRank(currentRank);
            if (souls < soulCost || (!string.IsNullOrWhiteSpace(skill.requiredResourceId) && Resource(skill.requiredResourceId) < resourceCost)) return false;
            souls -= soulCost;
            soulsInvested += soulCost;
            if (!string.IsNullOrWhiteSpace(skill.requiredResourceId))
            {
                TrySpend(skill.requiredResourceId, resourceCost);
                CampaignResourceAmount spent = FindInList(investedResources, skill.requiredResourceId);
                if (spent == null)
                {
                    spent = new CampaignResourceAmount { resourceId = skill.requiredResourceId };
                    investedResources.Add(spent);
                }
                spent.amount += resourceCost;
            }
            FindSkill(skill.id, true).rank++;
            return true;
        }

        public void ResetCampaignSkills(IEnumerable<CampaignSkill> skills)
        {
            foreach (CampaignSkill skill in skills)
            {
                CampaignSkillRank entry = FindSkill(skill.id, false);
                if (entry == null || entry.rank <= 0) continue;

                for (int rank = 0; rank < entry.rank; rank++)
                {
                    int refund = Mathf.Min(soulsInvested, skill.CostForRank(rank));
                    souls += refund;
                    soulsInvested -= refund;

                    CampaignResourceAmount invested = FindInList(investedResources, skill.requiredResourceId);
                    if (invested == null) continue;
                    int materialRefund = Mathf.Min(invested.amount, skill.ResourceCostForRank(rank));
                    AddResource(invested.resourceId, materialRefund);
                    invested.amount -= materialRefund;
                }

                skillRanks.Remove(entry);
            }
        }

        private CampaignPlanetProgress FindPlanet(string id, bool create)
        {
            foreach (CampaignPlanetProgress item in planetProgress)
                if (item != null && string.Equals(item.planetId, id, StringComparison.OrdinalIgnoreCase)) return item;
            if (!create) return null;
            CampaignPlanetProgress created = new CampaignPlanetProgress { planetId = id };
            planetProgress.Add(created);
            return created;
        }

        private CampaignResourceAmount FindResource(string id, bool create)
        {
            foreach (CampaignResourceAmount item in resources)
                if (item != null && string.Equals(item.resourceId, id, StringComparison.OrdinalIgnoreCase)) return item;
            if (!create) return null;
            CampaignResourceAmount created = new CampaignResourceAmount { resourceId = id };
            resources.Add(created);
            return created;
        }

        private CampaignSkillRank FindSkill(string id, bool create)
        {
            foreach (CampaignSkillRank item in skillRanks)
                if (item != null && string.Equals(item.skillId, id, StringComparison.OrdinalIgnoreCase)) return item;
            if (!create) return null;
            CampaignSkillRank created = new CampaignSkillRank { skillId = id };
            skillRanks.Add(created);
            return created;
        }

        private static CampaignResourceAmount FindInList(List<CampaignResourceAmount> values, string id)
        {
            foreach (CampaignResourceAmount value in values)
                if (value != null && string.Equals(value.resourceId, id, StringComparison.OrdinalIgnoreCase)) return value;
            return null;
        }
    }

    public static class CampaignTravel
    {
        public const string BaseScene = "CampaignBase";
        public const string MissionScene = "HR Floor one";
        private static string pendingPlanet;
        public static string PendingPlanet => pendingPlanet;
        public static void QueueMission(string planetId) => pendingPlanet = planetId;
        public static string ConsumeMission() { string id = pendingPlanet; pendingPlanet = null; return id; }
    }
}
