using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MapZoom : MonoBehaviour
{
    private Camera mapCamera;
    private Vector3 touchStartPos;

    [Header("Посилання на гравця")]
    [Tooltip("Перетягни сюди об'єкт твого гравця з Ієрархії")]
    [SerializeField] private Transform playerTransform;

    [Header("Налаштування масштабу (Size)")]
    [SerializeField] private float defaultSize = 35f;
    [SerializeField] private float minSize = 20f;
    [SerializeField] private float maxSize = 50f;

    [Header("Швидкість")]
    [SerializeField] private float zoomSpeed = 0.05f;
    [SerializeField] private float dragSpeed = 0.5f;
    [Tooltip("Швидкість зуму кнопками на ПК клавіатурі")]
    [SerializeField] private float keyboardZoomSpeed = 30f; // Збільшив швидкість для помітнішого тесту

    [Header("Жорсткі межі карти (Координати острова на сцені)")]
    [Tooltip("Лівий край острова (мінімальний X)")]
    [SerializeField] private float minX = 10f;
    [Tooltip("Правий край острова (максимальний X)")]
    [SerializeField] private float maxX = 70f;
    [Tooltip("Нижній край острова (мінімальний Y)")]
    [SerializeField] private float minY = -45f;
    [Tooltip("Верхній край острова (максимальний Y)")]
    [SerializeField] private float maxY = 0f;

    private void Awake()
    {
        mapCamera = GetComponent<Camera>();
        mapCamera.orthographicSize = defaultSize;
    }

    private void OnEnable()
    {
        CenterOnPlayer();
    }

    public void CenterOnPlayer()
    {
        if (playerTransform != null)
        {
            Vector3 targetPosition = new Vector3(playerTransform.position.x, playerTransform.position.y, transform.position.z);
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
            transform.position = targetPosition;
        }
    }

    private void Update()
    {
        // --- 1. ТЕСТ КЛАВІАТУРИ (ПЕРЕНЕСЕНО НА САМИЙ ВЕРХ ДЛЯ НАДІЙНОСТІ) ---
        if (Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.W))
        {
            Debug.Log("Натиснуто кнопку НАБЛИЖЕННЯ (+) або W. Поточний розмір камери: " + mapCamera.orthographicSize);
            ExecuteZoom(-keyboardZoomSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.S))
        {
            Debug.Log("Натиснуто кнопку ВІДДАЛЕННЯ (-) або S. Поточний розмір камери: " + mapCamera.orthographicSize);
            ExecuteZoom(keyboardZoomSpeed * Time.deltaTime);
        }

        // --- 2. ЛОГІКА ЗУМУ ДЛЯ МОБІЛКИ ---
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            ExecuteZoom(deltaMagnitudeDiff * zoomSpeed);
            return; // Тут ретурн безпечний, бо клавіатура вже перевірилась вище
        }

        // --- 3. ПЕРЕТЯГУВАННЯ МИШКОЮ ---
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = mapCamera.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 direction = touchStartPos - mapCamera.ScreenToWorldPoint(Input.mousePosition);
            transform.position += direction * dragSpeed;
        }

        // --- 4. ПЕРЕТЯГУВАННЯ ПАЛЬЦЕМ ---
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 touchDelta = touch.deltaPosition;
                transform.position -= new Vector3(touchDelta.x, touchDelta.y, 0) * (mapCamera.orthographicSize / 1000f) * dragSpeed;
            }
        }

        // --- 5. ОБМЕЖЕННЯ КАМЕРИ ---
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        transform.position = clampedPosition;
    }

    private void ExecuteZoom(float increment)
    {
        mapCamera.orthographicSize = Mathf.Clamp(mapCamera.orthographicSize + increment, minSize, maxSize);
    }
}