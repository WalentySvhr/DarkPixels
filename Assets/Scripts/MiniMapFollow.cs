using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        // Рухаємо камеру за гравцем
        Vector3 newPosition = player.position;
        newPosition.z = -10; // Камера має бути над сценою
        transform.position = newPosition;

        // Тримаємо камеру завжди повернутою вгору (не даємо їй крутитися за гравцем)
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
}
