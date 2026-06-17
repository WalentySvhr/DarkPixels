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
    [SerializeField] private float zoomSpeedMouse = 20f;
    [SerializeField] private float zoomSpeedTouch = 0.05f; // Швидкість зуму для тачу

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
        if (TowerManager.Instance != null && TowerManager.Instance.IsPlayerInTower) return;

        // Перевіряємо, чи грають на телефоні (є хоча б один дотик)
        if (Input.touchCount > 0)
        {
            HandleTouchControls();
        }
        else // Якщо тачів немає, працює керування для ПК
        {
            HandleMouseZoom();
            HandleMouseDrag();
        }
    }

    private void LateUpdate()
    {
        if (TowerManager.Instance != null && TowerManager.Instance.IsPlayerInTower) return;

        ClampCamera();
        ApplyPixelSnapping();
    }

    #region КЕРУВАННЯ ДЛЯ ТЕЛЕФОНІВ (TOUCH)

    private void HandleTouchControls()
    {
        // 1. ЗУМ ДВОМА ПАЛЬЦЯМИ (Pinch to Zoom)
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Знаходимо позицію пальців у попередньому кадрі
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Рахуємо відстань між пальцями в цьому та попередньому кадрах
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Різниця показує, зводять пальці чи розводять
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // Змінюємо розмір камери
            mapCamera.orthographicSize = Mathf.Clamp(
                mapCamera.orthographicSize + deltaMagnitudeDiff * zoomSpeedTouch,
                minSize,
                maxSize
            );
        }
        // 2. ПЕРЕТЯГУВАННЯ ОДНИМ ПАЛЬЦЕМ
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                dragOrigin = mapCamera.ScreenToWorldPoint(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector3 currentTouchPos = mapCamera.ScreenToWorldPoint(touch.position);
                Vector3 difference = dragOrigin - currentTouchPos;

                // Зміщуємо камеру з урахуванням руху пальця
                transform.position += difference;

                // Оновлюємо origin, щоб перетягування було плавним і без ривків
                dragOrigin = mapCamera.ScreenToWorldPoint(touch.position);
            }
        }
    }

    #endregion

    #region КЕРУВАННЯ ДЛЯ ПК (MOUSE)

    private void HandleMouseZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            Vector3 mouseBefore = mapCamera.ScreenToWorldPoint(Input.mousePosition);
            mapCamera.orthographicSize = Mathf.Clamp(mapCamera.orthographicSize - scroll * zoomSpeedMouse, minSize, maxSize);
            Vector3 mouseAfter = mapCamera.ScreenToWorldPoint(Input.mousePosition);
            transform.position += (mouseBefore - mouseAfter);
        }
    }

    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
            dragOrigin = mapCamera.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            Vector3 currentMousePos = mapCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 difference = dragOrigin - currentMousePos;
            transform.position += difference;
            dragOrigin = mapCamera.ScreenToWorldPoint(Input.mousePosition); // Виправлено баг ривків при зумі
        }
    }

    #endregion

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