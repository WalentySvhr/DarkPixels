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

    [Header("Захист (Чисельна Armor)")]
    [Tooltip("Точка балансу: при такій кількості загальної броні шкода зменшується рівно на 50%")]
    [SerializeField] private float K = 400f;

    public float amuletArmor = 0f;
    public float ringArmor = 0f;
    public float helmetArmor = 0f;
    public float chestplateArmor = 0f; // --- ДОДАНО ---
    public float bracersArmor = 0f;    // --- ДОДАНО ---

    // --- ПОЛЯ ДЛЯ СУМАРНОЇ РЕГЕНЕРАЦІЇ ---
    [HideInInspector] public int amuletRegen = 0;
    [HideInInspector] public int ringRegen = 0;
    [HideInInspector] public int helmetRegen = 0;
    [HideInInspector] public int chestplateRegen = 0; // --- ДОДАНО ---
    [HideInInspector] public int bracersRegen = 0;    // --- ДОДАНО ---
    private Coroutine regenCoroutine;

    void Start()
    {
        currentHealth = maxHealth;

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

        // --- НОВА ЛОГІКА ЧИСЕЛЬНОГО ЗАХИСТУ (ОНОВЛЕНО) ---
        float totalArmor = amuletArmor + ringArmor + helmetArmor + chestplateArmor + bracersArmor;

        // Розраховуємо коефіцієнт зменшення шкоди (Dota 2 / LoL формула)
        float multiplier = K / (K + totalArmor);

        int finalDamage = Mathf.CeilToInt(damage * multiplier);

        // Гарантуємо мінімум 1 одиницю шкоди, якщо атака нанесла хоч щось
        if (finalDamage <= 0 && damage > 0) finalDamage = 1;

        currentHealth -= finalDamage;
        SpawnDamageText(finalDamage);

        // --- АНІМАЦІЯ ТА ОГЛУШЕННЯ ---
        if (animator != null) animator.SetTrigger("TakeDamage");

        if (spriteRenderer != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

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
            int blockedDamage = damage - finalDamage;
            Debug.Log($"<color=blue>Броня ({totalArmor}) спрацювала! Заблоковано: {blockedDamage} урону. Отримано: {finalDamage}</color>");
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
        movementScript.isStunned = true;

        yield return new WaitForSeconds(hitStunDuration);

        if (!isDead)
        {
            movementScript.isStunned = false;
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

    // --- ОНОВЛЕНО: Тепер приймає чисельне значення armorValue замість відсотків ---
    public void StartBuffs(int regen, float armorValue, int slotType)
    {
        if (slotType == 0)
        {
            amuletRegen = regen;
            amuletArmor = armorValue;
        }
        else if (slotType == 1)
        {
            ringRegen = regen;
            ringArmor = armorValue;
        }
        else if (slotType == 2)
        {
            helmetRegen = regen;
            helmetArmor = armorValue;
        }
        else if (slotType == 3) // --- ДОДАНО: Нагрудник ---
        {
            chestplateRegen = regen;
            chestplateArmor = armorValue;
        }
        else if (slotType == 4) // --- ДОДАНО: Наручі ---
        {
            bracersRegen = regen;
            bracersArmor = armorValue;
        }

        // --- ОНОВЛЕНО: Перевірка всіх слотів на наявність регену ---
        if (regenCoroutine == null && (amuletRegen > 0 || ringRegen > 0 || helmetRegen > 0 || chestplateRegen > 0 || bracersRegen > 0))
        {
            regenCoroutine = StartCoroutine(RegenRoutine());
        }
    }

    public void StopBuffs(int slotType)
    {
        if (slotType == 0)
        {
            amuletRegen = 0;
            amuletArmor = 0f;
        }
        else if (slotType == 1)
        {
            ringRegen = 0;
            ringArmor = 0f;
        }
        else if (slotType == 2)
        {
            helmetRegen = 0;
            helmetArmor = 0f;
        }
        else if (slotType == 3) // --- ДОДАНО: Нагрудник ---
        {
            chestplateRegen = 0;
            chestplateArmor = 0f;
        }
        else if (slotType == 4) // --- ДОДАНО: Наручі ---
        {
            bracersRegen = 0;
            bracersArmor = 0f;
        }

        // --- ОНОВЛЕНО: Умова зупинки корутини ---
        if (amuletRegen == 0 && ringRegen == 0 && helmetRegen == 0 && chestplateRegen == 0 && bracersRegen == 0 && regenCoroutine != null)
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

            // --- ОНОВЛЕНО: Сума всього регену ---
            int totalRegen = amuletRegen + ringRegen + helmetRegen + chestplateRegen + bracersRegen;

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

            // --- ОНОВЛЕНО: Умова виходу ---
            if (amuletRegen == 0 && ringRegen == 0 && helmetRegen == 0 && chestplateRegen == 0 && bracersRegen == 0) break;
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

        // --- ОНОВЛЕНО: Обнулення нових слотів при смерті ---
        amuletRegen = 0;
        ringRegen = 0;
        helmetRegen = 0;
        chestplateRegen = 0;
        bracersRegen = 0;

        amuletArmor = 0f;
        ringArmor = 0f;
        helmetArmor = 0f;
        chestplateArmor = 0f;
        bracersArmor = 0f;

        // --- СИНХРОНІЗАЦІЯ З МАННОЮ ПРИ СМЕРТІ ---
        PlayerMana manaScript = GetComponent<PlayerMana>();
        if (manaScript != null)
        {
            manaScript.OnPlayerDeath();
        }

        StopAllCoroutines();

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (animator != null) animator.SetTrigger("Die");

        PlayerMovement movementScript = GetComponent<PlayerMovement>();
        if (movementScript != null)
        {
            movementScript.isStunned = true;
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

    public void Revive()
    {
        Time.timeScale = 1f;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        isDead = false;
        currentHealth = maxHealth;
        UpdateUI();

        // --- СИНХРОНІЗАЦІЯ З МАННОЮ ПРИ ВІДРОДЖЕННІ ---
        PlayerMana manaScript = GetComponent<PlayerMana>();
        if (manaScript != null)
        {
            manaScript.OnPlayerRevive();
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = false;

        PlayerMovement movementScript = GetComponent<PlayerMovement>();
        if (movementScript != null)
        {
            movementScript.enabled = true;
            movementScript.isStunned = false;
        }

        if (animator != null) animator.SetTrigger("Revive");

        PlayerEquipment equipment = GetComponent<PlayerEquipment>();
        if (equipment != null)
        {
            equipment.UpdateAllStats();
        }

        Debug.Log("<color=green>Гравець відродився, чисельні стати та манну відновлено!</color>");
    }

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