using UnityEngine;
using Cinemachine; // Додаємо для роботи з камерою

public class NextFloorDoor : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Якщо в двері зайшов гравець
        if (other.CompareTag("Player"))
        {
            Debug.Log("<color=magenta>Гравець зайшов у двері!</color>");

            // 1. Запам'ятовуємо позицію ПЕРЕД телепортацією
            Vector3 oldPos = other.transform.position;

            // 2. Кажемо менеджеру башти перекинути нас далі
            if (TowerManager.Instance != null)
            {
                TowerManager.Instance.GoToNextFloor();

                // 3. ФІКС КАМЕРИ
                // Оскільки позиція гравця вже змінилася всередині GoToNextFloor,
                // ми можемо знайти нову дельту і оновити камеру.
                CinemachineVirtualCamera vcam = FindFirstObjectByType<CinemachineVirtualCamera>();

                if (vcam != null)
                {
                    // Обчислюємо зміщення: нова позиція мінус стара
                    Vector3 delta = other.transform.position - oldPos;

                    // Повідомляємо Cinemachine про телепортацію
                    vcam.OnTargetObjectWarped(other.transform, delta);
                }
            }
        }
    }
}