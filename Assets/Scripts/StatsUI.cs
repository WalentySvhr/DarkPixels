using UnityEngine;
using TMPro;

public class StatsUI : MonoBehaviour
{
    public static StatsUI Instance;

    [Header("Посилання на скрипти гравця")]
    public PlayerCombat playerCombat;
    public PlayerHealth playerHealth;
    public PlayerMovement playerMovement;

    [Header("UI Текстові поля")]
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI attackSpeedText;
    public TextMeshProUGUI critChanceText;
    public TextMeshProUGUI critDamageText;
    public TextMeshProUGUI maxHealthText;
    public TextMeshProUGUI armorText;
    public TextMeshProUGUI healthRegenText;
    public TextMeshProUGUI moveSpeedText;

    [Header("Налаштування відображення (Inspector)")]
    public string damageLabel = "Урон: ";
    public string attackSpeedLabel = "Швидкість: ";
    public string critChanceLabel = "Шанс крита: ";
    public string critDamageLabel = "Сила крита: ";
    public string healthLabel = "Здоров'я: ";
    public string armorLabel = "Броня: ";
    public string regenLabel = "Регенерація: ";
    public string speedLabel = "Швидкість бігу: ";

    [Header("Кольори бонусів")]
    public string bonusColorTag = "#00FF00"; // Зелений

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        UpdateStatsUI();
    }

    public void UpdateStatsUI()
    {
        if (playerCombat == null) playerCombat = FindFirstObjectByType<PlayerCombat>();
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerMovement == null) playerMovement = FindFirstObjectByType<PlayerMovement>();

        // --- 1. БОЙОВІ ХАРАКТЕРИСТИКИ ---
        if (playerCombat != null)
        {
            // Урон
            int baseDamage = playerCombat.currentWeaponData != null ? playerCombat.currentWeaponData.damage : playerCombat.unarmedDamage;
            int bonusDamage = playerCombat.extraAmuletDamage + playerCombat.extraRingDamage;
            int totalDamage = baseDamage + bonusDamage;

            if (damageText != null)
            {
                string bonusStr = bonusDamage > 0 ? $" <color={bonusColorTag}>(+{bonusDamage})</color>" : "";
                damageText.text = $"{damageLabel}{totalDamage}{bonusStr}";
            }

            // Швидкість атаки (APS - Attacks Per Second)
            if (attackSpeedText != null)
            {
                float baseCd = playerCombat.currentWeaponData != null ? playerCombat.currentWeaponData.cooldown : playerCombat.unarmedCooldown;
                float totalBonus = playerCombat.extraAttackSpeed + playerCombat.extraRingAttackSpeed;

                // Формула: (1 / Базовий кулдаун) * (1 + Бонус %)
                float aps = (1f / baseCd) * (1f + totalBonus);
                attackSpeedText.text = $"{attackSpeedLabel}{aps:F1} уд/сек";
            }

            // Шанс крита
            if (critChanceText != null)
            {
                float critPct = playerCombat.critChance * 100f;
                critChanceText.text = $"{critChanceLabel}{Mathf.RoundToInt(critPct)}%";
            }

            // Сила крита
            if (critDamageText != null)
            {
                critDamageText.text = $"{critDamageLabel}x{playerCombat.critMultiplier}";
            }
        }

        // --- 2. ВИЖИВАННЯ ---
        if (playerHealth != null)
        {
            if (maxHealthText != null) maxHealthText.text = $"{healthLabel}{playerHealth.maxHealth}";

            if (armorText != null)
            {
                float totalArmor = playerHealth.amuletArmorPercent + playerHealth.ringArmorPercent;
                totalArmor = Mathf.Clamp(totalArmor, 0f, 0.9f);
                armorText.text = $"{armorLabel}{Mathf.RoundToInt(totalArmor * 100f)}%";
            }

            if (healthRegenText != null)
            {
                int totalRegen = playerHealth.amuletRegen + playerHealth.ringRegen;
                string color = totalRegen > 0 ? bonusColorTag : "#FFFFFF";
                healthRegenText.text = $"{regenLabel}<color={color}>+{totalRegen} HP/сек</color>";
            }
        }

        // --- 3. ШВИДКІСТЬ РУХУ ---
        if (playerMovement != null && moveSpeedText != null)
        {
            float totalSpeedBonus = playerMovement.extraSpeedMultiplier + playerMovement.extraRingSpeedMultiplier;
            float finalSpeed = playerMovement.moveSpeed * (1f + totalSpeedBonus);
            moveSpeedText.text = $"{speedLabel}{finalSpeed:F1}";
        }
    }
}