using UnityEngine;

namespace TINS.Campaign
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class CampaignPlanetWindow : MonoBehaviour
    {
        private const float Duration = CampaignPlanetRotation.FrameCount * CampaignPlanetRotation.SecondsPerFrame;
        private SpriteRenderer planetRenderer;
        private CampaignPlanetRotation rotation;
        private float elapsed;
        private int displayedFrame;

        private void Awake() => planetRenderer = GetComponent<SpriteRenderer>();

        public void Show(CampaignCatalog catalog, CampaignProfile profile)
        {
            rotation?.Release();
            CampaignPlanet planet = catalog.GetPlanet(profile.lastVisitedPlanetId);
            if (planet == null && string.IsNullOrEmpty(profile.lastVisitedPlanetId))
            {
                // Older profiles remember completion records, but don't have a travel history.
                CampaignPlanetProgress latest = profile.planetProgress.FindLast(progress => progress != null && progress.missionsCompleted > 0);
                planet = catalog.GetPlanet(latest?.planetId);
                if (planet == null && profile.firstExpeditionSettled && catalog.planets is { Length: > 0 })
                    planet = catalog.planets[0];
            }

            rotation = planet != null
                ? new CampaignPlanetRotation(planet.id, CampaignPlanetRotation.StageFor(profile.MissionsOn(planet.id), planet.missionsToPurify))
                : null;
            elapsed = 0f;
            displayedFrame = 0;
            planetRenderer.sprite = rotation?.Frame(0);
            planetRenderer.enabled = planetRenderer.sprite != null;
        }

        private void Update()
        {
            if (!planetRenderer.enabled) return;
            elapsed = Mathf.Repeat(elapsed + Time.unscaledDeltaTime, Duration);
            int frame = Mathf.Min(CampaignPlanetRotation.FrameCount - 1,
                Mathf.FloorToInt(elapsed / CampaignPlanetRotation.SecondsPerFrame));
            if (frame == displayedFrame) return;
            planetRenderer.sprite = rotation.Frame(frame);
            displayedFrame = frame;
        }

        private void OnDestroy() => rotation?.Release();
    }
}
