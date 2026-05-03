using UnityEngine;

public class QuestArrow : MonoBehaviour
{
    public float rotationSpeed = 10f;
    [Tooltip("Корекція кута: спробуй 0, 90, -90 або 180, якщо стрілка дивиться боком")]
    public float angleOffset = -90f;

    private SpriteRenderer spriteRenderer;
    private Transform targetTransform;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 1. Фікс масштабу: ігноруємо Flip гравця (щоб стрілку не віддзеркалювало)
        Vector3 parentScale = transform.parent.localScale;
        transform.localScale = new Vector3(Mathf.Sign(parentScale.x), 1, 1);

        // 2. Отримуємо ціль через QuestManager (нову систему реєстрації)
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
            spriteRenderer.enabled = false;
        }
    }

    void RotateTowardsTarget()
    {
        Vector3 direction = targetTransform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Використовуємо Quaternion.Euler для стабільності
        // Додаємо angleOffset, щоб ти міг підкоригувати "ніс" стрілки в інспекторі
        transform.rotation = Quaternion.Euler(0, 0, angle + angleOffset);
    }
}