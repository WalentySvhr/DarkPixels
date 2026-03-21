using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    [Header("UI References")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    [Header("Effects")]
    public GameObject damagePopupPrefab; 

    private LootDropper lootDropper;

    void Start()
    {
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

    public void TakeDamage(int damage, Vector2 knockbackDirection, float force)
    {
        if (isDead) return; 

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.isAggroedByDamage = true; 

        SpawnDamagePopup(damage);
        
        // StunRoutine() прибрано, щоб вороги не зупинялися при ударі

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
        
        // Зупиняємо всі активні корутини (наприклад, якщо якісь ще працювали)
        StopAllCoroutines();
        
        if (LevelManager.Instance != null) LevelManager.Instance.UnregisterEnemy();

        if (lootDropper != null) 
        {
            lootDropper.DropLoot();
        }
        
        Destroy(gameObject);
    }
}