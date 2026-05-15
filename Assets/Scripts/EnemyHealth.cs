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

    [Header("Quest Drop Settings")]
    [Tooltip("Список Target ID квестів, які можуть випасти саме з цього моба")]
    public List<string> allowedQuestItemIDs = new List<string>();

    [Header("Daily Quest Settings")]
    [Tooltip("Поставте галочку, якщо цей моб - елітний/міні-бос")]
    public bool isElite = false; // НОВЕ: Для квесту "Мисливець за головами"

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

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

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (animator != null) animator.SetTrigger("Die");

        Collider2D mobCollider = GetComponent<Collider2D>();
        if (mobCollider != null) mobCollider.enabled = false;

        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.StopAllCoroutines();
            ai.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        // if (rb != null)
        // {
        //     rb.velocity = Vector2.zero; 
        //     rb.isKinematic = true;      
        // }

        if (hpSlider != null) hpSlider.gameObject.SetActive(false);
        if (hpText != null) hpText.gameObject.SetActive(false);

        if (mySpawner != null) mySpawner.EnemyDied();
        if (towerSpawner != null) towerSpawner.EnemyDied(gameObject);
        if (LevelManager.Instance != null) LevelManager.Instance.UnregisterEnemy();

        CheckForUniqueQuestDrop();

        if (lootDropper != null) lootDropper.DropLoot();

        // --- Звичайні квести ---
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAction(QuestType.KillInTower, "");
        }

        // --- НОВЕ: Щоденні квести ---
        if (DailyQuestManager.Instance != null)
        {
            // Зараховуємо вбивство звичайного моба
            DailyQuestManager.Instance.AddProgress(DailyQuestType.KillEnemies, 1);

            // Якщо це елітний ворог, зараховуємо ще й квест на елітку
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