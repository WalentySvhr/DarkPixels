using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    private bool isDead = false;

    [Header("Animation & Visuals")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Color flashColor = Color.red;
    public float flashDuration = 0.1f;
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
    [SerializeField] private float K = 400f;

    private Dictionary<int, float> armorSlots = new Dictionary<int, float>();
    private Dictionary<int, int> regenSlots = new Dictionary<int, int>();

    private Coroutine regenCoroutine;

    // --- КЕШУВАННЯ ДЛЯ ОПТИМІЗАЦІЇ ---
    private PlayerMovement playerMovement;
    private PlayerMana playerMana;
    private PlayerEquipment playerEquipment;
    private WaitForSeconds oneSecondWait;

    void Start()
    {
        currentHealth = maxHealth;

        // Кешуємо компоненти один раз при старті
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
        playerMana = GetComponent<PlayerMana>();
        playerEquipment = GetComponent<PlayerEquipment>();

        // Створюємо очікування один раз, щоб не плодити сміття в пам'яті
        oneSecondWait = new WaitForSeconds(1f);

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateUI();
    }

    public float GetTotalArmor()
    {
        float total = 0f;
        foreach (var armorValue in armorSlots.Values)
        {
            total += armorValue;
        }
        return total;
    }

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

        float totalArmor = GetTotalArmor();
        float multiplier = K / (K + totalArmor);
        int finalDamage = Mathf.CeilToInt(damage * multiplier);

        if (finalDamage <= 0 && damage > 0) finalDamage = 1;

        currentHealth -= finalDamage;
        SpawnDamageText(finalDamage);

        if (animator != null) animator.SetTrigger("TakeDamage");

        if (spriteRenderer != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        // Використовуємо закешований скрипт руху без GetComponent
        if (playerMovement != null && hitStunDuration > 0f)
        {
            StartCoroutine(HitStunRoutine());
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        UpdateUI();
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    private IEnumerator HitStunRoutine()
    {
        playerMovement.isStunned = true;
        yield return new WaitForSeconds(hitStunDuration);
        if (!isDead)
        {
            playerMovement.isStunned = false;
        }
    }

    private void SpawnDamageText(int amount)
    {
        if (damagePopupPrefab != null)
        {
            // ТУТ НАДАЛІ БАЖАНО ЗАМІНИТИ НА ОБ'ЄКТНИЙ ПУЛ:
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

    public void StartBuffs(int regen, float armorValue, int slotType)
    {
        armorSlots[slotType] = armorValue;
        regenSlots[slotType] = regen;

        if (regenCoroutine == null && GetTotalRegen() > 0)
        {
            regenCoroutine = StartCoroutine(RegenRoutine());
        }
    }

    public void StopBuffs(int slotType)
    {
        armorSlots.Remove(slotType);
        regenSlots.Remove(slotType);

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
            yield return oneSecondWait; // Використовуємо оптимізовану змінну очікування

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

        armorSlots.Clear();
        regenSlots.Clear();

        if (playerMana != null)
        {
            playerMana.OnPlayerDeath();
        }

        StopAllCoroutines();

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        if (animator != null) animator.SetTrigger("Die");

        if (playerMovement != null)
        {
            playerMovement.isStunned = true;
            playerMovement.enabled = false;
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

        if (playerMana != null)
        {
            playerMana.OnPlayerRevive();
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = false;

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            playerMovement.isStunned = false;
        }

        if (animator != null) animator.SetTrigger("Revive");

        if (playerEquipment != null)
        {
            playerEquipment.UpdateAllStats();
        }
    }

    public void OnClickReviveWithAd()
    {
        if (AdsChecker.Instance != null)
        {
            AdsChecker.Instance.RequestAd(AdsChecker.RewardType.RevivePlayer);
        }
    }
}