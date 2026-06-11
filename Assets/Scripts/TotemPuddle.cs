using UnityEngine;

public class TotemPuddle : MonoBehaviour
{
    [Header("Налаштування шкоди")]
    [SerializeField] private int damagePerTick = 5;
    [Tooltip("Як часто наноситься шкода (в секундах). Наприклад: 0.5f — двічі на секунду")]
    [SerializeField] private float tickInterval = 0.5f;

    [Header("Час життя калюжі")]
    [SerializeField] private float lifetime = 4f;

    private float nextDamageTime = 0f; // Таймер для наступного тику шкоди

    private void Start()
    {
        // Калюжа автоматично знищиться через вказаний час життя
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Перевіряємо, чи це гравець
        if (collision.CompareTag("Player"))
        {
            // Перевіряємо, чи настав час для наступного тику шкоди
            if (Time.time >= nextDamageTime)
            {
                PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damagePerTick);

                    // Фіксуємо час наступного дозволеного удару
                    nextDamageTime = Time.time + tickInterval;

                    Debug.Log($"<color=purple>[Калюжа] Нанесено шкоду гравцю: {damagePerTick}. Наступний тик через {tickInterval} сек.</color>");
                }
            }
        }
    }
}