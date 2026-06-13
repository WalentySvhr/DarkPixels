using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 500;
    private int currentHealth;
    private bool isDead = false;

    [Header("Animation & Visuals")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    [Tooltip("Колір, в який фарбується бос при отриманні шкоди")]
    public Color flashColor = Color.red;
    [Tooltip("Тривалість блимання кольором")]
    public float flashDuration = 0.15f;
    [Tooltip("Затримка перед видаленням, щоб анімація смерті встигла програтися")]
    public float deathAnimationDuration = 2f;

    private Color originalColor;
    private Coroutine flashCoroutine;

    [Header("UI")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    [Header("Effects")]
    public GameObject damagePopupPrefab;

    [Header("Exit Logic")]
    [Tooltip("Об'єкт дверей на сцені має називатися 'DoorToWorld'")]
    public GameObject exitDoor;

    public void SetHealth(float multiplier)
    {
        maxHealth = Mathf.RoundToInt(maxHealth * multiplier);
        currentHealth = maxHealth;
        UpdateUI();

        Debug.Log($"<color=cyan>BossHealth ініціалізовано:</color> Нове HP = {maxHealth}");
    }

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
            UpdateUI();
        }

        if (exitDoor == null)
        {
            exitDoor = GameObject.Find("DoorToWorld");
        }

        if (exitDoor != null)
        {
            var renderer = exitDoor.GetComponent<SpriteRenderer>();
            var col = exitDoor.GetComponent<Collider2D>();

            if (renderer != null) renderer.enabled = false;
            if (col != null) col.enabled = false;
        }
        else
        {
            Debug.LogError("КРИТИЧНА ПОМИЛКА: Бос не знайшов 'DoorToWorld' на сцені! Перевір назву об'єкта.");
        }
    }

    public void TakeDamage(int damage, bool isCrit = false)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();

        if (animator != null) animator.SetTrigger("TakeDamage");

        if (spriteRenderer != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        SpawnDamagePopup(damage, isCrit);

        BossCombat combatScript = GetComponent<BossCombat>();
        if (combatScript != null)
        {
            combatScript.OnDamageReceived();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    void SpawnDamagePopup(int damageAmount, bool isCrit)
    {
        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + (Vector3.up * 1.5f), Quaternion.identity);
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();

            if (popupScript != null) popupScript.Setup(damageAmount, isCrit);
        }
    }

    void UpdateUI()
    {
        if (hpSlider != null) hpSlider.value = (float)currentHealth / maxHealth;
        if (hpText != null) hpText.text = currentHealth + " / " + maxHealth;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("БОС ПЕРЕМОЖЕНИЙ!");

        StopAllCoroutines();
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        if (animator != null) animator.SetTrigger("Die");

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }

        Collider2D bossCollider = GetComponent<Collider2D>();
        if (bossCollider != null) bossCollider.enabled = false;

        BossCombat combatScript = GetComponent<BossCombat>();
        if (combatScript != null) combatScript.enabled = false;

        if (hpSlider != null) hpSlider.gameObject.SetActive(false);
        if (hpText != null) hpText.gameObject.SetActive(false);

        // --- МОДИФІКОВАНО ---
        // Викликаємо оновлений універсальний LootDropper
        LootDropper dropper = GetComponent<LootDropper>();
        if (dropper != null)
        {
            dropper.DropLoot();
        }
        else
        {
            Debug.LogWarning($"На об'єкті {gameObject.name} не знайдено компонент LootDropper! Лут не випаде.");
        }

        // Відкриття дверей
        if (exitDoor != null)
        {
            var renderer = exitDoor.GetComponent<SpriteRenderer>();
            var col = exitDoor.GetComponent<Collider2D>();

            if (renderer != null) renderer.enabled = true;
            if (col != null) col.enabled = true;

            LocalTeleport teleportScript = exitDoor.GetComponent<LocalTeleport>();
            if (teleportScript != null)
            {
                teleportScript.isActive = true;
                Debug.Log("Двері відкриті!");
            }
        }

        Destroy(gameObject, deathAnimationDuration);
    }
}