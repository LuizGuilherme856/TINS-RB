using System.Collections.Generic;
using UnityEngine;

namespace TINS.Campaign
{
    /// <summary>Draws the three pixel-stepped levels: Iluminado, Penumbra and Breu; the Breu edge blocks the player.</summary>
    [DefaultExecutionOrder(1000)]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class CampaignDarknessBands : MonoBehaviour
    {
        [SerializeField] private PurificationReveal lightReveal;
        [SerializeField] private Transform player;
        [SerializeField] private Rigidbody2D playerBody;
        [SerializeField] private Camera targetCamera;
        [SerializeField, Min(0.1f)] private float penumbraWidth = 2.5f;
        [SerializeField, Min(0.01f)] private float pixelStepWorld = 0.0625f;
        [SerializeField, Range(0f, 1f)] private float penumbraOpacity = 0.62f;
        [SerializeField] private Material darknessMaterial;
        [SerializeField] private Transform legacyFloorRoot;
        [SerializeField] private Renderer[] legacyGroundTilemaps;

        private static readonly int CenterId = Shader.PropertyToID("_Center");
        private static readonly int LightRadiusId = Shader.PropertyToID("_LightRadius");
        private static readonly int PenumbraWidthId = Shader.PropertyToID("_PenumbraWidth");
        private static readonly int BandSizeId = Shader.PropertyToID("_BandSize");
        private static readonly int FlashEnabledId = Shader.PropertyToID("_FlashEnabled");
        private static readonly int FlashOriginId = Shader.PropertyToID("_FlashOrigin");
        private static readonly int FlashDirectionId = Shader.PropertyToID("_FlashDirection");
        private static readonly int FlashRangeId = Shader.PropertyToID("_FlashRange");
        private static readonly int FlashCosAngleId = Shader.PropertyToID("_FlashCosAngle");
        private static readonly int TerrainTilesId = Shader.PropertyToID("_TerrainTiles");
        private static readonly int TerrainAtlasSizeId = Shader.PropertyToID("_TerrainAtlasSize");
        private static readonly int TerrainEnabledId = Shader.PropertyToID("_TerrainEnabled");
        private static readonly int TerrainOnlyId = Shader.PropertyToID("_TerrainOnly");
        private static readonly int PurifiedRadiusId = Shader.PropertyToID("_PurifiedRadius");

        private static readonly int PurificationWaveId = Shader.PropertyToID("_PurificationWave");
        private Vector4 purificationWave;

        public void SetPurificationWave(float radius, float width, float strength)
        {
            purificationWave = new Vector4(radius, width, strength, 0f);
        }

        private SpriteRenderer overlay;
        private SpriteRenderer terrain;
        private readonly List<Renderer> hiddenFloor = new();
        private MaterialPropertyBlock properties;
        private Sprite generatedSprite;
        private bool ownsDarknessMaterial;
        private BeamMeleeAttack[] flashSources;
        private float nextFlashSearch;

        private CampaignPlanet configuredPlanet;
        private float CurrentPenumbraWidth => configuredPlanet != null
            ? configuredPlanet.PenumbraWidthAt(lightReveal != null ? lightReveal.Radius : configuredPlanet.initialLightRadius)
            : Mathf.Max(0.1f, penumbraWidth);

        public float BreuRadius => (lightReveal != null ? lightReveal.Radius : 0f) + CurrentPenumbraWidth;

        public void ConfigurePlanet(CampaignPlanet planet)
        {
            configuredPlanet = planet;
            bool useTerrain = planet?.floorTiles != null;
            if (useTerrain && terrain == null)
            {
                var ground = new GameObject("Mission terrain");
                ground.transform.SetParent(transform, false);
                terrain = ground.AddComponent<SpriteRenderer>();
                terrain.sprite = overlay.sprite;
                terrain.sharedMaterial = darknessMaterial;
                terrain.sortingOrder = -4;
            }
            if (terrain != null) terrain.enabled = useTerrain;

            if (!useTerrain)
            {
                RestoreLegacyFloor();
            }
            else if (hiddenFloor.Count == 0)
            {
                if (legacyFloorRoot != null)
                    foreach (var renderer in legacyFloorRoot.GetComponentsInChildren<SpriteRenderer>(true))
                        if (renderer.name == "Purified Floor" || renderer.name == "Corrupted Floor")
                            HideLegacyFloor(renderer);
                if (legacyGroundTilemaps != null)
                    foreach (var tilemap in legacyGroundTilemaps) HideLegacyFloor(tilemap);
            }
        }

        private void HideLegacyFloor(Renderer renderer)
        {
            if (renderer == null || !renderer.enabled) return;
            renderer.enabled = false;
            hiddenFloor.Add(renderer);
        }

        private void RestoreLegacyFloor()
        {
            foreach (var renderer in hiddenFloor)
                if (renderer != null) renderer.enabled = true;
            hiddenFloor.Clear();
        }

        private void Awake()
        {
            overlay = GetComponent<SpriteRenderer>();
            if (targetCamera == null) targetCamera = Camera.main;
            if (lightReveal == null) lightReveal = FindFirstObjectByType<PurificationReveal>();
            if (player == null)
            {
                PlayerInputHandler input = FindFirstObjectByType<PlayerInputHandler>();
                if (input != null) player = input.transform;
            }
            if (playerBody == null && player != null) playerBody = player.GetComponent<Rigidbody2D>();
            if (properties == null) properties = new MaterialPropertyBlock();
            if (overlay.sprite == null)
            {
                Texture2D texture = Texture2D.whiteTexture;
                generatedSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1f);
                overlay.sprite = generatedSprite;
            }
            overlay.color = Color.white;
            overlay.sortingLayerName = "EnemyBullet";
            overlay.sortingOrder = short.MaxValue;
            overlay.maskInteraction = SpriteMaskInteraction.None;
            if (darknessMaterial == null)
            {
                Shader shader = Shader.Find("TINS/Campaign/Discrete Darkness Bands");
                if (shader != null)
                {
                    darknessMaterial = new Material(shader);
                    ownsDarknessMaterial = true;
                }
            }
            if (darknessMaterial != null) overlay.sharedMaterial = darknessMaterial;
            flashSources = FindObjectsByType<BeamMeleeAttack>(FindObjectsSortMode.None);
        }

        private void LateUpdate()
        {
            ResolveReferences();
            FitToCamera();
            ClampPlayerAtBreu();
            UpdateMaterialProperties();
        }

        private void ResolveReferences()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            if (lightReveal == null) lightReveal = FindFirstObjectByType<PurificationReveal>();
            if (player == null)
            {
                PlayerInputHandler input = FindFirstObjectByType<PlayerInputHandler>();
                if (input != null) player = input.transform;
            }
            if (playerBody == null && player != null) playerBody = player.GetComponent<Rigidbody2D>();
            if (flashSources == null || flashSources.Length == 0 || Time.unscaledTime >= nextFlashSearch)
            {
                nextFlashSearch = Time.unscaledTime + 1f;
                flashSources = FindObjectsByType<BeamMeleeAttack>(FindObjectsSortMode.None);
            }
        }

        private void FitToCamera()
        {
            if (targetCamera == null || !targetCamera.orthographic) return;
            transform.SetPositionAndRotation(targetCamera.transform.position + targetCamera.transform.forward * (targetCamera.nearClipPlane + 0.02f), targetCamera.transform.rotation);
            transform.localScale = new Vector3(targetCamera.orthographicSize * 2f * targetCamera.aspect, targetCamera.orthographicSize * 2f, 1f);
        }

        private void ClampPlayerAtBreu()
        {
            if (player == null || lightReveal == null) return;
            Vector2 center = lightReveal.transform.position;
            Vector2 offset = (Vector2)player.position - center;
            float limit = Mathf.Max(0.1f, BreuRadius - 0.05f);
            if (offset.sqrMagnitude <= limit * limit) return;
            Vector2 corrected = center + offset.normalized * limit;
            player.position = new Vector3(corrected.x, corrected.y, player.position.z);
            if (playerBody != null)
            {
                playerBody.position = corrected;
                Vector2 velocity = playerBody.linearVelocity;
                if (Vector2.Dot(velocity, offset.normalized) > 0f)
                    playerBody.linearVelocity = velocity - Vector2.Dot(velocity, offset.normalized) * offset.normalized;
            }
        }

        private void UpdateMaterialProperties()
        {
            if (overlay == null || darknessMaterial == null) return;
            Vector2 center = lightReveal != null ? (Vector2)lightReveal.transform.position : Vector2.zero;
            float radius = lightReveal != null ? lightReveal.Radius : 0f;
            float width = CurrentPenumbraWidth;
            overlay.GetPropertyBlock(properties);
            properties.SetVector(CenterId, center);
            properties.SetFloat(LightRadiusId, radius);
            properties.SetFloat(PenumbraWidthId, width);
            properties.SetFloat(BandSizeId, Mathf.Max(0.01f, pixelStepWorld));
            properties.SetColor("_Color", Color.black);
            Vector3 flashOrigin = Vector3.zero;
            Vector2 flashDirection = Vector2.right;
            float flashRange = 0f;
            float halfAngle = 0f;
            bool flashActive = false;
            if (flashSources != null)
                foreach (BeamMeleeAttack flash in flashSources)
                    if (flash != null && flash.TryGetActiveCone(out flashOrigin, out flashDirection, out flashRange, out halfAngle))
                    {
                        flashActive = true;
                        break;
                    }
            properties.SetFloat(FlashEnabledId, flashActive ? 1f : 0f);
            properties.SetVector(FlashOriginId, flashOrigin);
            properties.SetVector(FlashDirectionId, flashDirection);
            properties.SetFloat(FlashRangeId, flashRange);
            properties.SetFloat(FlashCosAngleId, Mathf.Cos(halfAngle * Mathf.Deg2Rad));
            properties.SetFloat("_PenumbraOpacity", Mathf.Clamp01(penumbraOpacity));
            properties.SetVector(PurificationWaveId, purificationWave);
            Texture2D tiles = configuredPlanet?.floorTiles;
            bool useTerrain = terrain != null && terrain.enabled && tiles != null;
            properties.SetFloat(TerrainEnabledId, useTerrain ? 1f : 0f);
            properties.SetFloat(TerrainOnlyId, 0f);
            if (useTerrain)
            {
                properties.SetTexture(TerrainTilesId, tiles);
                properties.SetVector(TerrainAtlasSizeId, new Vector4(tiles.width, tiles.height, 0f, 0f));
                properties.SetFloat(PurifiedRadiusId, radius * configuredPlanet.purifiedGrassRadiusRatio);
            }
            overlay.SetPropertyBlock(properties);
            if (useTerrain)
            {
                properties.SetFloat(TerrainOnlyId, 1f);
                terrain.SetPropertyBlock(properties);
            }
        }

        private void OnDestroy()
        {
            RestoreLegacyFloor();
            if (generatedSprite != null) Destroy(generatedSprite);
            if (ownsDarknessMaterial && darknessMaterial != null) Destroy(darknessMaterial);
        }
    }
}
