using UnityEngine;
using TMPro;

public class StatsUI : MonoBehaviour
{
    public static StatsUI Instance;

    [Header("Посилання на скрипти гравця")]
    public PlayerCombat playerCombat;
    public PlayerHealth playerHealth;
    public PlayerMovement playerMovement;
    public PlayerMana playerMana; // 🌟 ДОДАНО ПОСИЛАННЯ НА МАНУ ДЛЯ ПРАВИЛЬНОГО ВІДОБРАЖЕННЯ

    [Header("UI Текстові поля")]
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI attackSpeedText;
    public TextMeshProUGUI critChanceText;
    public TextMeshProUGUI critDamageText;
    public TextMeshProUGUI maxHealthText;
    public TextMeshProUGUI armorText;
    public TextMeshProUGUI healthRegenText;
    public TextMeshProUGUI maxManaText;
    public TextMeshProUGUI manaRegenText;
    public TextMeshProUGUI moveSpeedText;

    [Header("Налаштування відображення (Inspector)")]
    public string damageLabel = "Урон: ";
    public string attackSpeedLabel = "Швидкість: ";
    public string critChanceLabel = "Шанс крита: ";
    public string critDamageLabel = "Сила крита: ";
    public string healthLabel = "Здоров'я: ";
    public string armorLabel = "Броня: ";
    public string regenLabel = "Регенерація: ";
    public string maxManaLabel = "Мана: ";
    public string manaRegenLabel = "Регенерація мани: ";
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
        if (playerMana == null) playerMana = FindFirstObjectByType<PlayerMana>(); // Знаходимо систему мани

        // Отримуємо посилання на систему екіпіровки
        PlayerEquipment eq = FindFirstObjectByType<PlayerEquipment>();

        // --- 1. БОЙОВІ ХАРАКТЕРИСТИКИ ---
        if (playerCombat != null)
        {
            // ⚔️ УРОН
            int baseDamage = playerCombat.currentWeaponData != null ? playerCombat.currentWeaponData.damage : playerCombat.unarmedDamage;
            int bonusDamage = playerCombat.extraAmuletDamage + playerCombat.extraRingDamage; // В PlayerEquipment сюди вже входить і урон від чобіт
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
                float totalBonus = playerCombat.extraAttackSpeed + playerCombat.extraRingAttackSpeed; // Сюди також уже входить бонус від чобіт через PlayerEquipment

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
                    // ➕ ДОДАНО: eq.currentBoots?.bonusCritChance
                    bonusCritPct = ((eq.currentAmulet?.bonusCritChance ?? 0f) +
                                    (eq.currentBelt?.bonusCritChance ?? 0f) +
                                    (eq.currentHelmet?.bonusCritChance ?? 0f) +
                                    (eq.currentChestplate?.bonusCritChance ?? 0f) +
                                    (eq.currentBracers?.bonusCritChance ?? 0f) +
                                    (eq.currentBoots?.bonusCritChance ?? 0f) +
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
                    if (eq.currentChestplate != null && eq.currentChestplate.bonusCritMultiplier > 2f) bonusCritM += eq.currentChestplate.bonusCritMultiplier - 2f;
                    if (eq.currentBracers != null && eq.currentBracers.bonusCritMultiplier > 2f) bonusCritM += eq.currentBracers.bonusCritMultiplier - 2f;
                    if (eq.currentRing1 != null && eq.currentRing1.bonusCritMultiplier > 2f) bonusCritM += eq.currentRing1.bonusCritMultiplier - 2f;
                    if (eq.currentRing2 != null && eq.currentRing2.bonusCritMultiplier > 2f) bonusCritM += eq.currentRing2.bonusCritMultiplier - 2f;
                    // ➕ ДОДАНО ДЛЯ ЧОБІТ:
                    if (eq.currentBoots != null && eq.currentBoots.bonusCritMultiplier > 2f) bonusCritM += eq.currentBoots.bonusCritMultiplier - 2f;
                }

                string bonusStr = bonusCritM > 0f ? $" <color={bonusColorTag}>(+{bonusCritM:F1}x)</color>" : "";
                critDamageText.text = $"{critDamageLabel}x{playerCombat.critMultiplier}{bonusStr}";
            }
        }

        // --- 2. ВИЖИВАННЯ ТА МАГІЯ ---
        if (playerHealth != null)
        {
            // ❤️ МАКСИМАЛЬНЕ ЗДОРОВ'Я
            if (maxHealthText != null)
            {
                int bonusHealth = 0;
                if (eq != null)
                {
                    // ➕ ДОДАНО: eq.currentBoots?.bonusMaxHealth
                    bonusHealth = (eq.currentAmulet?.bonusMaxHealth ?? 0) +
                                  (eq.currentBelt?.bonusMaxHealth ?? 0) +
                                  (eq.currentHelmet?.bonusMaxHealth ?? 0) +
                                  (eq.currentChestplate?.bonusMaxHealth ?? 0) +
                                  (eq.currentBracers?.bonusMaxHealth ?? 0) +
                                  (eq.currentBoots?.bonusMaxHealth ?? 0) +
                                  (eq.currentRing1?.bonusMaxHealth ?? 0) +
                                  (eq.currentRing2?.bonusMaxHealth ?? 0) +
                                  (int)(eq.currentPet?.bonusHealth ?? 0f);
                }

                string bonusStr = bonusHealth > 0 ? $" <color={bonusColorTag}>(+{bonusHealth})</color>" : "";
                maxHealthText.text = $"{healthLabel}{playerHealth.maxHealth}{bonusStr}";
            }

            // ✨ МАКСИМАЛЬНА МАНА
            if (maxManaText != null)
            {
                int bonusMana = 0;
                if (eq != null)
                {
                    // ➕ ДОДАНО: eq.currentBoots?.bonusMaxMana
                    bonusMana = (eq.currentAmulet?.bonusMaxMana ?? 0) +
                                (eq.currentBelt?.bonusMaxMana ?? 0) +
                                (eq.currentHelmet?.bonusMaxMana ?? 0) +
                                (eq.currentChestplate?.bonusMaxMana ?? 0) +
                                (eq.currentBracers?.bonusMaxMana ?? 0) +
                                (eq.currentBoots?.bonusMaxMana ?? 0) +
                                (eq.currentRing1?.bonusMaxMana ?? 0) +
                                (eq.currentRing2?.bonusMaxMana ?? 0);
                }

                string bonusStr = bonusMana > 0 ? $" <color={bonusColorTag}>(+{bonusMana})</color>" : "";

                // ✨ ВИПРАВЛЕНО ЗАГЛУШКУ: Отримуємо реальне значення макс. мани зі скрипта PlayerMana
                int displayMana = playerMana != null ? playerMana.maxMana : bonusMana;
                maxManaText.text = $"{maxManaLabel}{displayMana}{bonusStr}";
            }

            // 🛡️ БРОНЯ
            if (armorText != null)
            {
                float totalArmor = playerHealth.GetTotalArmor(); // В PlayerHealth вже враховано індекс бафу 5 (чоботи)

                float K = 400f;
                float multiplier = K / (K + totalArmor);
                float damageReduction = (1f - multiplier) * 100f;
                int damageReductionPct = Mathf.RoundToInt(damageReduction);

                string bonusStr = totalArmor > 0 ? $" <color={bonusColorTag}>(+{damageReductionPct}%)</color>" : "";
                armorText.text = $"{armorLabel}{totalArmor}{bonusStr}";
            }

            // 🧪 РЕГЕНЕРАЦІЯ ХП
            if (healthRegenText != null)
            {
                int totalRegen = playerHealth.GetTotalRegen(); // Тут теж автоматично враховано реген від чобіт

                string bonusStr = totalRegen > 0 ? $" <color={bonusColorTag}>(+{totalRegen})</color>" : "";
                healthRegenText.text = $"{regenLabel}+{totalRegen} HP/s{bonusStr}";
            }

            // 🧪 РЕГЕНЕРАЦІЯ МАНИ
            if (manaRegenText != null)
            {
                int totalManaRegen = 0;
                if (eq != null)
                {
                    // ➕ ДОДАНО: eq.currentBoots?.manaRegenPerSecond
                    totalManaRegen = (eq.currentAmulet?.manaRegenPerSecond ?? 0) +
                                     (eq.currentBelt?.manaRegenPerSecond ?? 0) +
                                     (eq.currentHelmet?.manaRegenPerSecond ?? 0) +
                                     (eq.currentChestplate?.manaRegenPerSecond ?? 0) +
                                     (eq.currentBracers?.manaRegenPerSecond ?? 0) +
                                     (eq.currentBoots?.manaRegenPerSecond ?? 0) +
                                     (eq.currentRing1?.manaRegenPerSecond ?? 0) +
                                     (eq.currentRing2?.manaRegenPerSecond ?? 0);
                }

                string bonusStr = totalManaRegen > 0 ? $" <color={bonusColorTag}>(+{totalManaRegen})</color>" : "";
                manaRegenText.text = $"{manaRegenLabel}+{totalManaRegen} MP/s{bonusStr}";
            }
        }

        // --- 3. ШВИДКІСТЬ РУХУ ---
        if (playerMovement != null && moveSpeedText != null)
        {
            float totalSpeedBonus = playerMovement.extraSpeedMultiplier + playerMovement.extraRingSpeedMultiplier;

            // ➕ ДОДАНО: Перевірка швидкості від чобіт (якщо пояс окремо перевірявся, чоботи мають бути тут обов'язково)
            if (eq != null)
            {
                if (eq.currentBelt != null) totalSpeedBonus += eq.currentBelt.bonusMoveSpeed;
                if (eq.currentBoots != null) totalSpeedBonus += eq.currentBoots.bonusMoveSpeed;
            }

            float baseSpeed = playerMovement.moveSpeed;
            float finalSpeed = baseSpeed * (1f + totalSpeedBonus);
            float bonusSpeed = finalSpeed - baseSpeed;

            string bonusStr = bonusSpeed > 0.05f ? $" <color={bonusColorTag}>(+{bonusSpeed:F1})</color>" : "";
            moveSpeedText.text = $"{speedLabel}{finalSpeed:F1}{bonusStr}";
        }
    }
}