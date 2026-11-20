using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TINS.Campaign;

/// <summary>
/// Projects a cone of light that reveals the arena and stuns enemies in missions.
/// </summary>
public sealed class BeamMeleeAttack : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInput;
    [SerializeField] private Transform beamOrigin;
    [SerializeField] private GameObject beamPrefab;
    [SerializeField, Min(0.05f)] private float duration = 0.5f;
    [SerializeField, Min(0.1f)] private float stunDuration = 1.5f;
    [SerializeField] private BeamMeleeUsesManager usesManager;
    [SerializeField] private GameObject coneLightPrefab;

    [Header("Cone Light")]
    [SerializeField, Range(5f, 180f), Tooltip("Abertura total do cone, em graus, antes das melhorias.")]
    private float baseConeAngle = 36f;
    [SerializeField, Min(0.1f), Tooltip("Distância do flash, em unidades de mundo, antes das melhorias.")]
    private float baseConeRange = 7f;
    [SerializeField, Min(0f)] private float anglePerAreaRank = 8f;
    [SerializeField, Min(0f)] private float rangeFractionPerAreaRank = 0.12f;
    [SerializeField] private bool enableConeLight = true;
    [SerializeField, Range(3, 64)] private int hitboxArcSegments = 24;

    private readonly List<ActiveBeam> activeBeams = new List<ActiveBeam>();
    private float campaignDurationMultiplier = 1f;
    private int areaRank;
    private float campaignAngleBonus;
    private float campaignRangeFractionBonus;

    private sealed class ActiveBeam
    {
        public Transform transform;
        public Light2D coneLight;
        public float coneRange;
        public float coneAngle;
        public float startTime;
        public HashSet<EnemyHealth> repelledEnemies;
    }

    private void Awake()
    {
        if (playerInput == null) playerInput = GetComponentInParent<PlayerInputHandler>();
        if (beamOrigin == null) beamOrigin = transform;
    }

    public bool FireBeam()
    {
        if (usesManager == null || !usesManager.isActiveAndEnabled || beamPrefab == null || beamOrigin == null) return false;
        if (enableConeLight && coneLightPrefab == null)
        {
            Debug.LogError("Beam melee has Cone Light enabled but no cone prefab assigned.", this);
            return false;
        }

        GameObject beam = Instantiate(beamPrefab);
        PlayerMeleeAttackSource attackSource = beam.GetComponent<PlayerMeleeAttackSource>();
        Health playerHealth = GetComponentInParent<Health>();
        if (attackSource == null || playerHealth == null)
        {
            Debug.LogError("Cone attack setup requires PlayerMeleeAttackSource and the player Health component.", beamPrefab);
            Destroy(beam);
            return false;
        }

        if (!usesManager.TryConsumeUse())
        {
            Destroy(beam);
            if (!usesManager.LastNoChargeAttemptResolvedSkillCheck)
                TowerAudioFeedbacks.Instance?.PlayNoCharges();
            return false;
        }

        TowerAudioFeedbacks.Instance?.PlayBeamAttack();

        // The beam only removes enemy protection; it is deliberately not configured
        // with damage stats and its prefab must not use the "Melee" damage tag.
        attackSource.Configure(playerHealth, PlayerAction.melee);

        SpriteRenderer spriteRenderer = beam.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        Collider2D[] oldColliders = beam.GetComponents<Collider2D>();
        for (int i = 0; i < oldColliders.Length; i++)
        {
            oldColliders[i].enabled = false;
            Destroy(oldColliders[i]);
        }

        PolygonCollider2D hitbox = beam.AddComponent<PolygonCollider2D>();
        hitbox.isTrigger = true;
        Light2D coneLight = null;
        float rangeFraction = campaignRangeFractionBonus + rangeFractionPerAreaRank * areaRank;
        float angleBonus = campaignAngleBonus + anglePerAreaRank * areaRank;
        float range = Mathf.Max(0.1f, baseConeRange * (1f + rangeFraction));
        float angle = Mathf.Clamp(baseConeAngle + angleBonus, 5f, 180f);
        if (enableConeLight)
        {
            GameObject coneObject = Instantiate(coneLightPrefab);
            coneLight = coneObject.GetComponent<Light2D>();
            if (coneLight == null)
            {
                Debug.LogError("Cone light prefab must have a Light2D component on its root.", coneLightPrefab);
                Destroy(coneObject);
                Destroy(beam);
                return false;
            }

            float innerRadiusRatio = coneLight.pointLightInnerRadius / Mathf.Max(0.1f, coneLight.pointLightOuterRadius);
            float innerAngleRatio = coneLight.pointLightInnerAngle / Mathf.Max(5f, coneLight.pointLightOuterAngle);
            coneLight.pointLightOuterRadius = range;
            coneLight.pointLightInnerRadius = range * Mathf.Clamp01(innerRadiusRatio);
            coneLight.pointLightOuterAngle = angle;
            coneLight.pointLightInnerAngle = angle * Mathf.Clamp01(innerAngleRatio);
        }
        SetConeHitbox(hitbox, range, angle);
        float pushSpeed = CampaignMissionRuntime.Active?.SkillEffect(CampaignSkillEffectType.FlashRepulsion).x ?? 0f;
        activeBeams.Add(new ActiveBeam
        {
            transform = beam.transform,
            coneLight = coneLight,
            coneRange = range,
            coneAngle = angle,
            startTime = Time.time,
            repelledEnemies = pushSpeed > 0f ? new HashSet<EnemyHealth>() : null
        });

        UpdateBeam(activeBeams[activeBeams.Count - 1]);
        CampaignMissionRuntime.Active?.DeployFlashTurret(transform.position);
        return true;
    }

    public void SetCampaignDurationMultiplier(float multiplier)
    {
        campaignDurationMultiplier = Mathf.Max(0.1f, multiplier);
    }

    public void SetCampaignArea(float angleBonus, float rangeFractionBonus)
    {
        areaRank = 0;
        campaignAngleBonus = Mathf.Max(0f, angleBonus);
        campaignRangeFractionBonus = Mathf.Max(0f, rangeFractionBonus);
    }

    public void SetCampaignAreaRank(int rank)
    {
        areaRank = Mathf.Max(0, rank);
        campaignAngleBonus = 0f;
        campaignRangeFractionBonus = 0f;
    }

    private void Update()
    {
        for (int i = activeBeams.Count - 1; i >= 0; i--)
        {
            ActiveBeam beam = activeBeams[i];
            if (beam.transform == null || Time.time - beam.startTime >= duration * campaignDurationMultiplier)
            {
                if (beam.transform != null) Destroy(beam.transform.gameObject);
                if (beam.coneLight != null) Destroy(beam.coneLight.gameObject);
                activeBeams.RemoveAt(i);
                continue;
            }

            UpdateBeam(beam);
        }

    }

    private void UpdateBeam(ActiveBeam beam)
    {
        Vector3 origin = beamOrigin.position;
        Vector2 direction;

        if (playerInput != null && playerInput.isUsingController)
        {
            float angle = playerInput.GetAimAngle() * Mathf.Deg2Rad;
            direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
        else if (Camera.main != null)
        {
            Camera camera = Camera.main;
            float depth = Mathf.Abs(camera.transform.position.z - origin.z);
            Vector3 mouseWorld = camera.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth));
            Vector2 toMouse = (Vector2)(mouseWorld - origin);
            direction = toMouse.sqrMagnitude > 0.000001f ? toMouse.normalized : Vector2.right;
        }
        else
        {
            float angle = playerInput != null ? playerInput.GetAimAngle() * Mathf.Deg2Rad : 0f;
            direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        float angleDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        beam.transform.SetPositionAndRotation(
            origin,
            Quaternion.Euler(0f, 0f, angleDegrees));
        beam.transform.localScale = Vector3.one;

        if (CampaignCombatRules.IsCampaignMission)
            StunEnemiesInCone(origin, direction, beam);

        if (beam.coneLight != null)
        {
            Transform lightTransform = beam.coneLight.transform;
            // URP 2D point-light arcs are authored along local +Y; the beam sprite
            // uses local +X, so compensate by 90 degrees to keep them aligned.
            lightTransform.SetPositionAndRotation(origin, Quaternion.Euler(0f, 0f, angleDegrees - 90f));
        }
    }

    public bool TryGetActiveCone(out Vector3 origin, out Vector2 direction, out float range, out float halfAngle)
    {
        for (int i = activeBeams.Count - 1; i >= 0; i--)
        {
            ActiveBeam beam = activeBeams[i];
            if (beam.transform == null || Time.time - beam.startTime >= duration * campaignDurationMultiplier) continue;
            origin = beam.transform.position;
            direction = beam.transform.right;
            range = beam.coneRange;
            halfAngle = beam.coneAngle * 0.5f;
            return true;
        }
        origin = Vector3.zero;
        direction = Vector2.right;
        range = 0f;
        halfAngle = 0f;
        return false;
    }

    private void StunEnemiesInCone(Vector3 origin, Vector2 direction, ActiveBeam beam)
    {
        float range = beam.coneRange;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, range);
        float halfAngle = beam.coneAngle * 0.5f;
        CampaignMissionRuntime mission = CampaignMissionRuntime.Active;
        float pushSpeed = mission?.SkillEffect(CampaignSkillEffectType.FlashRepulsion).x ?? 0f;
        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy == null || enemy.currentHealth <= 0f) continue;
            Vector2 offset = (Vector2)enemy.transform.position - (Vector2)origin;
            Vector2 closestOffset = hit.ClosestPoint(origin) - (Vector2)origin;
            bool centerInCone = offset.sqrMagnitude <= range * range && Vector2.Angle(direction, offset) <= halfAngle;
            bool colliderInCone = closestOffset.sqrMagnitude <= range * range && Vector2.Angle(direction, closestOffset) <= halfAngle;
            // Flash concussion is guaranteed on a cone hit, including against status-immune enemies.
            if (!centerInCone && !colliderInCone) continue;
            enemy.ApplyCampaignFlashStun(stunDuration * campaignDurationMultiplier);
            if (beam.repelledEnemies != null && beam.repelledEnemies.Add(enemy))
            {
                Vector2 away = mission?.DefenseCore != null
                    ? (Vector2)(enemy.transform.position - mission.DefenseCore.position) : offset;
                enemy.ApplyCampaignFlashRepulsion(away.sqrMagnitude > .001f ? away : direction, pushSpeed);
            }
        }
    }

    private void SetConeHitbox(PolygonCollider2D hitbox, float range, float angle)
    {
        if (hitbox == null) return;

        int segments = Mathf.Clamp(hitboxArcSegments, 3, 64);
        Vector2[] points = new Vector2[segments + 2];
        points[0] = Vector2.zero;
        float halfAngle = angle * 0.5f;
        for (int i = 0; i <= segments; i++)
        {
            float degrees = Mathf.Lerp(-halfAngle, halfAngle, i / (float)segments);
            float radians = degrees * Mathf.Deg2Rad;
            points[i + 1] = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * range;
        }
        hitbox.pathCount = 1;
        hitbox.SetPath(0, points);
    }

    private void OnDrawGizmosSelected()
    {
        Transform source = beamOrigin != null ? beamOrigin : transform;
        Vector3 origin = source.position;
        float halfAngle = baseConeAngle * 0.5f;
        Vector3 previous = origin;
        Gizmos.color = new Color(0.25f, 0.9f, 1f, 0.85f);
        for (int i = 0; i <= 24; i++)
        {
            float angle = Mathf.Lerp(-halfAngle, halfAngle, i / 24f);
            Vector3 point = origin + Quaternion.AngleAxis(angle, Vector3.forward) * source.right * baseConeRange;
            Gizmos.DrawLine(previous, point);
            previous = point;
        }
        Gizmos.DrawLine(previous, origin);
    }

    private void OnDisable()
    {
        for (int i = 0; i < activeBeams.Count; i++)
        {
            if (activeBeams[i].transform != null)
                Destroy(activeBeams[i].transform.gameObject);
            if (activeBeams[i].coneLight != null)
                Destroy(activeBeams[i].coneLight.gameObject);
        }
        activeBeams.Clear();
    }
}
