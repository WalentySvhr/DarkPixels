using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealth : MonoBehaviour
{
    [Header("Boss Health Settings")]
    public int maxHealth = 500;
    private int currentHealth;
    
    [Header("UI")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    [Header("Effects")]
    public GameObject damagePopupPrefab;

    void Start()
    {
        currentHealth = maxHealth;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = maxHealth;
        }
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();
        SpawnDamagePopup(damage);

        // Перевірка на смерть
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
        if (hpText != null) hpText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
    }

    void Die()
    {
        // Тут ми НЕ викликаємо LevelManager, тому спавн боса не буде циклічним
        Debug.Log("БОС ПЕРЕМОЖЕНИЙ!");
        
        // Можна додати окремий лут для боса
        Destroy(gameObject);
    }
}