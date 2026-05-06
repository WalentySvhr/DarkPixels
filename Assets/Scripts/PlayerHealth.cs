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

    [Header("Захист (Armor)")]
    [Range(0, 1f)] public float amuletArmorPercent = 0f; // 0.1 = 10% захисту
    [Range(0, 1f)] public float ringArmorPercent = 0f;   // 0.1 = 10% захисту

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

        // --- ЛОГІКА ЗАХИСТУ ---
        // Підсумовуємо весь захист
        float totalArmor = amuletArmorPercent + ringArmorPercent;

        // Обмежуємо захист на рівні 90% (0.9f), щоб гравець не став абсолютно безсмертним
        totalArmor = Mathf.Clamp(totalArmor, 0f, 0.9f);

        // Рахуємо фінальний урон: урон * (1 - відсоток захисту)
        // Наприклад: 10 урону * (1 - 0.2) = 8 урону.
        float multiplier = 1f - totalArmor;
        // Стане (завжди округлює вгору, 2.5 перетвориться на 3):
        int finalDamage = Mathf.CeilToInt(damage * multiplier);

        // Гарантуємо, що якщо ворог вдарив, то хоча б 1 одиниця урону пройде (якщо захист не 100%)
        if (finalDamage <= 0 && damage > 0 && totalArmor < 1f) finalDamage = 1;

        currentHealth -= finalDamage;
        SpawnDamageText(finalDamage);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        UpdateUI();

        if (totalArmor > 0)
        {
            Debug.Log($"<color=blue>Броня спрацювала! Заблоковано: {damage - finalDamage} урону. Отримано: {finalDamage}</color>");
        }
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

    // --- ОНОВЛЕНА ЛОГІКА РЕГЕНЕРАЦІЇ ТА ЗАХИСТУ ---

    public void StartBuffs(int regen, float armor, bool isAmulet = true)
    {
        if (isAmulet)
        {
            amuletRegen = regen;
            amuletArmorPercent = armor;
        }
        else
        {
            ringRegen = regen;
            ringArmorPercent = armor;
        }

        if (regenCoroutine == null && (amuletRegen > 0 || ringRegen > 0))
        {
            regenCoroutine = StartCoroutine(RegenRoutine());
        }
    }

    public void StopBuffs(bool isAmulet = true)
    {
        if (isAmulet)
        {
            amuletRegen = 0;
            amuletArmorPercent = 0f;
        }
        else
        {
            ringRegen = 0;
            ringArmorPercent = 0f;
        }

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
                Debug.Log($"<color=white>Регенерація: +{totalRegen}</color>");
            }

            if (amuletRegen == 0 && ringRegen == 0) break;
        }
        regenCoroutine = null;
    }

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
        amuletArmorPercent = 0f;
        ringArmorPercent = 0f;
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