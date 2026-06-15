using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerMana : MonoBehaviour
{
    [Header("Базові налаштування (без шмоту)")]
    [SerializeField] private float baseMaxMana = 100f;
    [SerializeField] private float baseRegenRate = 2f; // Базовий реген (одиниць у секунду)

    public float currentMana { get; private set; }
    private bool isDead = false;

    [Header("UI References")]
    public Slider playerManaProgressBar;
    public TextMeshProUGUI manaText;

    // --- ДИНАМІЧНІ СЛОВНИКИ МОДИФІКАТОРІВ (Як у PlayerHealth) ---
    private Dictionary<string, int> maxManaModifiers = new Dictionary<string, int>();
    private Dictionary<string, int> manaRegenModifiers = new Dictionary<string, int>();

    // 🌟 ДИНАМІЧНА МАКСИМАЛЬНА МАНА (База + сума всіх бонусів зі шмоту)
    public int maxMana
    {
        get
        {
            int totalBonus = 0;
            foreach (var bonus in maxManaModifiers.Values)
            {
                totalBonus += bonus;
            }
            return Mathf.RoundToInt(baseMaxMana) + totalBonus;
        }
    }

    // 🌟 ДИНАМІЧНА РЕГЕНЕРАЦІЯ МАНА (База + сума всіх бонусів зі шмоту)
    public int totalManaRegen
    {
        get
        {
            int totalBonus = 0;
            foreach (var bonus in manaRegenModifiers.Values)
            {
                totalBonus += bonus;
            }
            return Mathf.RoundToInt(baseRegenRate) + totalBonus;
        }
    }

    private Coroutine regenCoroutine;

    void Start()
    {
        // Заповнюємо ману на старті відповідно до поточної maxMana
        currentMana = maxMana;
        UpdateUI();

        // Запускаємо постійний реген
        if (regenCoroutine == null)
        {
            regenCoroutine = StartCoroutine(RegenRoutine());
        }
    }

    // МЕХАНІКА 1: Разове споживання (Миттєвий скіл)
    public bool TrySpendMana(float amount)
    {
        if (isDead || currentMana < amount) return false;

        currentMana -= amount;
        UpdateUI();
        return true;
    }

    // МЕХАНІКА 2: Активне споживання (Потоковий скіл / Аура щокадру)
    public bool SpendManaOverTime(float amountPerSecond)
    {
        if (isDead) return false;

        float amountThisFrame = amountPerSecond * Time.deltaTime;

        if (currentMana >= amountThisFrame)
        {
            currentMana -= amountThisFrame;
            UpdateUI();
            return true;
        }

        return false;
    }

    // Корутина регенерації (працює раз на секунду)
    private IEnumerator RegenRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(1f);

            int regen = totalManaRegen;

            if (regen > 0 && currentMana < maxMana)
            {
                currentMana += regen;
                if (currentMana > maxMana) currentMana = maxMana;

                // Спавн спливаючого тексту регену
                SpawnManaRegenText(regen);
                UpdateUI();
            }
        }
        regenCoroutine = null;
    }

    private void SpawnManaRegenText(float amount)
    {
        if (FXManager.instance != null)
        {
            FXManager.instance.SpawnManaText(Mathf.RoundToInt(amount));
        }
    }

    public void UpdateUI()
    {
        if (playerManaProgressBar != null)
        {
            playerManaProgressBar.maxValue = maxMana;
            playerManaProgressBar.value = currentMana;
        }

        if (manaText != null)
        {
            manaText.text = Mathf.RoundToInt(currentMana) + " / " + maxMana;
        }
    }

    // =================================================================
    // 🌟 УНІВЕРСАЛЬНІ МЕТОДИ ДЛЯ КЕРУВАННЯ БОНУСАМИ (Аналог PlayerHealth)
    // =================================================================

    /// <summary>
    /// Додає або оновлює бонус до максимальної мани та регенерації від предмета.
    /// </summary>
    /// <param name="sourceID">Унікальний ID предмета або назва слоту (напр. "Bracers", "Amulet")</param>
    public void AddManaEquipmentBonuses(string sourceID, int bonusMaxMana, int bonusRegen)
    {
        // Ставимо/оновлюємо значення у словниках
        maxManaModifiers[sourceID] = bonusMaxMana;
        manaRegenModifiers[sourceID] = bonusRegen;

        // Корректуємо поточну ману, щоб вона не перевищувала новий ліміт
        if (currentMana > maxMana) currentMana = maxMana;

        // Миттєво перемальовуємо UI
        UpdateUI();

        // Оновлюємо вікно статів StatsUI, якщо воно відкрите
        if (StatsUI.Instance != null) StatsUI.Instance.UpdateStatsUI();
    }

    /// <summary>
    /// Повністю видаляє бонуси предмета (викликається при знятті шмотки)
    /// </summary>
    public void RemoveManaEquipmentBonuses(string sourceID)
    {
        if (maxManaModifiers.ContainsKey(sourceID)) maxManaModifiers.Remove(sourceID);
        if (manaRegenModifiers.ContainsKey(sourceID)) manaRegenModifiers.Remove(sourceID);

        if (currentMana > maxMana) currentMana = maxMana;

        UpdateUI();

        if (StatsUI.Instance != null) StatsUI.Instance.UpdateStatsUI();
    }

    // =================================================================

    public void OnPlayerDeath()
    {
        isDead = true;
        maxManaModifiers.Clear();
        manaRegenModifiers.Clear();

        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }
    }

    public void OnPlayerRevive()
    {
        isDead = false;
        currentMana = maxMana;
        UpdateUI();

        if (regenCoroutine == null)
        {
            regenCoroutine = StartCoroutine(RegenRoutine());
        }
    }
}