using UnityEngine;

public class LocalTeleport : MonoBehaviour
{
    [Header("Налаштування телепорту")]
    public Transform targetLocation; // Точка виходу
    public bool isActive = false;    // Чи можна зайти (відкриті двері)
    public string locationName = "Відкритий світ";

    [Header("Налаштування Башти")]
    [Tooltip("Ставимо для входу в башту з головного світу")]
    public bool isEntranceToTower = false;
    [Tooltip("Ставимо для дверей, що ведуть з башти назад у світ")]
    public bool resetTowerOnExit = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Перевіряємо тег гравця та чи двері активовані
        if (isActive && collision.CompareTag("Player"))
        {
            // 1. Скидаємо швидкість, щоб гравця не "винесло" після ТП
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // 2. Переміщуємо гравця
            if (targetLocation != null)
            {
                collision.transform.position = targetLocation.position;
            }

            // 3. ВИКЛИК ТЕКСТУ (Універсально для префабів через Singleton)
            if (LocationAnnouncer.Instance != null)
            {
                Debug.Log($"Надсилаю сигнал анонсеру: {locationName}");
                LocationAnnouncer.Instance.ShowLocation(locationName);
            }
            else
            {
                Debug.LogError("КРИТИЧНА ПОМИЛКА: LocationAnnouncer.Instance не знайдено на сцені!");
            }

            // 4. Керування станом башти
            if (TowerManager.Instance != null)
            {
                if (isEntranceToTower)
                {
                    TowerManager.Instance.StartTowerRun();
                }
                else if (resetTowerOnExit)
                {
                    TowerManager.Instance.ResetTowerProgress();
                }
            }

            Debug.Log($"Гравець переміщений у: {locationName}");
        }
    }

    // Метод для активації дверей (наприклад, після вбивства боса)
    public void OpenDoor()
    {
        isActive = true;

        // Візуальна зміна (якщо є спрайт)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.white;
    }
}