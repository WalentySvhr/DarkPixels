using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealth : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 500;
    private int currentHealth;
    private bool isDead = false;

    [Header("UI")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    [Header("Exit Logic")]
    [Tooltip("Об'єкт дверей на сцені має називатися 'DoorToWorld'")]
    public GameObject exitDoor;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();

        // 1. Шукаємо двері на сцені за іменем
        if (exitDoor == null)
        {
            exitDoor = GameObject.Find("DoorToWorld");
        }

        // 2. Якщо знайшли, робимо їх невидимими, але залишаємо "активними" для коду
        if (exitDoor != null)
        {
            // Вимикаємо візуал і колайдер, щоб гравець не міг вийти раніше часу
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

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        UpdateUI();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
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

        // 1. Дроп луту
        BossLootDropper dropper = GetComponent<BossLootDropper>();
        if (dropper != null) dropper.DropBossLoot();

        // 2. АКТИВАЦІЯ ВИХОДУ
        if (exitDoor != null)
        {
            // Вмикаємо назад візуал і фізику
            var renderer = exitDoor.GetComponent<SpriteRenderer>();
            var col = exitDoor.GetComponent<Collider2D>();

            if (renderer != null) renderer.enabled = true;
            if (col != null) col.enabled = true;

            // Активуємо логіку телепорту всередині скрипта дверей
            LocalTeleport teleportScript = exitDoor.GetComponent<LocalTeleport>();
            if (teleportScript != null)
            {
                teleportScript.isActive = true;
                Debug.Log("Двері відкриті!");
            }
        }

        // 3. Плавне зникнення боса
        if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().enabled = false;
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        // Вимикаємо скрипти логіки, щоб бос не бився після смерті
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != this) script.enabled = false;
        }

        Destroy(gameObject, 0.5f);
    }
}