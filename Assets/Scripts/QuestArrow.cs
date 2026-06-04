using UnityEngine;

public class QuestArrow : MonoBehaviour
{
    // Робимо синглтон, щоб будь-яка кнопка в меню могла сказати стрілці: "Ану покажи сюди"
    public static QuestArrow Instance;

    public string CurrentOverrideTargetID => overrideTargetID;
    public float rotationSpeed = 10f;
    [Tooltip("Корекція кута: спробуй 0, 90, -90 або 180, якщо стрілка дивиться боком")]
    public float angleOffset = -90f;
    [Tooltip("Відстань до цілі, при якій ручне відстеження вимикається")]
    public float arriveDistance = 2f;

    private SpriteRenderer spriteRenderer;
    private Transform targetTransform;

    // Якщо тут є ID, стрілка веде до нього, ігноруючи поточний квест
    private string overrideTargetID = null;

    void Awake()
    {
        // Надійний синглтон
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 1. Фікс масштабу: ігноруємо Flip гравця (твоя робоча логіка)
        if (transform.parent != null)
        {
            Vector3 parentScale = transform.parent.localScale;
            transform.localScale = new Vector3(Mathf.Sign(parentScale.x), 1, 1);
        }

        // 2. ЛОГІКА ВИБОРУ ЦІЛІ (ІЄРАРХІЯ ПРІОРИТЕТІВ)

        // =========================================================================
        // ПРІОРИТЕТ 1: Ручне відстеження сюжетного квесту
        // =========================================================================
        if (QuestManager.Instance != null && !string.IsNullOrEmpty(overrideTargetID))
        {
            targetTransform = QuestManager.Instance.GetTargetTransform(overrideTargetID);

            if (targetTransform != null)
            {
                spriteRenderer.enabled = true;
                RotateTowardsTarget();

                // Перевіряємо, чи гравець уже дійшов до сюжетного NPC
                if (Vector2.Distance(transform.position, targetTransform.position) <= arriveDistance)
                {
                    Debug.Log("<color=yellow>[Квест]</color> Прийшли до цілі ручного сюжетного відстеження.");
                    overrideTargetID = null;
                }
            }
            else
            {
                spriteRenderer.enabled = false;
            }
            return; // Виходимо, сюжетний ручний режим має найвищий пріоритет
        }

        // =========================================================================
        // ПРІОРИТЕТ 2: Ручне відстеження ЩОДЕННОГО квесту
        // =========================================================================
        if (DailyQuestManager.Instance != null && DailyQuestManager.Instance.trackedDailyIndex != -1)
        {
            // Запитуємо у менеджера дейліків трансформ зареєстрованої точки DailyQuestPoint
            targetTransform = DailyQuestManager.Instance.GetTrackedTargetTransform();

            if (targetTransform != null)
            {
                spriteRenderer.enabled = true;
                RotateTowardsTarget();

                // ФІКС: Додано перевірку прибуття до цілі дейліка
                if (Vector2.Distance(transform.position, targetTransform.position) <= arriveDistance)
                {
                    Debug.Log("<color=orange>[Дейлік]</color> Дійшли до цілі щоденного квесту. Вимикаємо трекінг.");
                    DailyQuestManager.Instance.SetTrackedDaily(DailyQuestManager.Instance.trackedDailyIndex); // Скидає на -1
                }
            }
            else
            {
                // Якщо дейлік вибрано, але точки на сцені немає (інша локація) — ховаємо стрілку
                spriteRenderer.enabled = false;
            }
            return; // Виходимо, щоб автоматичний сюжет не перебивав дейлік
        }

        // =========================================================================
        // ПРІОРИТЕТ 3: Твоя стандартна автоматика (поточний активний сюжетний квест)
        // =========================================================================
        if (QuestManager.Instance != null && QuestManager.Instance.currentQuest != null)
        {
            string currentTargetID = QuestManager.Instance.currentQuest.targetID;
            targetTransform = QuestManager.Instance.GetTargetTransform(currentTargetID);

            if (targetTransform != null)
            {
                spriteRenderer.enabled = true;
                RotateTowardsTarget();
            }
            else
            {
                spriteRenderer.enabled = false;
            }
        }
        else
        {
            // Якщо взагалі нічого не активовано — повністю вимикаємо видимість стрілки
            spriteRenderer.enabled = false;
        }
    }

    void RotateTowardsTarget()
    {
        if (targetTransform == null) return;
        Vector3 direction = targetTransform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle + angleOffset);
    }

    // МЕТОД ДЛЯ КНОПКИ: увімкнути стрілку на конкретного сюжетного NPC
    public void TrackNPC(string npcTargetID)
    {
        overrideTargetID = npcTargetID;

        // Якщо ми вмикаємо сюжетний оверрайд, то скидаємо дейліки, щоб не було конфліктів
        if (DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.trackedDailyIndex = -1;
        }

        gameObject.SetActive(true);
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = true;

        Debug.Log($"<color=cyan>[Навігація]</color> Стрілка активована на сюжетний ID: {npcTargetID}");
    }

    public void ClearOverrideTarget()
    {
        overrideTargetID = null;
        Debug.Log("<color=cyan>[Навігація]</color> Ручне відстеження скасовано.");
    }
}