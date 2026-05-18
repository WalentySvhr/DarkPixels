using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player;
    public PolygonCollider2D mapBounds; // Сюди перетягнемо суцільний MapBounds

    [Header("Налаштування згладжування швів")]
    [Tooltip("Увімкнути прив'язку камери до пікселів для уникнення блимання швів")]
    public bool enablePixelSnapping = true;
    [Tooltip("Має збігатися з налаштуванням Pixels Per Unit ваших спрайтів (зазвичай 100, 32 або 16)")]
    public float pixelsPerUnit = 100f;

    private Camera cam;
    private float camHalfHeight;
    private float camHalfWidth;

    void Start()
    {
        cam = GetComponent<Camera>();

        // Вираховуємо реальний розмір камери.
        // Це потрібно, щоб камера впиралася в край карти своїм краєм, а не центром.
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = cam.orthographicSize * cam.aspect;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Початково беремо позицію гравця
        Vector3 newPosition = player.position;
        // На вашому попередньому скріншоті камера була на -25, 
        // але залишаю -10, як у вашому коді. Головне, щоб вона бачила тайли.
        newPosition.z = -10;

        // Якщо ми призначили межі карти в Інспекторі
        if (mapBounds != null)
        {
            Bounds bounds = mapBounds.bounds;

            // Вираховуємо допустимі межі, віднімаючи розмір самої камери
            float minX = bounds.min.x + camHalfWidth;
            float maxX = bounds.max.x - camHalfWidth;
            float minY = bounds.min.y + camHalfHeight;
            float maxY = bounds.max.y - camHalfHeight;

            // Вжимаємо координати по X та Y
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        }

        // --- НОВИЙ КОД: Округлення координат (Pixel Snapping) ---
        if (enablePixelSnapping)
        {
            newPosition.x = Mathf.Round(newPosition.x * pixelsPerUnit) / pixelsPerUnit;
            newPosition.y = Mathf.Round(newPosition.y * pixelsPerUnit) / pixelsPerUnit;
        }

        // Рухаємо камеру
        transform.position = newPosition;

        // Тримаємо камеру завжди повернутою вгору (не даємо їй крутитися за гравцем)
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
}