using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic; // --- ДОДАНО для використання Dictionary ---

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

    // --- УНІВЕРСАЛЬНА СИСТЕМА ДИНАМІЧНИХ ХАРАКТЕРИСТИК ---
    // Ключ int — це ID вашого слоту (0 = амулет, 1 = кільце, 2 = шолом і т.д.)
    private Dictionary<int, float> armorSlots = new Dictionary<int, float>();
    private Dictionary<int, int> regenSlots = new Dictionary<int, int>();

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

    // Метод для динамічного підрахунку всієї броні в словнику
    public float GetTotalArmor()
    {
        float total = 0f;
        foreach (var armorValue in armorSlots.Values)
        {
            total += armorValue;
        }
        return total;
    }

    // Метод для динамічного підрахунку всього регену в словнику
    public int GetTotalRegen()
    {
        int total = 0;
        foreach (var regenValue in regenSlots.Values)
        {
            total += regenValue;
        }
        return total;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // --- УНІВЕРСАЛЬНИЙ ПІДРАХУНОК БРОНІ ---
        float totalArmor = GetTotalArmor();

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

    // --- АБСОЛЮТНО УНІВЕРСАЛЬНИЙ СТАРТ БАФІВ (БЕЗ IF/ELSE) ---
    public void StartBuffs(int regen, float armorValue, int slotType)
    {
        // Словники самі додають або оновлюють значення для будь-якого slotType!
        armorSlots[slotType] = armorValue;
        regenSlots[slotType] = regen;

        // Вмикаємо регенерацію, якщо вона є хоч десь і корутина ще не запущена
        if (regenCoroutine == null && GetTotalRegen() > 0)
        {
            regenCoroutine = StartCoroutine(RegenRoutine());
        }
    }

    // --- АБСОЛЮТНО УНІВЕРСАЛЬНА ЗУПИНКА БАФІВ ---
    public void StopBuffs(int slotType)
    {
        // Просто вичищаємо бафи цього слоту зі словника
        armorSlots.Remove(slotType);
        regenSlots.Remove(slotType);

        // Якщо регену більше немає ніде — зупиняємо корутину
        if (GetTotalRegen() == 0 && regenCoroutine != null)
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

            int totalRegen = GetTotalRegen();

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

            if (totalRegen == 0) break;
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

        // --- УНІВЕРСАЛЬНЕ ОБНУЛЕННЯ СЛОВНИКІВ ---
        armorSlots.Clear();
        regenSlots.Clear();

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