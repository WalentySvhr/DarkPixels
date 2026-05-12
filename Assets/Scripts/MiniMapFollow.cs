using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player;
    public PolygonCollider2D mapBounds; // Сюди перетягнемо твій суцільний MapBounds

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
        newPosition.z = -10; // Твоє налаштування висоти камери

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

        // Рухаємо камеру
        transform.position = newPosition;

        // Тримаємо камеру завжди повернутою вгору (не даємо їй крутитися за гравцем)
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
}