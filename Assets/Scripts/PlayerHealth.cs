using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    private bool isDead = false;

    [Header("Animation & Visuals")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    [Tooltip("Колір, в який фарбується гравець при отриманні шкоди")]
    public Color flashColor = Color.red;
    [Tooltip("Тривалість блимання кольором")]
    public float flashDuration = 0.1f;
    [Tooltip("Затримка руху при отриманні шкоди")]
    public float hitStunDuration = 0.2f;

    private Color originalColor;
    private Coroutine flashCoroutine;

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

    // --- ПОЛЯ ДЛЯ СУМАРНОЇ РЕГЕНЕРАЦІЇ ---
    [HideInInspector] public int amuletRegen = 0;
    [HideInInspector] public int ringRegen = 0;
    private Coroutine regenCoroutine;

    void Start()
    {
        currentHealth = maxHealth;

        // Автоматично шукаємо компоненти, якщо вони не призначені
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // --- ЛОГІКА ЗАХИСТУ ---
        float totalArmor = amuletArmorPercent + ringArmorPercent;
        totalArmor = Mathf.Clamp(totalArmor, 0f, 0.9f);
        float multiplier = 1f - totalArmor;
        int finalDamage = Mathf.CeilToInt(damage * multiplier);
        if (finalDamage <= 0 && damage > 0 && totalArmor < 1f) finalDamage = 1;

        currentHealth -= finalDamage;
        SpawnDamageText(finalDamage);

        // --- АНІМАЦІЯ ТА ОГЛУШЕННЯ ---
        if (animator != null) animator.SetTrigger("TakeDamage");

        if (spriteRenderer != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        // Зупиняємо гравця через скрипт пересування (якщо час оглушення більше нуля)
        PlayerMovement movementScript = GetComponent<PlayerMovement>();
        if (movementScript != null && hitStunDuration > 0f)
        {
            StartCoroutine(HitStunRoutine(movementScript));
        }

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

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    private IEnumerator HitStunRoutine(PlayerMovement movementScript)
    {
        movementScript.isStunned = true; // Блокуємо керування м'яко

        yield return new WaitForSeconds(hitStunDuration);

        if (!isDead)
        {
            movementScript.isStunned = false; // Вмикаємо керування назад
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

    private IEnumerator RegenRoutine()
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
        Heal(amount);
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
        if (isDead) return;
        isDead = true;

        // Зверни увагу: при смерті бонуси обнуляються. Після Revive() вони не повернуться автоматично.
        amuletRegen = 0;
        ringRegen = 0;
        amuletArmorPercent = 0f;
        ringArmorPercent = 0f;

        StopAllCoroutines();

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (animator != null) animator.SetTrigger("Die");

        PlayerMovement movementScript = GetComponent<PlayerMovement>();
        if (movementScript != null)
        {
            movementScript.isStunned = true; // Блокуємо джойстик
            movementScript.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }

        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- НОВИЙ МЕТОД: ВІДРОДЖЕННЯ ---
    public void Revive()
    {
        Time.timeScale = 1f; // Відновлюємо час

        if (gameOverPanel != null) gameOverPanel.SetActive(false); // Ховаємо панель смерті

        isDead = false;
        currentHealth = maxHealth; // Повертаємо повне здоров'я
        UpdateUI();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = false; // Вмикаємо фізику назад
        }

        PlayerMovement movementScript = GetComponent<PlayerMovement>();
        if (movementScript != null)
        {
            movementScript.enabled = true;
            movementScript.isStunned = false; // Розблоковуємо керування
        }

        if (animator != null)
        {
            animator.SetTrigger("Revive"); // Скидаємо анімацію смерті через тригер
        }

        Debug.Log("<color=green>Гравець відродився за рекламу!</color>");
    }
    // Метод, який ми прив'яжемо до кнопки "Відродитися за рекламу" в UI
    public void OnClickReviveWithAd()
    {
        if (AdsChecker.Instance != null)
        {
            AdsChecker.Instance.RequestAd(AdsChecker.RewardType.RevivePlayer);
        }
        else
        {
            Debug.LogError("AdsChecker не знайдено!");
        }
    }
}