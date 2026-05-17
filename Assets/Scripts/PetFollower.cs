using UnityEngine;

public class PetFollower : MonoBehaviour
{
    [Header("Налаштування руху")]
    public Transform playerTarget;
    public float followSpeed = 4f;
    public float stopDistance = 1.5f;
    public float teleportDistance = 15f;

    [Header("Візуал та Анімації")]
    public float hoverAmplitude = 0.2f;
    public float hoverSpeed = 3f;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    [Header("Налаштування Луту")]
    public string lootTag = "Loot"; // Залишаємо тег тут, бо він загальний для гри

    // --- ДАНІ ПЕТА ---
    private PetData currentData;

    [HideInInspector] public Vector3 movementTarget;
    [HideInInspector] public float currentStopDistance;

    // Зберігаємо стан, щоб уникати мікро-зупинок
    private bool isCurrentlyMoving = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTarget = player.transform;
        }

        if (playerTarget != null) movementTarget = playerTarget.position;
        currentStopDistance = stopDistance;
    }

    // --- ПРИЙМАЄМО ДАНІ ВІД СПАВНЕРА ---
    public void InitializeData(PetData data)
    {
        currentData = data;
    }

    void LateUpdate()
    {
        if (playerTarget == null) return;

        // 1. Ривок повідця
        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
        if (distanceToPlayer > teleportDistance)
        {
            transform.position = playerTarget.position + new Vector3(-1f, 1f, 0f);
            movementTarget = playerTarget.position;
            return;
        }

        // 2. Логіка руху до цілі з БУФЕРНОЮ ЗОНОЮ
        float distanceToTarget = Vector2.Distance(transform.position, movementTarget);

        // Починаємо бігти тільки якщо гравець відійшов ДАЛІ за буфер (stopDistance + 0.2)
        if (distanceToTarget > currentStopDistance + 0.2f)
        {
            isCurrentlyMoving = true;
        }
        // Зупиняємось, коли підійшли впритул до stopDistance
        else if (distanceToTarget <= currentStopDistance)
        {
            isCurrentlyMoving = false;
        }

        // Рух
        if (isCurrentlyMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, movementTarget, followSpeed * Time.deltaTime);
        }

        // Передаємо параметр Speed в Animator
        if (animator != null)
        {
            float currentSpeedRatio = isCurrentlyMoving ? 1f : 0f;
            animator.SetFloat("Speed", currentSpeedRatio);
        }

        // 3. Розворот спрайту
        if (movementTarget.x > transform.position.x) spriteRenderer.flipX = false;
        else if (movementTarget.x < transform.position.x) spriteRenderer.flipX = true;

        // 4. Левітація
        float hoverY = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude * Time.deltaTime;
        transform.position += new Vector3(0, hoverY, 0);

        // 5. Виконання здібності
        ExecuteAbility();
    }

    private void ExecuteAbility()
    {
        if (currentData == null) return;

        switch (currentData.abilityType)
        {
            case PetAbilityType.MagnetLoot:
                PullLoot();
                break;

                // Заготовка на майбутні здібності:
                // case PetAbilityType.HealthRegen:
                //     break;
        }
    }

    private void PullLoot()
    {
        // Беремо радіус та швидкість із файла PetData!
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, currentData.abilityRadius);
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(lootTag))
            {
                col.transform.position = Vector2.MoveTowards(col.transform.position, playerTarget.position, currentData.abilityPower * Time.deltaTime);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        float radius = (currentData != null) ? currentData.abilityRadius : 5f; // Відображаємо радіус з даних
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}