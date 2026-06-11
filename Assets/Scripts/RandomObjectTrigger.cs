using UnityEngine;

public class RandomObjectTrigger : MonoBehaviour
{
    [Header("Об'єкт для активації")]
    [SerializeField] private GameObject targetObject;

    [Header("Режим 100% Гарантії")]
    [Tooltip("Якщо увімкнено, шанс стає 100%, об'єкт активується відразу, а стан записується в SaveManager")]
    [SerializeField] private bool isGuaranteedOneTime = false;

    [Header("Унікальний ID для збереження")]
    [Tooltip("Обов'язково вкажи унікальне ім'я (наприклад: 'open_chest_forest_1'), якщо увімкнено режим гарантії")]
    [SerializeField] private string uniqueID;

    [Header("Налаштування рандому (Якщо вимкнено гарантію)")]
    [Range(0f, 100f)]
    [SerializeField] private float spawnChance = 50f;

    [Header("Захист від спаму (Кулдаун)")]
    [Tooltip("Пауза в секундах перед наступним ролом шансу, якщо об'єкт НЕ заспавнився")]
    [SerializeField] private float rollCooldown = 5f;
    private float nextAllowedRollTime = 0f; // Час, коли можна буде кинути кубик наступного разу

    [Header("Поведінка (Якщо вимкнено гарантію)")]
    [Tooltip("Якщо увімкнено, об'єкт зникне, коли гравець вийде з зони (Для пасток)")]
    [SerializeField] private bool deactivateOnExit = false;

    [Tooltip("Якщо увімкнено, шанс прорахується лише ОДИН раз за рівень (Для скринь)")]
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered = false;

    private void Start()
    {
        // --- ПЕРЕВІРКА ЗБЕРЕЖЕННЯ ПРИ СТАРТІ ---
        if (isGuaranteedOneTime && !string.IsNullOrEmpty(uniqueID))
        {
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentData != null)
            {
                // Якщо в нашому списку збережень уже є цей ID
                if (SaveManager.Instance.CurrentData.unlockedTrueObjects.Contains(uniqueID))
                {
                    if (targetObject != null) targetObject.SetActive(true); // Залишаємо об'єкт увімкненим
                    Destroy(gameObject); // Видаляємо сам тригер, він більше не потрібен
                    return;
                }
            }
        }

        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (targetObject == null) return;

        // --- РЕЖИМ ГАРАНТІЇ ТА ЗБЕРЕЖЕННЯ ---
        if (isGuaranteedOneTime)
        {
            targetObject.SetActive(true);

            // Інтеграція з твоїм SaveManager
            if (!string.IsNullOrEmpty(uniqueID) && SaveManager.Instance != null && SaveManager.Instance.CurrentData != null)
            {
                // Якщо об'єкт ще не записаний у збереження, додаємо його
                if (!SaveManager.Instance.CurrentData.unlockedTrueObjects.Contains(uniqueID))
                {
                    SaveManager.Instance.CurrentData.unlockedTrueObjects.Add(uniqueID);
                    SaveManager.Instance.SaveGame(); // Записуємо файл на диск смартфона
                    Debug.Log($"<color=gold>[SaveManager] Об'єкт '{uniqueID}' активовано та збережено на диск!</color>");
                }
            }

            Destroy(gameObject); // Знищуємо сам тригер
            return; // Виходимо, щоб не йшов код рандому нижче
        }

        // 1. Перевірка на "одноразовий тригер" (для звичайного рандому)
        if (triggerOnlyOnce && hasTriggered) return;

        // 2. ЗАХИСТ ВІД СПАМУ: Перевіряємо, чи минув час кулдауну
        if (Time.time < nextAllowedRollTime)
        {
            long secondsLeft = Mathf.CeilToInt(nextAllowedRollTime - Time.time);
            Debug.Log($"<color=orange>[Тригер] Захист від спаму! Спробуй через {secondsLeft} сек.</color>");
            return;
        }

        // Якщо об'єкт ВЖЕ активний, повторно кубик не кидаємо
        if (targetObject.activeSelf) return;

        // Оновлюємо час наступного дозволеного ролу (фіксуємо кулдаун)
        nextAllowedRollTime = Time.time + rollCooldown;
        hasTriggered = true;

        // Кидаємо кубик
        float roll = Random.Range(0f, 100f);

        if (roll <= spawnChance)
        {
            targetObject.SetActive(true);
            Debug.Log($"<color=green>[Тригер] Успіх! Об'єкт активовано.</color>");
        }
        else
        {
            Debug.Log($"<color=red>[Тригер] Невдача ({roll:F1}%). Наступний рол заблоковано на {rollCooldown} сек.</color>");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (deactivateOnExit && targetObject != null)
        {
            targetObject.SetActive(false);

            if (!triggerOnlyOnce)
            {
                hasTriggered = false;
            }
        }
    }
}