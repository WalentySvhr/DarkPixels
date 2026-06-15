using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerMana : MonoBehaviour
{
    [Header("Mana Settings")]
    public float maxMana = 100f;
    public float currentMana;
    [SerializeField] private float baseRegenRate = 2f; // Базовий реген (одиниць у секунду)
    private bool isDead = false;

    [Header("UI References")]
    public Slider playerManaProgressBar;
    public TextMeshProUGUI manaText;

    // --- ПОЛЯ ДЛЯ СУМАРНОЇ РЕГЕНЕРАЦІЇ МАННИ (як в PlayerHealth) ---
    [HideInInspector] public float amuletManaRegen = 0f;
    [HideInInspector] public float ringManaRegen = 0f;
    [HideInInspector] public float helmetManaRegen = 0f;
    [HideInInspector] public float chestplateManaRegen = 0f;
    [HideInInspector] public float bracersManaRegen = 0f;

    private Coroutine regenCoroutine;

    void Start()
    {
        currentMana = maxMana;
        UpdateUI();

        // Запускаємо постійний реген (він працюватиме завжди, поки гравець живий)
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

        return false; // Манна закінчилася, менеджер скілів повинен її вирубити
    }

    // Корутина регенерації (працює раз на секунду)
    private IEnumerator RegenRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(1f);

            // Рахуємо сумарний реген: база + шмот
            float totalRegen = baseRegenRate + amuletManaRegen + ringManaRegen + helmetManaRegen + chestplateManaRegen + bracersManaRegen;

            if (totalRegen > 0 && currentMana < maxMana)
            {
                currentMana += totalRegen;
                if (currentMana > maxMana) currentMana = maxMana;

                // Спавн спливаючого тексту регену через глобальний FXManager
                SpawnManaRegenText(totalRegen);

                UpdateUI();
            }
        }
        regenCoroutine = null;
    }

    private void SpawnManaRegenText(float amount)
    {
        // Стукаємо в глобальний FXManager і просимо його намалювати текст регену манни
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
            // Округлюємо до цілих перед виводом на екран
            manaText.text = Mathf.RoundToInt(currentMana) + " / " + Mathf.RoundToInt(maxMana);
        }
    }

    // --- МЕТОДИ ДЛЯ ЕКІПІРУВАННЯ (викликатимуться з PlayerEquipment) ---
    public void StartManaBuffs(float regenBonus, int slotType)
    {
        if (slotType == 0) amuletManaRegen = regenBonus;
        else if (slotType == 1) ringManaRegen = regenBonus;
        else if (slotType == 2) helmetManaRegen = regenBonus;
        else if (slotType == 3) chestplateManaRegen = regenBonus;
        else if (slotType == 4) bracersManaRegen = regenBonus;
    }

    public void StopManaBuffs(int slotType)
    {
        if (slotType == 0) amuletManaRegen = 0f;
        else if (slotType == 1) ringManaRegen = 0f;
        else if (slotType == 2) helmetManaRegen = 0f;
        else if (slotType == 3) chestplateManaRegen = 0f;
        else if (slotType == 4) bracersManaRegen = 0f;
    }

    // Інтеграція зі смертю/ревайвом (викликаються з PlayerHealth автоматично)
    public void OnPlayerDeath()
    {
        isDead = true;
        amuletManaRegen = 0;
        ringManaRegen = 0;
        helmetManaRegen = 0;
        chestplateManaRegen = 0;
        bracersManaRegen = 0;

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