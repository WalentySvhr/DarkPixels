using UnityEngine;

public class RandomObjectTrigger : MonoBehaviour
{
    [Header("Об'єкт для активації")]
    [SerializeField] private GameObject targetObject;

    [Header("Режим 100% Гарантії")]
    [Tooltip("Якщо увімкнено, шанс стає 100%, об'єкт активується відразу, а стан записується в SaveManager")]
    [SerializeField] private bool isGuaranteedOneTime = false;

    [Tooltip("Якщо УВІМКНЕНО — об'єкт збережеться в SaveManager відразу при наступанні (ідеально для скринь та схованок).")]
    [SerializeField] private bool saveImmediately = true;

    [Header("Унікальний ID для збереження")]
    [Tooltip("Обов'язково вкажи унікальне ім'я (наприклад: 'open_chest_forest_1'), якщо увімкнено режим гарантії")]
    [SerializeField] private string uniqueID;

    [Header("Налаштування рандому (Якщо вимкнено гарантію)")]
    [Range(0f, 100f)]
    [SerializeField] private float spawnChance = 50f;

    [Header("Захист від спаму (Кулдаун)")]
    [SerializeField] private float rollCooldown = 5f;
    private float nextAllowedRollTime = 0f;

    [Header("Поведінка (Якщо вимкнено гарантію)")]
    [SerializeField] private bool deactivateOnExit = false;
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered = false;

    private void Start()
    {
        // --- ПЕРЕВІРКА ЗБЕРЕЖЕННЯ ПРИ СТАРТІ ---
        if (isGuaranteedOneTime && !string.IsNullOrEmpty(uniqueID))
        {
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentData != null)
            {
                if (SaveManager.Instance.CurrentData.unlockedTrueObjects.Contains(uniqueID))
                {
                    if (targetObject != null)
                    {
                        targetObject.SetActive(true);
                    }
                    Destroy(gameObject);
                    return;
                }
            }
        }

        if (targetObject != null) targetObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (targetObject == null) return;

        // --- РЕЖИМ ГАРАНТІЇ ТА ЗБЕРЕЖЕННЯ ---
        if (isGuaranteedOneTime)
        {
            targetObject.SetActive(true);

            // Зберігаємо МИТТЄВО, тільки якщо увімкнено saveImmediately
            if (saveImmediately && !string.IsNullOrEmpty(uniqueID) && SaveManager.Instance != null && SaveManager.Instance.CurrentData != null)
            {
                if (!SaveManager.Instance.CurrentData.unlockedTrueObjects.Contains(uniqueID))
                {
                    SaveManager.Instance.CurrentData.unlockedTrueObjects.Add(uniqueID);
                    SaveManager.Instance.SaveGame();
                    Debug.Log($"<color=gold>[SaveManager] Об'єкт '{uniqueID}' збережено миттєво!</color>");
                }
            }

            Destroy(gameObject); // Знищуємо сам тригер, він свою роботу зробив
            return;
        }

        // Логіка звичайного рандому
        if (triggerOnlyOnce && hasTriggered) return;
        if (Time.time < nextAllowedRollTime) return;
        if (targetObject.activeSelf) return;

        nextAllowedRollTime = Time.time + rollCooldown;
        hasTriggered = true;

        float roll = Random.Range(0f, 100f);
        if (roll <= spawnChance)
        {
            targetObject.SetActive(true);
            Debug.Log($"<color=green>[Тригер] Успіх! Об'єкт активовано.</color>");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (deactivateOnExit && targetObject != null)
        {
            targetObject.SetActive(false);
            if (!triggerOnlyOnce) hasTriggered = false;
        }
    }
}