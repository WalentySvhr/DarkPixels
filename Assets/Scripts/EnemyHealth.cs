using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic; // Обов'язково для List<>

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;    // обов'язково public float
    private bool isDead = false;

    [Header("UI References")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;
    public PolygonAreaSpawner mySpawner; // Для опена
    public TowerSpawner towerSpawner;    // Для башти

    [Header("Effects")]
    public GameObject damagePopupPrefab;

    [Header("Spawn Context")]
    private LootDropper lootDropper;

    // --- НОВЕ: Налаштування для унікального луту ---
    [Header("Quest Drop Settings")]
    [Tooltip("Список Target ID квестів, які можуть випасти саме з цього моба")]
    public List<string> allowedQuestItemIDs = new List<string>();

    void Start()
    {
        if (TowerManager.Instance != null)
        {
            float multiplier = TowerManager.Instance.GetDifficultyMultiplier();

            // Збільшуємо максимальне здоров'я згідно з поверхом башти
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

        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.isAggroedByDamage = true;
            ai.OnTakeDamage();
        }

        SpawnDamagePopup(damage, isCrit);

        if (currentHealth <= 0)
        {
            Die();
        }
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

        if (mySpawner != null)
        {
            mySpawner.EnemyDied();
        }

        if (towerSpawner != null)
        {
            towerSpawner.EnemyDied(gameObject);
        }

        if (LevelManager.Instance != null) LevelManager.Instance.UnregisterEnemy();

        // --- ЛОГІКА УНІКАЛЬНОГО КВЕСТОВОГО ЛУТУ ---
        CheckForUniqueQuestDrop();

        if (lootDropper != null) lootDropper.DropLoot(); // Звичайний лут

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAction(QuestType.KillInTower, "");
        }

        Destroy(gameObject);
    }

    void CheckForUniqueQuestDrop()
    {
        // Переконуємось, що менеджер і квест існують
        if (QuestManager.Instance == null || QuestManager.Instance.currentQuest == null) return;

        // Беремо ID цілі поточного квесту
        string questTargetID = QuestManager.Instance.currentQuest.targetID;

        // --- ПЕРЕВІРКА НА ДОЗВІЛ ---
        // Якщо список цього моба не містить потрібного ID, перериваємо метод (предмет не випаде)
        if (!allowedQuestItemIDs.Contains(questTargetID)) return;

        // Перевіряємо, чи є взагалі targetID, і чи дозволяє QuestManager йому випасти
        if (!string.IsNullOrEmpty(questTargetID) && QuestManager.Instance.TryDropQuestItem(questTargetID))
        {
            // Отримуємо предмет із поточного квесту
            Item itemToDrop = QuestManager.Instance.currentQuest.itemToCollect;

            if (itemToDrop != null)
            {
                // Використовуємо твій існуючий метод для фізичного дропу на землю
                QuestManager.Instance.DropItemOnGround(itemToDrop);
                Debug.Log($"<color=magenta>Унікальний квестовий предмет ({itemToDrop.itemName}) випав!</color>");
            }
        }
    }
}