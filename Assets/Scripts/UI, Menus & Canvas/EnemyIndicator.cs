using UnityEngine;
using System.Collections.Generic;

public class EnemyIndicator : MonoBehaviour
{
    [SerializeField] private RunData runData;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float arrowPadding = 10f;
    [SerializeField] private float updateArrowsInterval = 0;
    [SerializeField] private float smoothSpeed = 5f; // Smoothing speed.
    public EnemyWaveManager waveManager;

    private RectTransform canvasRect;
    private Canvas canvas;
    private Camera mainCamera;
    private const int ENEMY_THRESHOLD = 3;
    private List<GameObject> activeArrows = new List<GameObject>();
    private List<Vector2> targetPositions = new List<Vector2>(); // Target positions.
    private CountEnemies_Helper[] currentEnemies;

    private float timeSinceEnable = 0;
    private float timeSinceLastArrowUpdate = 0;

    private void OnEnable()
    {
        timeSinceEnable = 0;
        timeSinceLastArrowUpdate = 0;
    }

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        timeSinceEnable += Time.deltaTime;
        timeSinceLastArrowUpdate += Time.deltaTime;

        bool canUpdate = runData.timeElapsed_currentWave > 30 && timeSinceEnable > 30 && waveManager.canEnemySpawn;

        if (runData.enemiesAlive <= ENEMY_THRESHOLD && canUpdate)
        {
            if (timeSinceLastArrowUpdate >= updateArrowsInterval)
            {
                UpdateArrows();
                timeSinceLastArrowUpdate = 0;
            }
        }
        else
        {
            HideAllArrows();
        }
    }

    private void LateUpdate()
    {
        // Smooth arrow positions each frame.
        bool canUpdate = runData.timeElapsed_currentWave > 30 && timeSinceEnable > 30f;

        if (runData.enemiesAlive <= ENEMY_THRESHOLD && canUpdate)
        {
            SmoothArrowPositions();
        }
    }

    private void UpdateArrows()
    {
        if (currentEnemies == null || runData.enemiesAlive <= ENEMY_THRESHOLD)
        {
            currentEnemies = FindObjectsByType<CountEnemies_Helper>(FindObjectsSortMode.None);
        }

        // Match arrow count to enemy count.
        while (activeArrows.Count < currentEnemies.Length)
        {
            GameObject newArrow = Instantiate(arrowPrefab, transform);
            activeArrows.Add(newArrow);
            targetPositions.Add(Vector2.zero);
        }

        while (activeArrows.Count > currentEnemies.Length)
        {
            GameObject arrowToRemove = activeArrows[activeArrows.Count - 1];
            activeArrows.RemoveAt(activeArrows.Count - 1);
            targetPositions.RemoveAt(targetPositions.Count - 1);
            Destroy(arrowToRemove);
        }

        // Calculate each arrow's target position.
        for (int i = 0; i < activeArrows.Count; i++)
        {
            targetPositions[i] = CalculateArrowTargetPosition(currentEnemies[i]);
            activeArrows[i].SetActive(true);
        }
    }

    private void SmoothArrowPositions()
    {
        // Interpolate from current to target position.
        for (int i = 0; i < activeArrows.Count && i < targetPositions.Count; i++)
        {
            RectTransform arrowRect = activeArrows[i].GetComponent<RectTransform>();
            if (arrowRect != null)
            {
                Vector2 smoothedPosition = Vector2.Lerp(
                    arrowRect.anchoredPosition,
                    targetPositions[i],
                    smoothSpeed * Time.deltaTime
                );
                arrowRect.anchoredPosition = smoothedPosition;

                // Set rotation directly because it must match exactly.
                if (i < currentEnemies.Length && currentEnemies[i] != null)
                {
                    RotateArrowTowardsEnemy(activeArrows[i].transform, currentEnemies[i].transform.position);
                }
            }
        }
    }

    private Vector2 CalculateArrowTargetPosition(CountEnemies_Helper enemy)
    {
        if (enemy == null)
            return Vector2.zero;

        Vector3 enemyWorldPosition = enemy.transform.position;
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(enemyWorldPosition);

        // Invert the position when the enemy is behind the camera.
        if (screenPoint.z < 0)
        {
            screenPoint.x = Screen.width - screenPoint.x;
            screenPoint.y = Screen.height - screenPoint.y;
        }

        // Clamp the arrow to the padded screen bounds.
        Vector2 clampedPosition = ClampArrowToScreen(new Vector2(screenPoint.x, screenPoint.y));

        // Convert screen coordinates to Canvas coordinates.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            clampedPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out Vector2 localPoint
        );

        return localPoint;
    }

    private Vector2 ClampArrowToScreen(Vector2 screenPosition)
    {
        float minX = arrowPadding;
        float maxX = Screen.width - arrowPadding;
        float minY = arrowPadding;
        float maxY = Screen.height - arrowPadding;

        screenPosition.x = Mathf.Clamp(screenPosition.x, minX, maxX);
        screenPosition.y = Mathf.Clamp(screenPosition.y, minY, maxY);

        return screenPosition;
    }

    private void RotateArrowTowardsEnemy(Transform arrowTransform, Vector3 enemyWorldPosition)
    {
        Vector2 direction = (Vector2)(enemyWorldPosition - mainCamera.transform.position);
        float angle = Vector2.SignedAngle(Vector2.up, direction);
        arrowTransform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void HideAllArrows()
    {
        foreach (GameObject arrow in activeArrows)
        {
            arrow.SetActive(false);
        }
    }
}