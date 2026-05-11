using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; // НОВЕ: Необхідно для IEnumerator

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

        // --- НОВЕ: АНІМАЦІЯ ТА ОГЛУШЕННЯ ---
        if (animator != null) animator.SetTrigger("TakeDamage");

        if (spriteRenderer != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        // Зупиняємо гравця через скрипт пересування (якщо час оглушення більше нуля)
        PlayerMovement movementScript = GetComponent<PlayerMovement>();
        if (movementScript != null && hitStunDuration > 0f) // <-- Додали перевірку
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

    // НОВЕ: Корутина для ефекту блимання
    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    // НОВЕ: Корутина для короткої зупинки руху гравця
    private IEnumerator HitStunRoutine(PlayerMovement movementScript)
    {
        movementScript.enabled = false; // Вимикаємо керування

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Зупиняємо фізичний рух
        }

        yield return new WaitForSeconds(hitStunDuration);

        if (!isDead) // Перевіряємо, чи не помер гравець під час оглушення
        {
            movementScript.enabled = true; // Вмикаємо керування назад
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
        Heal(amount); // Використовуємо вже існуючий метод Heal, щоб не дублювати код
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
        if (isDead) return; // Захист від повторного виклику
        isDead = true;

        amuletRegen = 0;
        ringRegen = 0;
        amuletArmorPercent = 0f;
        ringArmorPercent = 0f;

        StopAllCoroutines(); // Зупиняємо реген, блимання та стан оглушення

        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        if (animator != null) animator.SetTrigger("Die");

        if (GetComponent<PlayerMovement>() != null) GetComponent<PlayerMovement>().enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Гасимо інерцію
            rb.isKinematic = true;
        }

        // Замість миттєвої зупинки часу, краще викликати Game Over із затримкою, 
        // щоб гравець встиг побачити свою анімацію смерті.
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        // Чекаємо 1.5 секунди, поки програється анімація смерті
        yield return new WaitForSeconds(1.5f);

        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}