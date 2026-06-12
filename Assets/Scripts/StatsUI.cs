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

        // Отримуємо посилання на систему екіпіровки для підрахунку чистих бонусів
        PlayerEquipment eq = FindFirstObjectByType<PlayerEquipment>();

        // --- 1. БОЙОВІ ХАРАКТЕРИСТИКИ ---
        if (playerCombat != null)
        {
            // ⚔️ УРОН
            int baseDamage = playerCombat.currentWeaponData != null ? playerCombat.currentWeaponData.damage : playerCombat.unarmedDamage;
            int bonusDamage = playerCombat.extraAmuletDamage + playerCombat.extraRingDamage;
            int totalDamage = baseDamage + bonusDamage;

            if (damageText != null)
            {
                string bonusStr = bonusDamage > 0 ? $" <color={bonusColorTag}>(+{bonusDamage})</color>" : "";
                damageText.text = $"{damageLabel}{totalDamage}{bonusStr}";
            }

            // ⚡ ШВИДКІСТЬ АТАКИ (APS)
            if (attackSpeedText != null)
            {
                float baseCd = playerCombat.currentWeaponData != null ? playerCombat.currentWeaponData.cooldown : playerCombat.unarmedCooldown;
                float totalBonus = playerCombat.extraAttackSpeed + playerCombat.extraRingAttackSpeed;

                float baseAps = 1f / baseCd;
                float totalAps = baseAps * (1f + totalBonus);
                float bonusAps = totalAps - baseAps;

                string bonusStr = bonusAps > 0.05f ? $" <color={bonusColorTag}>(+{bonusAps:F1})</color>" : "";
                attackSpeedText.text = $"{attackSpeedLabel}{totalAps:F1} aps{bonusStr}";
            }

            // 🎯 ШАНС КРИТА
            if (critChanceText != null)
            {
                float totalCritPct = playerCombat.critChance * 100f;
                float bonusCritPct = 0f;

                if (eq != null)
                {
                    bonusCritPct = ((eq.currentAmulet?.bonusCritChance ?? 0f) +
                                    (eq.currentBelt?.bonusCritChance ?? 0f) +
                                    (eq.currentHelmet?.bonusCritChance ?? 0f) +
                                    (eq.currentRing1?.bonusCritChance ?? 0f) +
                                    (eq.currentRing2?.bonusCritChance ?? 0f)) * 100f;
                }

                string bonusStr = bonusCritPct > 0f ? $" <color={bonusColorTag}>(+{Mathf.RoundToInt(bonusCritPct)}%)</color>" : "";
                critChanceText.text = $"{critChanceLabel}{Mathf.RoundToInt(totalCritPct)}%{bonusStr}";
            }

            // 🔥 СИЛА КРИТА
            if (critDamageText != null)
            {
                float bonusCritM = 0f;
                if (eq != null)
                {
                    bonusCritM += (eq.currentAmulet != null && eq.currentAmulet.bonusCritMultiplier > 2f) ? eq.currentAmulet.bonusCritMultiplier - 2f : 0f;
                    bonusCritM += (eq.currentBelt != null && eq.currentBelt.bonusCritMultiplier > 2f) ? eq.currentBelt.bonusCritMultiplier - 2f : 0f;
                    bonusCritM += (eq.currentHelmet != null && eq.currentHelmet.bonusCritMultiplier > 2f) ? eq.currentHelmet.bonusCritMultiplier - 2f : 0f;
                    if (eq.currentRing1 != null && eq.currentRing1.bonusCritMultiplier > 2f) bonusCritM += eq.currentRing1.bonusCritMultiplier - 2f;
                    if (eq.currentRing2 != null && eq.currentRing2.bonusCritMultiplier > 2f) bonusCritM += eq.currentRing2.bonusCritMultiplier - 2f;
                }

                string bonusStr = bonusCritM > 0f ? $" <color={bonusColorTag}>(+{bonusCritM:F1}x)</color>" : "";
                critDamageText.text = $"{critDamageLabel}x{playerCombat.critMultiplier}{bonusStr}";
            }
        }

        // --- 2. ВИЖИВАННЯ ---
        if (playerHealth != null)
        {
            // ❤️ МАКСИМАЛЬНЕ ЗДОРОВ'Я
            if (maxHealthText != null)
            {
                int bonusHealth = 0;
                if (eq != null)
                {
                    bonusHealth = (eq.currentAmulet?.bonusMaxHealth ?? 0) +
                                  (eq.currentBelt?.bonusMaxHealth ?? 0) +
                                  (eq.currentHelmet?.bonusMaxHealth ?? 0) +
                                  (eq.currentRing1?.bonusMaxHealth ?? 0) +
                                  (eq.currentRing2?.bonusMaxHealth ?? 0) +
                                  (int)(eq.currentPet?.bonusHealth ?? 0f);
                }

                string bonusStr = bonusHealth > 0 ? $" <color={bonusColorTag}>(+{bonusHealth})</color>" : "";
                maxHealthText.text = $"{healthLabel}{playerHealth.maxHealth}{bonusStr}";
            }

            // 🛡️ БРОНЯ
            if (armorText != null)
            {
                // 1. Рахуємо сумарну чисельну броню
                float totalArmor = playerHealth.amuletArmor + playerHealth.ringArmor + playerHealth.helmetArmor;

                // 2. Рахуємо реальний відсоток поглинання шкоди за нашою формулою (K = 400)
                float K = 400f;
                float multiplier = K / (K + totalArmor);
                float damageReduction = (1f - multiplier) * 100f;
                int damageReductionPct = Mathf.RoundToInt(damageReduction);

                // 3. Формуємо рядок з бонусом. Тепер показуємо чисельну броню як основне значення.
                string bonusStr = totalArmor > 0 ? $" <color={bonusColorTag}>(+{damageReductionPct}% )</color>" : ""; // protection/захисту в процентах для гравця, який не розуміє формулу броні, але бачить реальний ефект від неї. 


                // Виведе на екран наприклад: Захист: 120 (+23% захисту)
                armorText.text = $"{armorLabel}{totalArmor}{bonusStr}";
            }

            // 🧪 РЕГЕНЕРАЦІЯ
            if (healthRegenText != null)
            {
                int totalRegen = playerHealth.amuletRegen + playerHealth.ringRegen + playerHealth.helmetRegen;

                string bonusStr = totalRegen > 0 ? $" <color={bonusColorTag}>(+{totalRegen})</color>" : "";
                healthRegenText.text = $"{regenLabel}+{totalRegen} HP/s{bonusStr}";
            }
        }

        // --- 3. ШВИДКІСТЬ РУХУ ---
        if (playerMovement != null && moveSpeedText != null)
        {
            float totalSpeedBonus = playerMovement.extraSpeedMultiplier + playerMovement.extraRingSpeedMultiplier;
            float baseSpeed = playerMovement.moveSpeed;
            float finalSpeed = baseSpeed * (1f + totalSpeedBonus);
            float bonusSpeed = finalSpeed - baseSpeed;

            string bonusStr = bonusSpeed > 0.05f ? $" <color={bonusColorTag}>(+{bonusSpeed:F1})</color>" : "";
            moveSpeedText.text = $"{speedLabel}{finalSpeed:F1}{bonusStr}";
        }
    }
}