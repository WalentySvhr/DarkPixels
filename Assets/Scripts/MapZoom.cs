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
        ClampCamera();
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

    public void CenterOnPlayer()
    {
        if (playerTransform == null) playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform != null) transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, transform.position.z);
    }
}