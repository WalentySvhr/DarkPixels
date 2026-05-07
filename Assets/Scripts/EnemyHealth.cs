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
    private LootDropper lootDropper;

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

    // === ОНОВЛЕНО: Додано параметр bool isCrit = false ===
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

        // === ОНОВЛЕНО: Передаємо isCrit у метод створення попапу ===
        SpawnDamagePopup(damage, isCrit);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // === ОНОВЛЕНО: Метод тепер приймає bool isCrit ===
    void SpawnDamagePopup(int damageAmount, bool isCrit)
    {
        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up, Quaternion.identity);
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();

            // === ОНОВЛЕНО: Передаємо isCrit у скрипт попапу ===
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
        if (lootDropper != null) lootDropper.DropLoot();

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAction(QuestType.KillInTower, "");
        }

        Destroy(gameObject);
    }
}