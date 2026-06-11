using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    private bool isDead = false;

    [Header("Animation & Visuals")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    [Tooltip("Колір, в який фарбується моб при отриманні шкоди")]
    public Color flashColor = Color.red;
    [Tooltip("Тривалість блимання кольором")]
    public float flashDuration = 0.15f;
    [Tooltip("Скільки секунд чекати перед видаленням об'єкта, щоб програлася анімація смерті")]
    public float deathAnimationDuration = 1f;

    private Color originalColor;
    private Coroutine flashCoroutine;

    [Header("UI References")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;
    public PolygonAreaSpawner mySpawner;
    public TowerSpawner towerSpawner;

    [Header("Effects")]
    public GameObject damagePopupPrefab;

    [Header("Spawn Context")]
    private LootDropper lootDropper;

    [Header("Quest & Enemy Settings")]
    [Tooltip("Унікальний ID моба для квестів (наприклад: Skeleton, Goblin). Має збігатися з targetID у квесті.")]
    public string enemyID;
    [Tooltip("Список Target ID квестів, які можуть випасти саме з цього моба")]
    public List<string> allowedQuestItemIDs = new List<string>();

    [Header("Daily Quest Settings")]
    [Tooltip("Поставте галочку, якщо цей моб - елітний/міні-бос")]
    public bool isElite = false;
    [Header("Налаштування типу об'єкта")]
    [Tooltip("Увімкни це ТІЛЬКИ на префабі тотема чи бочки, щоб вони не рахувалися як живі вороги в квестах та рекордах")]
    [SerializeField] private bool isStructure = false;

    // === Фізика ===
    private Rigidbody2D rb;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Отримуємо Rigidbody2D
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (TowerManager.Instance != null)
        {
            float multiplier = TowerManager.Instance.GetDifficultyMultiplier();
            maxHealth = Mathf.RoundToInt(maxHealth * multiplier);
        }

        currentHealth = maxHealth;
        lootDropper = GetComponent<LootDropper>();

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = maxHealth;
        }

        if (LevelManager.Instance != null) LevelManager.Instance.RegisterEnemy();

        UpdateHealthUI();
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection, float force, bool isCrit = false)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        // ЗАСТОСОВУЄМО ВІДКИДАННЯ
        if (rb != null && force > 0)
        {
            rb.linearVelocity = Vector2.zero; // Скидаємо швидкість, щоб відкидання завжди працювало стабільно
            rb.AddForce(knockbackDirection * force, ForceMode2D.Impulse);
        }

        if (animator != null) animator.SetTrigger("TakeDamage");

        if (spriteRenderer != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.OnTakeDamage();
        }



        SpawnDamagePopup(damage, isCrit);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    void SpawnDamagePopup(int damageAmount, bool isCrit)
    {
        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up, Quaternion.identity);
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();

            if (popupScript != null) popupScript.Setup(damageAmount, isCrit);
        }
    }

    void UpdateHealthUI()
    {
        if (hpSlider != null) hpSlider.value = currentHealth;
        if (hpText != null) hpText.text = $"{currentHealth} / {maxHealth}";
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();

        // Зараховуємо вбивство в рекорд Башні, тільки якщо це ЖИВИЙ ВОРОГ, а не пастка
        if (!isStructure && TowerManager.Instance != null)
            TowerManager.Instance.AddKill();

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (animator != null) animator.SetTrigger("Die");

        Collider2D mobCollider = GetComponent<Collider2D>();
        if (mobCollider != null) mobCollider.enabled = false;

        // Якщо у тотема немає скрипта EnemyAI, Unity просто пропустить цей блок без помилок
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.StopAllCoroutines();
            ai.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }

        if (hpSlider != null) hpSlider.gameObject.SetActive(false);
        if (hpText != null) hpText.gameObject.SetActive(false);

        // Звільнення лічильників кімнати робимо ТІЛЬКИ для монстрів.
        // Якщо це тотем-пастка, ми не зменшуємо кількість ворогів, необхідних для зачистки поверху.
        if (!isStructure)
        {
            if (mySpawner != null) mySpawner.EnemyDied();
            if (towerSpawner != null) towerSpawner.EnemyDied(gameObject);
            if (LevelManager.Instance != null) LevelManager.Instance.UnregisterEnemy();
        }

        CheckForUniqueQuestDrop();

        // Лут (монетки/хілки) тотем все одно скине, якщо на ньому висить скрипт lootDropper
        if (lootDropper != null) lootDropper.DropLoot();

        // === ОНОВЛЕНА ЛОГІКА КВЕСТІВ (Тільки для монстрів) ===
        if (!isStructure && QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAction(QuestType.KillInTower, enemyID);
            QuestManager.Instance.OnQuestAction(QuestType.KillSpecific, enemyID);
        }

        // Щоденні квести (не зараховуємо руйнування тотема як вбивство істоти)
        if (!isStructure && DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.AddProgress(DailyQuestType.KillEnemies, 1);
            if (isElite)
            {
                DailyQuestManager.Instance.AddProgress(DailyQuestType.KillElite, 1);
            }
        }

        Destroy(gameObject, deathAnimationDuration);
    }

    void CheckForUniqueQuestDrop()
    {
        if (QuestManager.Instance == null || QuestManager.Instance.currentQuest == null) return;

        string questTargetID = QuestManager.Instance.currentQuest.targetID;

        if (!allowedQuestItemIDs.Contains(questTargetID)) return;

        if (!string.IsNullOrEmpty(questTargetID) && QuestManager.Instance.TryDropQuestItem(questTargetID))
        {
            Item itemToDrop = QuestManager.Instance.currentQuest.itemToCollect;

            if (itemToDrop != null)
            {
                QuestManager.Instance.DropItemOnGround(itemToDrop);
                Debug.Log($"<color=magenta>Унікальний квестовий предмет ({itemToDrop.itemName}) випав!</color>");
            }
        }
    }
}