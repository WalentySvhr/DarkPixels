using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    private bool isDead = false;

    [Header("UI References")]
    public Slider playerHPProgressBar;
    public TextMeshProUGUI hpText;
    public GameObject gameOverPanel;

    [Header("Damage Visuals")]
    public GameObject damagePopupPrefab;
    public Vector3 popupOffset = new Vector3(0, 1.5f, 0);

    // --- НОВІ ПОЛЯ ДЛЯ СУМАРНОЇ РЕГЕНЕРАЦІЇ ---
    private int amuletRegen = 0;
    private int ringRegen = 0;
    private Coroutine regenCoroutine;

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

        // Debug лог для пошуку джерела урону
        // Debug.LogWarning("<color=red>Damage: " + damage + "</color>\n" + StackTraceUtility.ExtractStackTrace());

        currentHealth -= damage;
        SpawnDamageText(damage);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        UpdateUI();
    }

    private void SpawnDamageText(int amount)
    {
        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + popupOffset, Quaternion.identity);
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();
            if (popupScript != null) popupScript.Setup(amount);
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        if (FXManager.instance != null) FXManager.instance.SpawnHealText(amount);

        UpdateUI();
    }

    // --- ОНОВЛЕНІ МЕТОДИ ДЛЯ БОНУСНОГО ХП ---
    // Вони працюють універсально і для амулетів, і для кілець
    public void AddBonusHealth(int bonus)
    {
        maxHealth += bonus;
        currentHealth += bonus;
        UpdateUI();
    }

    public void RemoveBonusHealth(int bonus)
    {
        maxHealth -= bonus;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateUI();
    }

    // --- ОНОВЛЕНА ЛОГІКА РЕГЕНЕРАЦІЇ ---

    public void StartRegen(int amount, bool isAmulet = true)
    {
        if (isAmulet) amuletRegen = amount;
        else ringRegen = amount;

        // Перезапускаємо корутину тільки якщо вона ще не запущена
        if (regenCoroutine == null)
        {
            regenCoroutine = StartCoroutine(RegenRoutine());
        }
    }

    public void StopRegen(bool isAmulet = true)
    {
        if (isAmulet) amuletRegen = 0;
        else ringRegen = 0;

        // Якщо обидва джерела регену по 0 — зупиняємо корутину повністю
        if (amuletRegen == 0 && ringRegen == 0 && regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }
    }

    private System.Collections.IEnumerator RegenRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            int totalRegen = amuletRegen + ringRegen;

            if (totalRegen > 0 && currentHealth < maxHealth && !isDead)
            {
                currentHealth += totalRegen;
                if (currentHealth > maxHealth) currentHealth = maxHealth;

                if (FXManager.instance != null)
                {
                    FXManager.instance.SpawnHealText(totalRegen);
                }

                UpdateUI();
                Debug.Log($"<color=white>Регенерація спрацювала: +{totalRegen} (A:{amuletRegen} + R:{ringRegen})</color>");
            }

            // Якщо регену більше немає, виходимо з циклу
            if (amuletRegen == 0 && ringRegen == 0) break;
        }
        regenCoroutine = null;
    }

    // Лікування від зони (ApplyHeal) залишається без змін
    public void ApplyHeal(int amount)
    {
        if (isDead) return;
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        if (FXManager.instance != null) FXManager.instance.SpawnHealText(amount);
        UpdateUI();
    }

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
        amuletRegen = 0;
        ringRegen = 0;
        if (regenCoroutine != null) StopCoroutine(regenCoroutine);

        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (GetComponent<PlayerMovement>() != null) GetComponent<PlayerMovement>().enabled = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}