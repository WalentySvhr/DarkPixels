using UnityEngine;

public class QuestArrow : MonoBehaviour
{
    // Робимо синглтон, щоб будь-яка кнопка в меню могла сказати стрілці: "Ану покажи сюди"
    public static QuestArrow Instance;
    // Додай це в скрипт QuestArrow.cs, щоб кнопки могли бачити поточну ціль
    public string CurrentOverrideTargetID => overrideTargetID;
    public float rotationSpeed = 10f;
    [Tooltip("Корекція кута: спробуй 0, 90, -90 або 180, якщо стрілка дивиться боком")]
    public float angleOffset = -90f;
    [Tooltip("Відстань до NPC, при якій ручне відстеження вимикається")]
    public float arriveDistance = 2f;

    private SpriteRenderer spriteRenderer;
    private Transform targetTransform;

    // Якщо тут є ID, стрілка веде до нього, ігноруючи поточний квест
    private string overrideTargetID = null;

    void Awake()
    {
        Instance = this;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 1. Фікс масштабу: ігноруємо Flip гравця
        if (transform.parent != null)
        {
            Vector3 parentScale = transform.parent.localScale;
            transform.localScale = new Vector3(Mathf.Sign(parentScale.x), 1, 1);
        }

        // 2. Логіка вибору цілі
        if (QuestManager.Instance != null)
        {
            // ПРІОРИТЕТ 1: Якщо гравець сам вибрав квест для відстеження з меню
            if (!string.IsNullOrEmpty(overrideTargetID))
            {
                targetTransform = QuestManager.Instance.GetTargetTransform(overrideTargetID);

                if (targetTransform != null)
                {
                    spriteRenderer.enabled = true;
                    RotateTowardsTarget();

                    // Перевіряємо, чи гравець уже дійшов до NPC
                    if (Vector2.Distance(transform.position, targetTransform.position) <= arriveDistance)
                    {
                        Debug.Log("<color=yellow>[Квест]</color> Прийшли до цілі ручного відстеження. Повертаємось до автомата.");
                        overrideTargetID = null; // Скидаємо ручний режим, коли підійшли впритул!
                    }
                }
                else
                {
                    spriteRenderer.enabled = false;
                }
                return; // Виходимо з Update, щоб автоматика не перебивала ручний режим
            }

            // ПРІОРИТЕТ 2: Твоя стандартна автоматика (поточний активний квест)
            if (QuestManager.Instance.currentQuest != null)
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
                spriteRenderer.enabled = false;
            }
        }
        else
        {
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

    // МЕТОД ДЛЯ КНОПКИ: увімкнути стрілку на конкретного NPC
    public void TrackNPC(string npcTargetID)
    {
        overrideTargetID = npcTargetID;

        // === ОСЬ ВІН, НАЙВАЖЛИВІШИЙ ФІКС: ===
        // Змушуємо об'єкт стрілки прокинутися в ієрархії, навіть якщо QuestManager його вимкнув!
        gameObject.SetActive(true);

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = true;

        Debug.Log($"<color=cyan>[Навігація]</color> Стрілка ПРИМУСОВО активована на пошук ID: {npcTargetID}");
    }
    // Додай цей метод в кінець скрипта QuestArrow.cs
    public void ClearOverrideTarget()
    {
        overrideTargetID = null;
        Debug.Log("<color=cyan>[Навігація]</color> Ручне відстеження СКАСОВАНО гравцем.");
    }
}