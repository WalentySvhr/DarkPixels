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
    public BeltData currentBelt;  // --- ДОДАНО: Слот для пояса ---

    [Header("Тестування")]
    public AmuletData testAmulet;
    public RingData testRing;
    public BeltData testBelt; // --- ДОДАНО: Для тестування пояса ---

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

    // --- ЛОГІКА ПОЯСА (НОВЕ) ---
    public void EquipBelt(BeltData newBelt)
    {
        if (newBelt == null || currentBelt == newBelt) return;
        if (currentBelt != null) UnequipBelt();

        currentBelt = newBelt;

        if (playerHealth != null)
            playerHealth.AddBonusHealth(currentBelt.bonusMaxHealth);

        UpdateAllStats();
        Debug.Log($"<color=green>Одягнено пояс:</color> {currentBelt.name}");
    }

    public void UnequipBelt()
    {
        if (currentBelt == null) return;

        if (playerHealth != null)
            playerHealth.RemoveBonusHealth(currentBelt.bonusMaxHealth);

        currentBelt = null;
        UpdateAllStats();
        Debug.Log($"<color=red>Знято пояс.</color>");
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
    // Цей метод збирає всі показники з Амулета, Пояса та двох Кілець і передає їх гравцю
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

        // 2. Збираємо стати Пояса (НОВЕ)
        int bDmg = currentBelt != null ? currentBelt.bonusDamage : 0;
        float bAtkSpd = currentBelt != null ? currentBelt.bonusAttackSpeed : 0f;
        float bMovSpd = currentBelt != null ? currentBelt.bonusMoveSpeed : 0f;
        float bCrit = currentBelt != null ? currentBelt.bonusCritChance : 0f;
        float bCritM = (currentBelt != null && currentBelt.bonusCritMultiplier > 2f) ? currentBelt.bonusCritMultiplier - 2f : 0f;
        int bRegen = currentBelt != null ? currentBelt.healthRegenPerSecond : 0;
        float bArmor = currentBelt != null ? currentBelt.bonusArmorPercent : 0f;

        // 3. Збираємо і СУМУЄМО стати обох Кілець
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

        // 4. Передаємо суми в системи гравця
        if (playerCombat != null)
        {
            // Урон: Амулет + Кільця + Пояс
            playerCombat.extraAmuletDamage = amDmg + bDmg; // Для простоти додаємо урон пояса до змінної амулета
            playerCombat.extraRingDamage = rDmg;

            // Швидкість атаки: Амулет + Кільця + Пояс
            playerCombat.extraAttackSpeed = amAtkSpd + bAtkSpd;
            playerCombat.extraRingAttackSpeed = rAtkSpd;

            // Кріти сумуємо повністю (Амулет + Пояс + обидва кільця)
            playerCombat.critChance = amCrit + bCrit + rCrit;
            playerCombat.critMultiplier = 2f + amCritM + bCritM + rCritM;
        }

        if (playerMovement != null)
        {
            // Швидкість бігу: Амулет + Пояс + Кільця
            playerMovement.extraSpeedMultiplier = amMovSpd + bMovSpd;
            playerMovement.extraRingSpeedMultiplier = rMovSpd;
        }

        if (playerHealth != null)
        {
            // Оскільки в PlayerHealth є тільки слоти для "Amulet" та "Ring" бафів, 
            // ми хитро сумуємо реген і броню пояса з амулетом (isAmulet = true).
            // Це заощадить нам час на переписування PlayerHealth!
            playerHealth.StartBuffs(amRegen + bRegen, amArmor + bArmor, true);
            playerHealth.StartBuffs(rRegen, rArmor, false);
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

        // --- ДОДАНО: Тестування пояса ---
        if (Input.GetKeyDown(KeyCode.B)) if (testBelt != null) EquipBelt(testBelt);
        if (Input.GetKeyDown(KeyCode.N)) UnequipBelt();
    }
}