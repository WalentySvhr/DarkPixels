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
    [Tooltip("Скільки секунд чекать перед видаленням об'єкта, щоб програлася анімація смерті")]
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
    public string enemyID;
    public List<string> allowedQuestItemIDs = new List<string>();

    [Header("Daily Quest Settings")]
    public bool isElite = false;
    [Header("Налаштування типу об'єкта")]
    [SerializeField] private bool isStructure = false;

    // === КЕШУВАННЯ КОМПОНЕНТІВ ДЛЯ ОПТИМІЗАЦІЇ ===
    private Rigidbody2D rb;
    private EnemyAI ai;
    private Collider2D mobCollider;
    private WaitForSeconds flashWait;
    private bool hasLootDropper;
    private bool hasHpSlider;
    private bool hasHpText;

    void Start()
    {
        // Кешуємо ВСЕ на старті, щоб не викликати GetComponent в бою
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        ai = GetComponent<EnemyAI>();
        mobCollider = GetComponent<Collider2D>();
        lootDropper = GetComponent<LootDropper>();

        // Швидкі перевірки на null (булеві прапори працюють швидше, ніж постійний check на null в Update)
        hasLootDropper = lootDropper != null;
        hasHpSlider = hpSlider != null;
        hasHpText = hpText != null;

        // Кешуємо WaitForSeconds для корутини блимання, щоб не плодити сміття
        flashWait = new WaitForSeconds(flashDuration);

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

        if (hasHpSlider)
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

        // Застосовуємо відкидання через закешований Rigidbody
        if (rb != null && force > 0)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockbackDirection * force, ForceMode2D.Impulse);
        }

        if (animator != null) animator.SetTrigger("TakeDamage");

        if (spriteRenderer != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        // Викликаємо реакцію ШІ без GetComponent
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
        yield return flashWait; // Оптимізовано: нуль сміття в пам'яті
        spriteRenderer.color = originalColor;
    }

    void SpawnDamagePopup(int damageAmount, bool isCrit)
    {
        if (damagePopupPrefab != null)
        {
            // У МАЙБУТНЬОМУ тут вкрай важливо замінити Instantiate на пул:
            // GameObject popup = ObjectPool.Instance.SpawnFromPool("DamagePopup", transform.position + Vector3.up, Quaternion.identity);

            GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up, Quaternion.identity);
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();

            if (popupScript != null) popupScript.Setup(damageAmount, isCrit);
        }
    }

    void UpdateHealthUI()
    {
        if (hasHpSlider) hpSlider.value = currentHealth;

        // Оптимізація виведення тексту: робимо це тільки якщо ХП-бар дійсно видно і активовано
        if (hasHpText)
        {
            hpText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();

        if (TowerManager.Instance != null && !isStructure)
            TowerManager.Instance.AddKill();

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (animator != null) animator.SetTrigger("Die");

        // Вимикаємо колайдер відразу, щоб мертвий моб не блокував снаряди, що летять далі
        if (mobCollider != null) mobCollider.enabled = false;

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

        if (hasHpSlider) hpSlider.gameObject.SetActive(false);
        if (hasHpText) hpText.gameObject.SetActive(false);

        if (!isStructure)
        {
            if (mySpawner != null) mySpawner.EnemyDied();
            if (towerSpawner != null) towerSpawner.EnemyDied(gameObject);
            if (LevelManager.Instance != null) LevelManager.Instance.UnregisterEnemy();
        }

        CheckForUniqueQuestDrop();

        if (hasLootDropper) lootDropper.DropLoot();

        // Системи менеджерів квестів
        if (!isStructure && QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAction(QuestType.KillInTower, enemyID);
            QuestManager.Instance.OnQuestAction(QuestType.KillSpecific, enemyID);
        }

        if (!isStructure && DailyQuestManager.Instance != null)
        {
            DailyQuestManager.Instance.AddProgress(DailyQuestType.KillEnemies, 1);
            if (isElite)
            {
                DailyQuestManager.Instance.AddProgress(DailyQuestType.KillElite, 1);
            }
        }

        // Повністю прибираємо Debug.Log, який навантажував збірку гри
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
            }
        }
    }
}