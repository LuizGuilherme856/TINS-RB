using UnityEngine;
using TINS.Campaign;
using MoreMountains.Feedbacks;

/// <summary>Simple generated blue-crystal pickup; no external art asset required.</summary>
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public sealed class BlueCrystalPickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int value = 1;
    [SerializeField, Min(0.1f)] private float pickupRadius = 0.8f;
    [SerializeField, Min(0.1f)] private float followLerpSpeed = 12f;
    [SerializeField] private float rotationSpeed = 35f;
    private static Sprite crystalSprite;
    private SpriteRenderer spriteRenderer;
    private Collider2D pickupCollider;
    private Rigidbody2D body;
    private BlueCrystalInventory carrier;
    private int followIndex;
    private float carriedOrbitAngle;
    private bool isCarried;
    private bool isOrbiting;
    private Vector3 orbitCenter;
    private float orbitRadius;
    private float orbitSpeed;
    private float orbitPhase;
    private bool isAbsorbing;
    private float absorbElapsed;
    private float absorbDuration;
    private Vector3 absorbStart;
    private Vector3 absorbTarget;
    private Vector3 absorbStartScale;
    private MMF_Player skillCheckFailurePlayer;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
        pickupCollider = GetComponent<Collider2D>();
        ApplyPickupRadius();
        body = GetComponent<Rigidbody2D>();
        skillCheckFailurePlayer = GetComponent<MMF_Player>();

        if (spriteRenderer != null)
        {
            // Keep artwork assigned on the prefab. Generate a simple fallback only
            // when no sprite was assigned, and preserve the Inspector tint.
            if (spriteRenderer.sprite == null)
                spriteRenderer.sprite = GetCrystalSprite();
            spriteRenderer.sortingOrder = 20;
        }
    }

    private void OnValidate()
    {
        pickupRadius = Mathf.Max(0.1f, pickupRadius);
        ApplyPickupRadius();
    }

    private void ApplyPickupRadius()
    {
        CircleCollider2D circle = pickupCollider as CircleCollider2D;
        if (circle == null)
            circle = GetComponent<CircleCollider2D>();
        if (circle != null)
            circle.radius = Mathf.Max(0.1f, pickupRadius);
    }

    public static void Spawn(Vector3 position, int value = 1)
    {
        BlueCrystalPickup prefab = Resources.Load<BlueCrystalPickup>("BlueCrystal");
        if (prefab == null)
        {
            Debug.LogError("Blue crystal prefab is missing. Expected Assets/TINS - New Mechanics/Resources/BlueCrystal.prefab.");
            return;
        }

        BlueCrystalPickup pickup = Instantiate(prefab, position, prefab.transform.rotation);
        pickup.value = Mathf.Max(1, value);
        pickup.name = "Blue Crystal";
    }

    private static Sprite GetCrystalSprite()
    {
        if (crystalSprite == null)
            crystalSprite = Sprite.Create(Texture2D.whiteTexture,
                new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                new Vector2(0.5f, 0.5f), 1f);
        return crystalSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCarried) return;

        var inventory = other.GetComponentInParent<BlueCrystalInventory>();
        if (inventory == null) return;
        inventory.Collect(this, value);
    }

    public void SetAsCarried(BlueCrystalInventory inventory, int index)
    {
        bool joiningOrbit = inventory != carrier || !isCarried || isOrbiting || isAbsorbing;
        carrier = inventory;
        followIndex = index;
        isCarried = true;
        isOrbiting = false;
        isAbsorbing = false;

        if (pickupCollider != null)
            pickupCollider.enabled = false;
        if (body != null)
            body.simulated = false;
        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = 25 + index;

        if (joiningOrbit && carrier != null)
            carriedOrbitAngle = carrier.GetPlayerOrbitPhase(index);
    }

    public void SetOrbit(Vector3 center, float radius, float degreesPerSecond, float phase)
    {
        isOrbiting = true;
        orbitCenter = center;
        orbitRadius = Mathf.Max(0f, radius);
        orbitSpeed = degreesPerSecond;
        orbitPhase = phase;
    }

    public void BeginAbsorption(Vector3 center, float duration)
    {
        isOrbiting = false;
        isAbsorbing = true;
        absorbElapsed = 0f;
        absorbDuration = Mathf.Max(0.01f, duration);
        absorbStart = transform.position;
        absorbTarget = center;
        absorbStartScale = transform.localScale;
    }

    public void PlaySkillCheckFailureFeedback()
    {
        if (skillCheckFailurePlayer != null)
            skillCheckFailurePlayer.PlayFeedbacks(transform.position);
    }

    public void PlayCollectSound()
    {
        TowerAudioFeedbacks.Instance?.PlayCrystalPickup();
    }

    private void LateUpdate()
    {
        if (!isCarried)
        {
            AttractToCollector();
            return;
        }
        if (carrier == null) return;

        if (isAbsorbing)
        {
            absorbElapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(absorbElapsed / absorbDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            transform.position = Vector3.LerpUnclamped(absorbStart, absorbTarget, eased);
            transform.localScale = Vector3.LerpUnclamped(absorbStartScale, Vector3.zero, eased);
            return;
        }

        if (isOrbiting)
        {
            float angle = (Time.unscaledTime * orbitSpeed + orbitPhase) * Mathf.Deg2Rad;
            Vector3 orbitTarget = orbitCenter + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * orbitRadius;
            transform.position = InterpolateTowards(transform.position, orbitTarget, Time.unscaledDeltaTime);
            transform.Rotate(0f, 0f, rotationSpeed * Time.unscaledDeltaTime);
            return;
        }

        carriedOrbitAngle = Mathf.Repeat(carriedOrbitAngle + carrier.GetPlayerOrbitSpeed(followIndex) * Time.deltaTime, 360f);
        float radians = carriedOrbitAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * carrier.PlayerOrbitRadius;
        transform.position = carrier.transform.position + offset;
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    private void AttractToCollector()
    {
        CampaignMissionRuntime mission = CampaignMissionRuntime.Active;
        BlueCrystalInventory target = mission?.CrystalCollector;
        if (target == null || !target.isActiveAndEnabled) return;

        float reachBonus = mission.SkillEffect(CampaignSkillEffectType.CrystalMagnet).x;
        if (reachBonus <= 0f) return;
        Vector2 offset = (Vector2)(target.transform.position - transform.position);
        float attractionRadius = pickupRadius + reachBonus;
        if (offset.sqrMagnitude > attractionRadius * attractionRadius) return;

        if (offset.sqrMagnitude <= pickupRadius * pickupRadius) target.Collect(this, value);
        else if (body != null) body.position = Vector2.MoveTowards(body.position, target.transform.position, 9f * Time.deltaTime);
    }

    private Vector3 InterpolateTowards(Vector3 current, Vector3 target, float deltaTime)
    {
        float t = 1f - Mathf.Exp(-Mathf.Max(0.1f, followLerpSpeed) * deltaTime);
        return Vector3.Lerp(current, target, t);
    }
}
