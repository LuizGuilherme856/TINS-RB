using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace TINS.Campaign
{
    /// <summary>Owns the traps and tower orbitals deployed by the purchased mission upgrades.</summary>
    public sealed class CampaignDefenseUpgrades : MonoBehaviour
    {
        private readonly List<GameObject> guardianBlades = new();
        private TowerController tower;
        private GameObject decoyPrefab;
        private GameObject turretPrefab;
        private GameObject activeDecoy;
        private GameObject activeTurret;
        private float decoyCooldown;
        private float turretCooldown;
        private float nextDecoy;
        private float nextTurret;

        public void Configure(TowerController defenseCore, CampaignSkill skill, Vector2 effect)
        {
            tower = defenseCore;
            switch (skill.effectType)
            {
                case CampaignSkillEffectType.MonkeyDecoy:
                    decoyPrefab = skill.effectPrefab;
                    decoyCooldown = effect.x;
                    break;
                case CampaignSkillEffectType.BladeTurret:
                    turretPrefab = skill.effectPrefab;
                    turretCooldown = effect.x;
                    break;
                case CampaignSkillEffectType.GuardianBlades:
                    CreateBlades(skill.orbitalEffect, Mathf.RoundToInt(effect.x), Mathf.Max(.1f, skill.secondaryEffectPerRank));
                    break;
            }
        }

        public void DeployDecoy(Vector3 position)
        {
            if (decoyPrefab == null || activeDecoy != null || Time.time < nextDecoy) return;
            activeDecoy = Instantiate(decoyPrefab, position, Quaternion.identity);
            nextDecoy = Time.time + decoyCooldown;
        }

        public void DeployTurret(Vector3 position)
        {
            if (turretPrefab == null || activeTurret != null || Time.time < nextTurret) return;
            activeTurret = Instantiate(turretPrefab, position, Quaternion.identity);
            nextTurret = Time.time + turretCooldown;
        }

        private void CreateBlades(OrbitalUpgrade_ActiveEffect definition, int count, float radius)
        {
            MeleeData weapon = definition != null ? definition.meleeDataToOrbit : null;
            if (tower == null || weapon == null || weapon.meleeGameObject == null || count <= 0) return;

            tower.Damaged -= LoseBlade;
            tower.Damaged += LoseBlade;
            for (int i = 0; i < count; i++)
            {
                float phase = i * 2f * Mathf.PI / count;
                Vector3 offset = new Vector3(Mathf.Cos(phase), Mathf.Sin(phase)) * radius;
                GameObject blade = Instantiate(weapon.meleeGameObject, tower.transform.position + offset, Quaternion.identity);
                if (blade.TryGetComponent(out OrbitalUpgradeHolder holder))
                    holder.orbitalUpgrade_ActiveEffect = definition;
                if (blade.TryGetComponent(out MeleeStats stats))
                    stats.weaponData = weapon;
                if (blade.TryGetComponent(out MMAutoRotate orbit))
                {
                    orbit.OrbitCenterTransform = tower.transform;
                    orbit.OrbitRadius = radius;
                    orbit.OrbitRotationSpeed = 145f;
                    orbit.OrbitCorrectionSpeed = definition.OrbitCorrectionSpeed;
                    orbit.Orbit(true);
                }
                guardianBlades.Add(blade);
            }
        }

        private void LoseBlade()
        {
            guardianBlades.RemoveAll(blade => blade == null);
            if (guardianBlades.Count == 0) return;
            int last = guardianBlades.Count - 1;
            RemoveDefense(guardianBlades[last]);
            guardianBlades.RemoveAt(last);
        }

        public void StopDefenses()
        {
            if (tower != null) tower.Damaged -= LoseBlade;
            RemoveDefense(activeDecoy);
            RemoveDefense(activeTurret);
            foreach (GameObject blade in guardianBlades) RemoveDefense(blade);
            guardianBlades.Clear();
        }

        private static void RemoveDefense(GameObject defense)
        {
            if (defense == null) return;
            defense.SetActive(false);
            Destroy(defense);
        }
    }
}
