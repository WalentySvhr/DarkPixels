using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
    // Сюди спавнер сам запише себе при створенні ворога
    // public PolygonAreaSpawner mySpawner;

    private LootDropper lootDropper;

    void Start()
    {
        // 1. Спочатку дізнаємося, на якому ми поверсі та наскільки треба посилити моба
        if (TowerManager.Instance != null)
        {
            float multiplier = TowerManager.Instance.GetDifficultyMultiplier(); // тут помилка

            // Збільшуємо максимальне здоров'я згідно з поверхом башти
            // Mathf.RoundToInt використано, щоб здоров'я було цілим числом
            maxHealth = Mathf.RoundToInt(maxHealth * multiplier);
        }

        // 2. Встановлюємо поточне здоров'я вже з урахуванням бонусу
        currentHealth = maxHealth;

        lootDropper = GetComponent<LootDropper>();

        // 3. Оновлюємо слайдер (смужку ХП) новими значеннями
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = maxHealth;
        }

        if (LevelManager.Instance != null) LevelManager.Instance.RegisterEnemy();

        UpdateHealthUI();
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection, float force)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        // Отримуємо посилання на EnemyAI
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.isAggroedByDamage = true;
            // ВИКЛИК НОВОЇ МЕХАНІКИ: ворог перевірить, чи повинен він тікати
            ai.OnTakeDamage();
        }

        SpawnDamagePopup(damage);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void SpawnDamagePopup(int damageAmount)
    {
        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up, Quaternion.identity);
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();
            if (popupScript != null) popupScript.Setup(damageAmount);
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

        // ПОВІДОМЛЯЄМО ВІДПОВІДНИЙ СПАВНЕР
        if (mySpawner != null)
        {
            mySpawner.EnemyDied();
        }

        if (towerSpawner != null)
        {
            towerSpawner.EnemyDied(gameObject);
        }

        if (LevelManager.Instance != null) LevelManager.Instance.UnregisterEnemy();
        if (lootDropper != null) lootDropper.DropLoot();

        // --- ДОДАНО: Повідомляємо квестову систему ---
        if (QuestManager.Instance != null)
        {
            // Передаємо тип квесту і порожній рядок (будь-який моб)
            QuestManager.Instance.OnQuestAction(QuestType.KillInTower, "");
        }
        // ---------------------------------------------

        Destroy(gameObject);
    }
}