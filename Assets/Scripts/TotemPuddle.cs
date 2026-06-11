using UnityEngine;

public class TotemPuddle : MonoBehaviour
{
    [Header("Налаштування шкоди")]
    [SerializeField] private int damagePerTick = 5;
    [Tooltip("Як часто наноситься шкода (в секундах). Наприклад: 0.5f — двічі на секунду")]
    [SerializeField] private float tickInterval = 0.5f;

    [Header("Час життя калюжі")]
    [SerializeField] private float lifetime = 4f;

    private PlayerHealth playerHealthInZone; // Посилання на гравця, якщо він всередині
    private float tickTimer = 0f;            // Власний таймер для тиків

    private void Start()
    {
        // Калюжа автоматично знищиться через вказаний час життя
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Якщо гравець стоїть у калюжі — таймер рахує час незалежно від того, рухається він чи ні
        if (playerHealthInZone != null)
        {
            tickTimer -= Time.deltaTime;

            if (tickTimer <= 0f)
            {
                playerHealthInZone.TakeDamage(damagePerTick);

                // Скидаємо таймер на заданий інтервал
                tickTimer = tickInterval;

                Debug.Log($"<color=purple>[Калюжа] Нанесено шкоду гравцю: {damagePerTick}. Наступний тик через {tickInterval} сек.</color>");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealthInZone = playerHealth;

                // ЕФЕКТ НЕСПОДІВАНКИ: наносимо перший тик шкоди ОДРАЗУ при наступанні в калюжу
                playerHealthInZone.TakeDamage(damagePerTick);

                // Запускаємо таймер для НАСТУПНОГО тику
                tickTimer = tickInterval;

                Debug.Log("<color=green>[Калюжа] Гравець наступив в зону! Перший урон нанесено одразу.</color>");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Перевіряємо, чи вийшов саме той гравець, якого ми трекаємо
            if (collision.GetComponent<PlayerHealth>() == playerHealthInZone)
            {
                playerHealthInZone = null;
                Debug.Log("<color=yellow>[Калюжа] Гравець вийшов із зони ураження.</color>");
            }
        }
    }
}