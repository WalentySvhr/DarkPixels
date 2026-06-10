using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MapZoom : MonoBehaviour
{
    private Camera mapCamera;
    private Vector3 dragOrigin;

    [Header("Налаштування")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float minSize = 5f;
    [SerializeField] private float maxSize = 50f;
    [SerializeField] private float zoomSpeed = 20f;

    [Header("Піксельна сітка")]
    [Tooltip("Кількість пікселів на юніт у твоїх спрайтах (наприклад, 16 або 32)")]
    [SerializeField] private float pixelsPerUnit = 16f;
    [Tooltip("Увімкнути прив'язку камери до піксельної сітки")]
    [SerializeField] private bool enablePixelSnapping = true;

    [Header("Межі")]
    [SerializeField] private float minX, maxX, minY, maxY;

    private void Awake() => mapCamera = GetComponent<Camera>();

    private void Start() => CenterOnPlayer();

    private void Update()
    {
        // ПЕРЕВІРКА: Якщо ми в башті, виходимо з Update і нічого не робимо
        if (TowerManager.Instance != null && TowerManager.Instance.IsPlayerInTower)
        {
            return;
        }

        HandleZoom();
        HandleDrag();
    }

    // Камери завжди мають позиціонуватися в LateUpdate, щоб уникнути тремтіння (jittering)
    private void LateUpdate()
    {
        if (TowerManager.Instance != null && TowerManager.Instance.IsPlayerInTower) return;

        ClampCamera();
        ApplyPixelSnapping();
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            Vector3 mouseBefore = mapCamera.ScreenToWorldPoint(Input.mousePosition);
            mapCamera.orthographicSize = Mathf.Clamp(mapCamera.orthographicSize - scroll * zoomSpeed, minSize, maxSize);
            Vector3 mouseAfter = mapCamera.ScreenToWorldPoint(Input.mousePosition);
            transform.position += (mouseBefore - mouseAfter);
        }
    }

    private void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
            dragOrigin = mapCamera.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            Vector3 difference = dragOrigin - mapCamera.ScreenToWorldPoint(Input.mousePosition);
            transform.position += difference;
        }
    }

    private void ClampCamera()
    {
        float vertExtent = mapCamera.orthographicSize;
        float horzExtent = vertExtent * mapCamera.aspect;

        float minXClamped = minX + horzExtent;
        float maxXClamped = maxX - horzExtent;
        float minYClamped = minY + vertExtent;
        float maxYClamped = maxY - vertExtent;

        float x = (maxXClamped < minXClamped) ? (minX + maxX) / 2f : Mathf.Clamp(transform.position.x, minXClamped, maxXClamped);
        float y = (maxYClamped < minYClamped) ? (minY + maxY) / 2f : Mathf.Clamp(transform.position.y, minYClamped, maxYClamped);

        transform.position = new Vector3(x, y, transform.position.z);
    }

    private void ApplyPixelSnapping()
    {
        if (!enablePixelSnapping || pixelsPerUnit <= 0) return;

        // Округляємо фінальну позицію камери чітко до меж пікселів текстури
        Vector3 snappedPosition = transform.position;
        snappedPosition.x = Mathf.Round(snappedPosition.x * pixelsPerUnit) / pixelsPerUnit;
        snappedPosition.y = Mathf.Round(snappedPosition.y * pixelsPerUnit) / pixelsPerUnit;

        transform.position = snappedPosition;
    }

    public void CenterOnPlayer()
    {
        if (playerTransform == null) playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform != null) transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, transform.position.z);
    }
}