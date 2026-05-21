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
    public BeltData currentBelt;  // Слот для пояса
    public PetData currentPet;    // Слот для пета
    public WeaponData currentWeapon; // --- ДОДАНО: Слот для зброї ---

    [Header("Тестування")]
    public AmuletData testAmulet;
    public RingData testRing;
    public BeltData testBelt;
    public PetData testPet;
    public WeaponData testWeapon; // --- ДОДАНО: Для тестування зброї ---

    // --- ЛОГІКА ЗБРОЇ (НОВЕ) ---
    public void EquipWeapon(WeaponData newWeapon)
    {
        if (newWeapon == null || currentWeapon == newWeapon) return;
        if (currentWeapon != null) UnequipWeapon();

        currentWeapon = newWeapon;

        // Передаємо зброю в PlayerCombat для візуалізації та застосування її статів
        if (playerCombat != null)
        {
            playerCombat.EquipWeapon(currentWeapon);
        }

        UpdateAllStats();
        Debug.Log($"<color=yellow>Екіпіровано зброю:</color> {currentWeapon.name}");
    }

    public void UnequipWeapon()
    {
        if (currentWeapon == null) return;

        currentWeapon = null;

        // Наказуємо PlayerCombat прибрати зброю
        if (playerCombat != null)
        {
            playerCombat.EquipWeapon(null);
        }

        UpdateAllStats();
        Debug.Log($"<color=red>Зброю знято. Бій руками.</color>");
    }

    // --- ЛОГІКА АМУЛЕТА ---
    public void EquipAmulet(AmuletData newAmulet)
    {
        if (newAmulet == null || currentAmulet == newAmulet) return;
        if (currentAmulet != null) UnequipAmulet();

        currentAmulet = newAmulet;

        if (playerHealth != null)
            playerHealth.AddBonusHealth(currentAmulet.bonusMaxHealth);

        UpdateAllStats();
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

    // --- ЛОГІКА ПОЯСА ---
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

        UnequipRing(slotIndex);

        if (slotIndex == 1) currentRing1 = newRing;
        else if (slotIndex == 2) currentRing2 = newRing;

        if (playerHealth != null)
            playerHealth.AddBonusHealth(newRing.bonusMaxHealth);

        UpdateAllStats();
        Debug.Log($"<color=cyan>Одягнено кільце в слот {slotIndex}:</color> {newRing.name}");
    }

    public void UnequipRing(int slotIndex)
    {
        RingData ringToRemove = (slotIndex == 1) ? currentRing1 : currentRing2;
        if (ringToRemove == null) return;

        if (playerHealth != null)
            playerHealth.RemoveBonusHealth(ringToRemove.bonusMaxHealth);

        if (slotIndex == 1) currentRing1 = null;
        else if (slotIndex == 2) currentRing2 = null;

        UpdateAllStats();
        Debug.Log($"<color=red>Знято кільце зі слота {slotIndex}</color>");
    }

    // --- ЛОГІКА ПЕТА ---
    public void EquipPet(PetData newPet)
    {
        if (newPet == null || currentPet == newPet) return;
        if (currentPet != null) UnequipPet();

        currentPet = newPet;

        if (playerHealth != null && currentPet.bonusHealth > 0f)
            playerHealth.AddBonusHealth((int)currentPet.bonusHealth);

        if (PetSpawner.Instance != null)
        {
            PetSpawner.Instance.SpawnPet(currentPet);
        }

        UpdateAllStats();
        Debug.Log($"<color=lime>Активовано помічника:</color> {currentPet.itemName}");
    }

    public void UnequipPet()
    {
        if (currentPet == null) return;

        if (playerHealth != null && currentPet.bonusHealth > 0f)
            playerHealth.RemoveBonusHealth((int)currentPet.bonusHealth);

        if (PetSpawner.Instance != null)
        {
            PetSpawner.Instance.DespawnPet();
        }

        currentPet = null;
        UpdateAllStats();
        Debug.Log($"<color=orange>Помічника відправлено відпочивати.</color>");
    }

    // --- МАГІЯ СУМУВАННЯ СТАТІВ ---
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

        // 2. Збираємо стати Пояса
        int bDmg = currentBelt != null ? currentBelt.bonusDamage : 0;
        float bAtkSpd = currentBelt != null ? currentBelt.bonusAttackSpeed : 0f;
        float bMovSpd = currentBelt != null ? currentBelt.bonusMoveSpeed : 0f;
        float bCrit = currentBelt != null ? currentBelt.bonusCritChance : 0f;
        float bCritM = (currentBelt != null && currentBelt.bonusCritMultiplier > 2f) ? currentBelt.bonusCritMultiplier - 2f : 0f;
        int bRegen = currentBelt != null ? currentBelt.healthRegenPerSecond : 0;
        float bArmor = currentBelt != null ? currentBelt.bonusArmorPercent : 0f;

        // 3. Збираємо стати Пета
        int petDmg = currentPet != null ? currentPet.bonusDamage : 0;

        // 4. Збираємо і СУМУЄМО стати обох Кілець
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

        // 5. Передаємо суми в системи гравця
        if (playerCombat != null)
        {
            playerCombat.extraAmuletDamage = amDmg + bDmg + petDmg;
            playerCombat.extraRingDamage = rDmg;

            playerCombat.extraAttackSpeed = amAtkSpd + bAtkSpd;
            playerCombat.extraRingAttackSpeed = rAtkSpd;

            playerCombat.critChance = amCrit + bCrit + rCrit;
            playerCombat.critMultiplier = 2f + amCritM + bCritM + rCritM;
        }

        if (playerMovement != null)
        {
            playerMovement.extraSpeedMultiplier = amMovSpd + bMovSpd;
            playerMovement.extraRingSpeedMultiplier = rMovSpd;
        }

        if (playerHealth != null)
        {
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

        if (Input.GetKeyDown(KeyCode.B)) if (testBelt != null) EquipBelt(testBelt);
        if (Input.GetKeyDown(KeyCode.N)) UnequipBelt();

        if (Input.GetKeyDown(KeyCode.P)) if (testPet != null) EquipPet(testPet);
        if (Input.GetKeyDown(KeyCode.O)) UnequipPet();

        // --- ДОДАНО: Клавіші для тесту зброї ---
        if (Input.GetKeyDown(KeyCode.G)) if (testWeapon != null) EquipWeapon(testWeapon);
        if (Input.GetKeyDown(KeyCode.F)) UnequipWeapon();
    }
}