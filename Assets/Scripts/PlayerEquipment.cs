using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Header("Посилання на системи гравця")]
    public PlayerHealth playerHealth;
    public PlayerCombat playerCombat;
    public PlayerMovement playerMovement;

    [Header("Поточна екіпіровка")]
    public AmuletData currentAmulet;
    public RingData currentRing1; // Слот для першого кільця
    public RingData currentRing2; // Слот для другого кільця

    [Header("Тестування (T/U - Амулет, Y/I - Кільце 1, H/J - Кільце 2)")]
    public AmuletData testAmulet;
    public RingData testRing;

    // --- ЛОГІКА АМУЛЕТА ---
    public void EquipAmulet(AmuletData newAmulet)
    {
        if (newAmulet == null || currentAmulet == newAmulet) return;
        if (currentAmulet != null) UnequipAmulet();

        currentAmulet = newAmulet;

        if (playerHealth != null)
            playerHealth.AddBonusHealth(currentAmulet.bonusMaxHealth);

        UpdateAllStats(); // Оновлюємо всі суми
        Debug.Log($"<color=green>Одягнено амулет:</color> {currentAmulet.name}");
    }

    public void UnequipAmulet()
    {
        if (currentAmulet == null) return;

        if (playerHealth != null)
            playerHealth.RemoveBonusHealth(currentAmulet.bonusMaxHealth);

        currentAmulet = null;
        UpdateAllStats();
        Debug.Log($"<color=red>Знято амулет.</color>");
    }

    // --- ЛОГІКА ДЛЯ 2-Х КІЛЕЦЬ ---
    public void EquipRing(RingData newRing, int slotIndex)
    {
        if (newRing == null) return;

        // Спочатку знімаємо старе кільце з цього слота, якщо воно там було
        UnequipRing(slotIndex);

        // Ставимо нове кільце у відповідний слот
        if (slotIndex == 1) currentRing1 = newRing;
        else if (slotIndex == 2) currentRing2 = newRing;

        // Додаємо ХП від нового кільця
        if (playerHealth != null)
            playerHealth.AddBonusHealth(newRing.bonusMaxHealth);

        UpdateAllStats(); // Перераховуємо всі статі
        Debug.Log($"<color=cyan>Одягнено кільце в слот {slotIndex}:</color> {newRing.name}");
    }

    public void UnequipRing(int slotIndex)
    {
        RingData ringToRemove = (slotIndex == 1) ? currentRing1 : currentRing2;
        if (ringToRemove == null) return;

        // Віднімаємо ХП знятого кільця
        if (playerHealth != null)
            playerHealth.RemoveBonusHealth(ringToRemove.bonusMaxHealth);

        // Очищуємо слот
        if (slotIndex == 1) currentRing1 = null;
        else if (slotIndex == 2) currentRing2 = null;

        UpdateAllStats();
        Debug.Log($"<color=red>Знято кільце зі слота {slotIndex}</color>");
    }

    // --- МАГІЯ СУМУВАННЯ СТАТІВ ---
    // Цей метод збирає всі показники з Амулета та двох Кілець і передає їх гравцю
    private void UpdateAllStats()
    {
        // 1. Збираємо стати Амулета
        int amDmg = currentAmulet != null ? currentAmulet.bonusDamage : 0;
        float amAtkSpd = currentAmulet != null ? currentAmulet.bonusAttackSpeed : 0f;
        float amMovSpd = currentAmulet != null ? currentAmulet.bonusMoveSpeed : 0f;
        float amCrit = currentAmulet != null ? currentAmulet.bonusCritChance : 0f;
        float amCritM = (currentAmulet != null && currentAmulet.bonusCritMultiplier > 2f) ? currentAmulet.bonusCritMultiplier - 2f : 0f;
        int amRegen = currentAmulet != null ? currentAmulet.healthRegenPerSecond : 0;
        float amArmor = currentAmulet != null ? currentAmulet.bonusArmorPercent : 0f;

        // 2. Збираємо і СУМУЄМО стати обох Кілець
        int rDmg = 0; float rAtkSpd = 0f; float rMovSpd = 0f;
        float rCrit = 0f; float rCritM = 0f; int rRegen = 0; float rArmor = 0f;

        if (currentRing1 != null)
        {
            rDmg += currentRing1.bonusDamage;
            rAtkSpd += currentRing1.bonusAttackSpeed;
            rMovSpd += currentRing1.bonusMoveSpeed;
            rCrit += currentRing1.bonusCritChance;
            rRegen += currentRing1.healthRegenPerSecond;
            rArmor += currentRing1.bonusArmorPercent;
            if (currentRing1.bonusCritMultiplier > 2f) rCritM += currentRing1.bonusCritMultiplier - 2f;
        }
        if (currentRing2 != null)
        {
            rDmg += currentRing2.bonusDamage;
            rAtkSpd += currentRing2.bonusAttackSpeed;
            rMovSpd += currentRing2.bonusMoveSpeed;
            rCrit += currentRing2.bonusCritChance;
            rRegen += currentRing2.healthRegenPerSecond;
            rArmor += currentRing2.bonusArmorPercent;
            if (currentRing2.bonusCritMultiplier > 2f) rCritM += currentRing2.bonusCritMultiplier - 2f;
        }

        // 3. Передаємо суми в системи гравця
        if (playerCombat != null)
        {
            playerCombat.extraAmuletDamage = amDmg;
            playerCombat.extraRingDamage = rDmg; // Урон обох кілець разом

            playerCombat.extraAttackSpeed = amAtkSpd;
            playerCombat.extraRingAttackSpeed = rAtkSpd;

            // Кріти сумуємо повністю (амулет + обидва кільця)
            playerCombat.critChance = amCrit + rCrit;
            playerCombat.critMultiplier = 2f + amCritM + rCritM; // 2f - базова шкода х2
        }

        if (playerMovement != null)
        {
            playerMovement.extraSpeedMultiplier = amMovSpd;
            playerMovement.extraRingSpeedMultiplier = rMovSpd; // Швидкість обох кілець разом
        }

        if (playerHealth != null)
        {
            playerHealth.StartBuffs(amRegen, amArmor, true);  // Передаємо бафи амулета
            playerHealth.StartBuffs(rRegen, rArmor, false);   // Передаємо сумарні бафи кілець
        }
    }

    void Update()
    {
        // Тестування кнопками з клавіатури
        if (Input.GetKeyDown(KeyCode.T)) if (testAmulet != null) EquipAmulet(testAmulet);
        if (Input.GetKeyDown(KeyCode.U)) UnequipAmulet();

        if (Input.GetKeyDown(KeyCode.Y)) if (testRing != null) EquipRing(testRing, 1);
        if (Input.GetKeyDown(KeyCode.I)) UnequipRing(1);

        if (Input.GetKeyDown(KeyCode.H)) if (testRing != null) EquipRing(testRing, 2);
        if (Input.GetKeyDown(KeyCode.J)) UnequipRing(2);
    }
}