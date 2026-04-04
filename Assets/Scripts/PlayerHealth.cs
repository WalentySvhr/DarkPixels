using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    [Header("UI References")]
    public Slider playerHPProgressBar;
    public TextMeshProUGUI hpText;
    public GameObject gameOverPanel;

    [Header("Damage Visuals")]
    public GameObject damagePopupPrefab; // Префаб із твоїм скриптом DamagePopup
    public Vector3 popupOffset = new Vector3(0, 1.5f, 0); // Позиція появи тексту над головою

    void Start()
    {
        currentHealth = maxHealth;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // --- ВІДОБРАЖЕННЯ УРОНУ ---
        SpawnDamageText(damage);
        // --------------------------

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        UpdateUI();
    }

    // Метод для створення спливаючого тексту урону
    private void SpawnDamageText(int amount)
    {
        if (damagePopupPrefab != null)
        {
            // Створюємо префаб у позиції гравця із невеликим зміщенням вгору
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + popupOffset, Quaternion.identity);

            // Отримуємо компонент DamagePopup і викликаємо Setup
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();
            if (popupScript != null)
            {
                popupScript.Setup(amount);
            }
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (FXManager.instance != null)
        {
            FXManager.instance.SpawnHealText(amount);
        }

        Debug.Log("Гравець підібрав зілля! Поточне здоров'я: " + currentHealth);
        UpdateUI();
    }

    // --- МЕТОДИ ДЛЯ КУЛОНІВ ТА БРОНІ ---

    public void AddBonusHealth(int bonus)
    {
        maxHealth += bonus; // Збільшуємо максимальне ХП
        currentHealth += bonus; // Даємо трохи здоров'я відразу
        UpdateUI(); // Миттєво оновлюємо смужку ХП на екрані
    }

    public void RemoveBonusHealth(int bonus)
    {
        maxHealth -= bonus; // Забираємо бонусне ХП

        // Якщо поточне ХП після зняття кулона більше за новий максимум - обрізаємо зайве
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        UpdateUI(); // Миттєво оновлюємо смужку ХП на екрані
    }
    // -----------------------------------

    void UpdateUI()
    {
        if (playerHPProgressBar != null)
        {
            playerHPProgressBar.maxValue = maxHealth;
            playerHPProgressBar.value = currentHealth;
        }

        if (hpText != null)
        {
            hpText.text = currentHealth + " / " + maxHealth;
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Гравець загинув!");

        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Вимикаємо рух
        if (GetComponent<PlayerMovement>() != null)
            GetComponent<PlayerMovement>().enabled = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}