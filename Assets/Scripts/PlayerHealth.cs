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

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        UpdateUI();
    }

    // --- НОВИЙ МЕТОД ДЛЯ ЛІКУВАННЯ ---
    public void Heal(int amount)
    {
        if (isDead) return; // Мертвого не вилікувати

        currentHealth += amount;

        // Обмежуємо здоров'я максимальним значенням
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        Debug.Log("Гравець підібрав зілля! Поточне здоров'я: " + currentHealth);
        UpdateUI();
    }
    // --------------------------------

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

        if (GetComponent<PlayerMovement>() != null) GetComponent<PlayerMovement>().enabled = false;
        if (GetComponent<PlayerCombat>() != null) GetComponent<PlayerCombat>().enabled = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}