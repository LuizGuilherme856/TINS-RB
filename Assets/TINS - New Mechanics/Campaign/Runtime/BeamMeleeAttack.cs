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
    [SerializeField, Min(0.1f)] private float maximumLength = 30f;
    [SerializeField] private BeamMeleeUsesManager usesManager;
    [SerializeField] private GameObject coneLightPrefab;

    [Header("Cone Light")]
    [SerializeField] private bool enableConeLight = true;
    [SerializeField, Range(3, 64)] private int hitboxArcSegments = 24;

    private readonly List<ActiveBeam> activeBeams = new List<ActiveBeam>();
    private float campaignDurationMultiplier = 1f;

    private sealed class ActiveBeam
    {
        public Transform transform;
        public PolygonCollider2D hitbox;
        public Light2D coneLight;
        public float coneRange;
        public float coneAngle;
        public float startTime;
        public float activeRange;
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
        float prefabConeRange = 0f;
        float prefabConeAngle = 0f;
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

            prefabConeRange = coneLight.pointLightOuterRadius;
            prefabConeAngle = coneLight.pointLightOuterAngle;
        }
        activeBeams.Add(new ActiveBeam
        {
            transform = beam.transform,
            hitbox = hitbox,
            coneLight = coneLight,
            coneRange = prefabConeRange,
            coneAngle = prefabConeAngle,
            startTime = Time.time
        });

        UpdateBeam(activeBeams[activeBeams.Count - 1]);
        return true;
    }

    public void SetCampaignDurationMultiplier(float multiplier)
    {
        campaignDurationMultiplier = Mathf.Max(0.1f, multiplier);
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
        float length;

        if (playerInput != null && playerInput.isUsingController)
        {
            float angle = playerInput.GetAimAngle() * Mathf.Deg2Rad;
            direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            length = maximumLength;
        }
        else if (Camera.main != null)
        {
            Camera camera = Camera.main;
            float depth = Mathf.Abs(camera.transform.position.z - origin.z);
            Vector3 mouseWorld = camera.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth));
            Vector2 toMouse = (Vector2)(mouseWorld - origin);
            length = maximumLength;
            direction = toMouse.sqrMagnitude > 0.000001f ? toMouse.normalized : Vector2.right;
        }
        else
        {
            float angle = playerInput != null ? playerInput.GetAimAngle() * Mathf.Deg2Rad : 0f;
            direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            length = maximumLength;
        }

        length = Mathf.Clamp(length, 0.05f, maximumLength);
        float angleDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        beam.transform.SetPositionAndRotation(
            origin,
            Quaternion.Euler(0f, 0f, angleDegrees));
        beam.transform.localScale = Vector3.one;

        float hitRange = beam.coneLight != null ? Mathf.Min(beam.coneRange, length) : length;
        float hitAngle = beam.coneLight != null ? beam.coneAngle : 50f;
        beam.activeRange = hitRange;
        SetConeHitbox(beam.hitbox, hitRange, hitAngle);
        if (CampaignCombatRules.IsCampaignMission)
            StunEnemiesInCone(origin, direction, hitRange, hitAngle);

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
            range = beam.activeRange;
            halfAngle = (beam.coneLight != null ? beam.coneAngle : 50f) * 0.5f;
            return true;
        }
        origin = Vector3.zero;
        direction = Vector2.right;
        range = 0f;
        halfAngle = 0f;
        return false;
    }

    private void StunEnemiesInCone(Vector3 origin, Vector2 direction, float range, float angle)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, range);
        float halfAngle = angle * 0.5f;
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
            if (centerInCone || colliderInCone)
                enemy.ApplyCampaignFlashStun(stunDuration * campaignDurationMultiplier);
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
