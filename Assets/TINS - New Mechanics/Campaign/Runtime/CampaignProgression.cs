using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TINS.Campaign
{
    [Serializable] public sealed class CampaignPlanetProgress { public string planetId; public int missionsCompleted; }
    [Serializable] public sealed class CampaignResourceAmount { public string resourceId; public int amount; }
    [Serializable] public sealed class CampaignSkillRank { public string skillId; public int rank; }
    [Serializable] public sealed class CampaignSkillGrant { public string skillId; public int rank; }
    [Serializable] public sealed class CampaignSkillPurchase
    {
        public string skillId;
        public int soulsPaid;
        public string resourceId;
        public int resourcePaid;
    }

    [Serializable]
    public sealed class CampaignProfile
    {
        public const string FlashSkillId = "flash-duration";
        public int version = 3;
        public bool firstExpeditionSettled;
        public bool inheritedFlash;
        public string lastVisitedPlanetId;
        public bool HasFlash => Rank(FlashSkillId) > 0;
        public int souls;
        public int soulsInvested;
        public List<CampaignPlanetProgress> planetProgress = new List<CampaignPlanetProgress>();
        public List<CampaignResourceAmount> resources = new List<CampaignResourceAmount>();
        public List<CampaignSkillRank> skillRanks = new List<CampaignSkillRank>();
        public List<CampaignResourceAmount> investedResources = new List<CampaignResourceAmount>();
        public List<CampaignSkillGrant> freeSkillGrants = new List<CampaignSkillGrant>();
        public List<CampaignSkillPurchase> skillPurchases = new List<CampaignSkillPurchase>();
        public int SoulsInvested => soulsInvested;
        public int CampaignSkillsSoulsInvested
        {
            get
            {
                int total = 0;
                if (skillPurchases != null)
                    foreach (CampaignSkillPurchase purchase in skillPurchases)
                        if (purchase != null) total += Mathf.Max(0, purchase.soulsPaid);
                return total;
            }
        }

        private static string SavePath => Path.Combine(Application.persistentDataPath, "tins-campaign.json");

        public static CampaignProfile Load()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    CampaignProfile loaded = JsonUtility.FromJson<CampaignProfile>(File.ReadAllText(SavePath));
                    if (loaded != null && (loaded.version == 1 || loaded.version == 2 || loaded.version == 3))
                    {
                        loaded.planetProgress ??= new List<CampaignPlanetProgress>();
                        loaded.resources ??= new List<CampaignResourceAmount>();
                        loaded.skillRanks ??= new List<CampaignSkillRank>();
                        loaded.investedResources ??= new List<CampaignResourceAmount>();
                        loaded.freeSkillGrants ??= new List<CampaignSkillGrant>();
                        loaded.skillPurchases ??= new List<CampaignSkillPurchase>();
                        if (loaded.version == 1)
                        {
                            // Existing Flash upgrades imply that this profile already owns the ability.
                            if (loaded.HasFlash || loaded.Rank("flash-capacity") > 0 || loaded.Rank("flash-recharge") > 0)
                            {
                                loaded.FindSkill(FlashSkillId, true).rank++;
                                loaded.inheritedFlash = true;
                            }
                            loaded.firstExpeditionSettled = loaded.HasFlash || loaded.planetProgress.Exists(p => p != null && p.missionsCompleted > 0);
                        }
                        if (loaded.inheritedFlash && loaded.FreeSkillRank(FlashSkillId) == 0)
                            loaded.freeSkillGrants.Add(new CampaignSkillGrant { skillId = FlashSkillId, rank = 1 });
                        loaded.version = 3;
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
            CampaignPlanet planet = catalog.planets[index];
            if (planet == null) return false;
            if (MissionsOn(planet.id) > 0 || planet.prerequisitePlanetIds == null) return true;
            foreach (string requiredId in planet.prerequisitePlanetIds)
                if (!IsPurified(catalog.GetPlanet(requiredId))) return false;
            return true;
        }

        public CampaignPlanet FirstUnpurifiedPrerequisite(CampaignCatalog catalog, int index)
        {
            if (catalog?.planets == null || index < 0 || index >= catalog.planets.Length) return null;
            var requirements = catalog.planets[index]?.prerequisitePlanetIds;
            if (requirements == null) return null;
            foreach (string requiredId in requirements)
            {
                CampaignPlanet prerequisite = catalog.GetPlanet(requiredId);
                if (prerequisite != null && !IsPurified(prerequisite)) return prerequisite;
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

        public int FreeSkillRank(string id)
        {
            if (freeSkillGrants == null) return 0;
            CampaignSkillGrant grant = FindGrant(id, false);
            return grant != null ? Mathf.Max(0, grant.rank) : 0;
        }

        public void GrantSkillRanks(string id, int amount)
        {
            if (string.IsNullOrWhiteSpace(id) || amount <= 0) return;
            CampaignSkillGrant grant = FindGrant(id, true);
            grant.rank += amount;
            CampaignSkillRank skill = FindSkill(id, true);
            skill.rank += amount;
        }

        /// <summary>Backfills a one-time purchase ledger for saves created before exact refunds were recorded.</summary>
        public bool EnsurePurchaseLedger(IEnumerable<CampaignSkill> skills)
        {
            if (skills == null) return false;
            skillPurchases ??= new List<CampaignSkillPurchase>();
            freeSkillGrants ??= new List<CampaignSkillGrant>();
            bool changed = false;
            foreach (CampaignSkill skill in skills)
            {
                if (skill == null || string.IsNullOrWhiteSpace(skill.id)) continue;
                int totalRank = Rank(skill.id);
                int freeRanks = FreeSkillRank(skill.id);
                int paidRanks = Mathf.Max(0, totalRank - freeRanks);
                int recordedPurchases = 0;
                foreach (CampaignSkillPurchase purchase in skillPurchases)
                    if (purchase != null && string.Equals(purchase.skillId, skill.id, StringComparison.OrdinalIgnoreCase)) recordedPurchases++;

                while (recordedPurchases < paidRanks)
                {
                    int currentRank = freeRanks + recordedPurchases;
                    skillPurchases.Add(new CampaignSkillPurchase
                    {
                        skillId = skill.id,
                        soulsPaid = skill.CostForRank(currentRank),
                        resourceId = skill.requiredResourceId,
                        resourcePaid = skill.ResourceCostForRank(currentRank)
                    });
                    recordedPurchases++;
                    changed = true;
                }
            }
            version = 3;
            return changed;
        }

        public int Rank(string id)
        {
            CampaignSkillRank item = FindSkill(id, false);
            return item != null ? item.rank : 0;
        }

        public bool IsSkillVisible(CampaignSkill skill)
        {
            if (skill == null) return false;
            if (skill.visibleAtStart || Rank(skill.id) > 0) return true;
            if (skill.revealPrerequisites != null && skill.revealPrerequisites.Length > 0)
                return MeetsSkillPrerequisites(skill.revealPrerequisites, skill.requireAllRevealPrerequisites);
            return skill.prerequisites != null && skill.prerequisites.Length > 0 &&
                MeetsSkillPrerequisites(skill.prerequisites, skill.requireAllPrerequisites);
        }

        public bool MeetsSkillRequirements(CampaignSkill skill) => skill != null &&
            (!skill.requiresFlash || HasFlash) && ConflictingSkill(skill) == null &&
            MeetsSkillPrerequisites(skill.prerequisites, skill.requireAllPrerequisites);

        public CampaignSkill ConflictingSkill(CampaignSkill skill)
        {
            if (skill == null || skill.exclusiveWith == null) return null;
            foreach (CampaignSkill other in skill.exclusiveWith)
                if (other != null && Rank(other.id) > 0) return other;
            return null;
        }

        private bool MeetsSkillPrerequisites(CampaignSkillPrerequisite[] requirements, bool requireAll,
            string changedSkillId = null, int changedRank = 0)
        {
            if (requirements == null || requirements.Length == 0) return true;
            bool anyMet = false;
            foreach (CampaignSkillPrerequisite requirement in requirements)
            {
                bool met = false;
                if (requirement != null && requirement.skill != null)
                {
                    string requiredId = requirement.skill.id;
                    int rank = string.Equals(requiredId, changedSkillId, StringComparison.OrdinalIgnoreCase)
                        ? changedRank : Rank(requiredId);
                    met = rank >= Mathf.Max(1, requirement.minimumRank);
                }
                if (requireAll && !met) return false;
                anyMet |= met;
            }
            return anyMet;
        }

        public bool CanBuy(CampaignSkill skill)
        {
            if (skill == null || string.IsNullOrWhiteSpace(skill.id) || !IsSkillVisible(skill) ||
                !MeetsSkillRequirements(skill)) return false;
            int rank = Rank(skill.id);
            return rank < Mathf.Max(1, skill.maximumRank) && souls >= skill.CostForRank(rank) &&
                (string.IsNullOrWhiteSpace(skill.requiredResourceId) || Resource(skill.requiredResourceId) >= skill.ResourceCostForRank(rank));
        }

        public bool TryBuy(CampaignSkill skill)
        {
            if (!CanBuy(skill)) return false;
            int currentRank = Rank(skill.id);
            int soulCost = skill.CostForRank(currentRank);
            int resourceCost = skill.ResourceCostForRank(currentRank);
            souls -= soulCost;
            soulsInvested += soulCost;
            if (resourceCost > 0 && !string.IsNullOrWhiteSpace(skill.requiredResourceId))
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
            skillPurchases ??= new List<CampaignSkillPurchase>();
            skillPurchases.Add(new CampaignSkillPurchase
            {
                skillId = skill.id,
                soulsPaid = soulCost,
                resourceId = skill.requiredResourceId,
                resourcePaid = resourceCost
            });
            FindSkill(skill.id, true).rank++;
            return true;
        }

        public bool HasRefundableRank(CampaignSkill skill) => LastPurchaseIndex(skill) >= 0;

        private int LastPurchaseIndex(CampaignSkill skill)
        {
            if (skill == null || skillPurchases == null || Rank(skill.id) <= FreeSkillRank(skill.id)) return -1;
            return skillPurchases.FindLastIndex(purchase => purchase != null &&
                string.Equals(purchase.skillId, skill.id, StringComparison.OrdinalIgnoreCase));
        }

        public CampaignSkill GetRefundBlocker(CampaignSkill skill, IEnumerable<CampaignSkill> skills)
        {
            if (skill == null || skills == null) return null;
            int remainingRank = Rank(skill.id) - 1;
            bool losesFlash = remainingRank <= 0 && string.Equals(skill.id, FlashSkillId, StringComparison.OrdinalIgnoreCase);
            foreach (CampaignSkill owned in skills)
            {
                if (owned == null || owned == skill || Rank(owned.id) <= 0) continue;
                if ((losesFlash && owned.requiresFlash) ||
                    (MeetsSkillPrerequisites(owned.prerequisites, owned.requireAllPrerequisites) &&
                     !MeetsSkillPrerequisites(owned.prerequisites, owned.requireAllPrerequisites, skill.id, remainingRank)))
                    return owned;
            }
            return null;
        }

        public bool TryRefundRank(CampaignSkill skill, IEnumerable<CampaignSkill> skills)
        {
            int index = LastPurchaseIndex(skill);
            if (index < 0 || GetRefundBlocker(skill, skills) != null) return false;

            RefundPurchase(skillPurchases[index]);
            skillPurchases.RemoveAt(index);
            CampaignSkillRank rank = FindSkill(skill.id, false);
            if (--rank.rank == 0) skillRanks.Remove(rank);
            return true;
        }

        private void RefundPurchase(CampaignSkillPurchase purchase)
        {
            int refundedSouls = Mathf.Max(0, purchase.soulsPaid);
            souls += refundedSouls;
            soulsInvested = Mathf.Max(0, soulsInvested - refundedSouls);
            if (string.IsNullOrWhiteSpace(purchase.resourceId) || purchase.resourcePaid <= 0) return;

            AddResource(purchase.resourceId, purchase.resourcePaid);
            CampaignResourceAmount invested = FindInList(investedResources, purchase.resourceId);
            if (invested != null) invested.amount = Mathf.Max(0, invested.amount - purchase.resourcePaid);
        }

        public void ResetCampaignSkills(IEnumerable<CampaignSkill> skills)
        {
            EnsurePurchaseLedger(skills);
            HashSet<string> resetIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (skillPurchases != null)
            {
                foreach (CampaignSkillPurchase purchase in skillPurchases)
                {
                    if (purchase == null) continue;
                    if (!string.IsNullOrWhiteSpace(purchase.skillId)) resetIds.Add(purchase.skillId);
                    RefundPurchase(purchase);
                }
                skillPurchases.Clear();
            }

            foreach (string id in resetIds)
            {
                CampaignSkillRank rank = FindSkill(id, false);
                if (rank == null) continue;
                rank.rank = FreeSkillRank(id);
                if (rank.rank <= 0) skillRanks.Remove(rank);
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

        private CampaignSkillGrant FindGrant(string id, bool create)
        {
            freeSkillGrants ??= new List<CampaignSkillGrant>();
            foreach (CampaignSkillGrant item in freeSkillGrants)
                if (item != null && string.Equals(item.skillId, id, StringComparison.OrdinalIgnoreCase)) return item;
            if (!create) return null;
            CampaignSkillGrant created = new CampaignSkillGrant { skillId = id };
            freeSkillGrants.Add(created);
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
