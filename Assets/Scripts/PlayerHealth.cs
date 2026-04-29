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
    public GameObject damagePopupPrefab; // Префаб із твоїм скриптом DamagePopup
    public Vector3 popupOffset = new Vector3(0, 1.5f, 0); // Позиція появи тексту над головою

    // Змінна для збереження посилання на активну регенерацію
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

        // --- ДОДАЙ ЦЕЙ РЯДОК ДЛЯ ПОШУКУ БАГА ---
        Debug.LogWarning("<color=red>Хтось наніс мені " + damage + " урону! Хто це зробив?</color>\n" + StackTraceUtility.ExtractStackTrace());
        // ---------------------------------------

        currentHealth -= damage;
        SpawnDamageText(damage);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        UpdateUI();
    }
    // Метод для створення спливаючого тексту урону
    private void SpawnDamageText(int amount)
    {
        if (damagePopupPrefab != null)
        {
            // Створюємо префаб у позиції гравця із невеликим зміщенням вгору
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + popupOffset, Quaternion.identity);

            // Отримуємо компонент DamagePopup і викликаємо Setup
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();
            if (popupScript != null)
            {
                popupScript.Setup(amount);
            }
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (FXManager.instance != null)
        {
            FXManager.instance.SpawnHealText(amount);
        }

        Debug.Log("Гравець підібрав зілля! Поточне здоров'я: " + currentHealth);
        UpdateUI();
    }

    // --- МЕТОДИ ДЛЯ КУЛОНІВ ТА БРОНІ ---

    public void AddBonusHealth(int bonus)
    {
        maxHealth += bonus; // Збільшуємо максимальне ХП
        currentHealth += bonus; // Даємо трохи здоров'я відразу
        UpdateUI(); // Миттєво оновлюємо смужку ХП на екрані
    }

    public void RemoveBonusHealth(int bonus)
    {
        maxHealth -= bonus; // Забираємо бонусне ХП

        // Якщо поточне ХП після зняття кулона більше за новий максимум - обрізаємо зайве
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        UpdateUI(); // Миттєво оновлюємо смужку ХП на екрані
    }

    // Запуск регенерації
    public void StartRegen(int regenAmount)
    {
        Debug.Log("<color=cyan>StartRegen викликано!</color> Значення: " + regenAmount); // ДОДАЙ ЦЕ
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
        }
        regenCoroutine = StartCoroutine(RegenRoutine(regenAmount));
    }
    // Зупинка регенерації
    public void StopRegen()
    {
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }
    }

    //регенерація від зони (метод, який викликає зона)
    // Регенерація від зони (метод, який викликає зона)
    public void ApplyHeal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        // --- ДОДАЙ ЦЕЙ БЛОК СЮДИ ---
        if (FXManager.instance != null)
        {
            FXManager.instance.SpawnHealText(amount);
        }
        // ---------------------------

        UpdateUI();
        Debug.Log("<color=green>Лікування від зони:</color> " + amount);
    }

    // Сама логіка регенерації (Корутина)
    private System.Collections.IEnumerator RegenRoutine(int regenAmount)
    {
        Debug.Log("<color=yellow>Корутина RegenRoutine почала цикл!</color>");
        while (true)
        {
            yield return new WaitForSeconds(1f);

            // Перевіряємо, чи живий гравець і чи потрібно йому ХП
            if (currentHealth < maxHealth && !isDead)
            {
                currentHealth += regenAmount;

                // Обмежуємо максимум
                if (currentHealth > maxHealth) currentHealth = maxHealth;

                // --- ДОДАЄМО ТЕКСТ ДЛЯ АМУЛЕТА ТУТ ---
                if (FXManager.instance != null)
                {
                    FXManager.instance.SpawnHealText(regenAmount);
                }
                // -------------------------------------

                UpdateUI();
                Debug.Log("<color=white>Регенерація амулета спрацювала! Поточне ХП:</color> " + currentHealth);
            }
        }
    }
    // -----------------------------------

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

        // Зупиняємо регенерацію при смерті
        StopRegen();

        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Вимикаємо рух
        if (GetComponent<PlayerMovement>() != null)
            GetComponent<PlayerMovement>().enabled = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}