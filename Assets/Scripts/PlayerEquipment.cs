using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Header("Посилання на системи гравця")]
    public PlayerHealth playerHealth;
    public PlayerCombat playerCombat;
    public PlayerMovement playerMovement;

    [Header("Поточна екіпіровка")]
    public AmuletData currentAmulet;
    public RingData currentRing1;
    public RingData currentRing2;
    public BeltData currentBelt;
    public HelmetData currentHelmet;
    public ChestplateData currentChestplate;
    public BracersData currentBracers; // --- НОВИЙ ТИП: НАРУЧІ ---
    public PetData currentPet;
    public WeaponData currentWeapon;

    // --- ЛОГІКА ЗБРОЇ ---
    public void EquipWeapon(WeaponData newWeapon)
    {
        if (newWeapon == null || currentWeapon == newWeapon) return;
        if (currentWeapon != null) UnequipWeapon();

        currentWeapon = newWeapon;

        if (playerCombat != null) playerCombat.EquipWeapon(currentWeapon);
        UpdateAllStats();
    }

    public void UnequipWeapon()
    {
        if (currentWeapon == null) return;
        currentWeapon = null;
        if (playerCombat != null) playerCombat.EquipWeapon(null);
        UpdateAllStats();
    }

    // --- ЛОГІКА АМУЛЕТА ---
    public void EquipAmulet(AmuletData newAmulet)
    {
        if (newAmulet == null || currentAmulet == newAmulet) return;
        if (currentAmulet != null) UnequipAmulet();

        currentAmulet = newAmulet;
        if (playerHealth != null) playerHealth.AddBonusHealth(currentAmulet.bonusMaxHealth);
        UpdateAllStats();
    }

    public void UnequipAmulet()
    {
        if (currentAmulet == null) return;
        if (playerHealth != null) playerHealth.RemoveBonusHealth(currentAmulet.bonusMaxHealth);
        currentAmulet = null;
        UpdateAllStats();
    }

    // --- ЛОГІКА ПОЯСА ---
    public void EquipBelt(BeltData newBelt)
    {
        if (newBelt == null || currentBelt == newBelt) return;
        if (currentBelt != null) UnequipBelt();

        currentBelt = newBelt;
        if (playerHealth != null) playerHealth.AddBonusHealth(currentBelt.bonusMaxHealth);
        UpdateAllStats();
    }

    public void UnequipBelt()
    {
        if (currentBelt == null) return;
        if (playerHealth != null) playerHealth.RemoveBonusHealth(currentBelt.bonusMaxHealth);
        currentBelt = null;
        UpdateAllStats();
    }

    // --- ЛОГІКА ШОЛОМА ---
    public void EquipHelmet(HelmetData newHelmet)
    {
        if (newHelmet == null || currentHelmet == newHelmet) return;
        if (currentHelmet != null) UnequipHelmet();

        currentHelmet = newHelmet;
        if (playerHealth != null) playerHealth.AddBonusHealth(currentHelmet.bonusMaxHealth);
        UpdateAllStats();
    }

    public void UnequipHelmet()
    {
        if (currentHelmet == null) return;
        if (playerHealth != null) playerHealth.RemoveBonusHealth(currentHelmet.bonusMaxHealth);
        currentHelmet = null;
        UpdateAllStats();
    }

    // --- ЛОГІКА НАГРУДНИКА ---
    public void EquipChestplate(ChestplateData newChestplate)
    {
        if (newChestplate == null || currentChestplate == newChestplate) return;
        if (currentChestplate != null) UnequipChestplate();

        currentChestplate = newChestplate;
        if (playerHealth != null) playerHealth.AddBonusHealth(currentChestplate.bonusMaxHealth);
        UpdateAllStats();
    }

    public void UnequipChestplate()
    {
        if (currentChestplate == null) return;
        if (playerHealth != null) playerHealth.RemoveBonusHealth(currentChestplate.bonusMaxHealth);
        currentChestplate = null;
        UpdateAllStats();
    }

    // --- ЛОГІКА НАРУЧІВ ---
    public void EquipBracers(BracersData newBracers)
    {
        if (newBracers == null || currentBracers == newBracers) return;
        if (currentBracers != null) UnequipBracers();

        currentBracers = newBracers;
        if (playerHealth != null) playerHealth.AddBonusHealth(currentBracers.bonusMaxHealth);
        UpdateAllStats();
    }

    public void UnequipBracers()
    {
        if (currentBracers == null) return;
        if (playerHealth != null) playerHealth.RemoveBonusHealth(currentBracers.bonusMaxHealth);
        currentBracers = null;
        UpdateAllStats();
    }

    // --- ЛОГІКА КІЛЕЦЬ ---
    public void EquipRing(RingData newRing, int slotIndex)
    {
        if (newRing == null) return;
        UnequipRing(slotIndex);

        if (slotIndex == 1) currentRing1 = newRing;
        else if (slotIndex == 2) currentRing2 = newRing;

        if (playerHealth != null) playerHealth.AddBonusHealth(newRing.bonusMaxHealth);
        UpdateAllStats();
    }

    public void UnequipRing(int slotIndex)
    {
        RingData ringToRemove = (slotIndex == 1) ? currentRing1 : currentRing2;
        if (ringToRemove == null) return;

        if (playerHealth != null) playerHealth.RemoveBonusHealth(ringToRemove.bonusMaxHealth);
        if (slotIndex == 1) currentRing1 = null;
        else if (slotIndex == 2) currentRing2 = null;
        UpdateAllStats();
    }

    // --- ЛОГІКА ПЕТА ---
    public void EquipPet(PetData newPet)
    {
        if (newPet == null || currentPet == newPet) return;
        if (currentPet != null) UnequipPet();

        currentPet = newPet;
        if (playerHealth != null && currentPet.bonusHealth > 0f)
            playerHealth.AddBonusHealth((int)currentPet.bonusHealth);

        if (PetSpawner.Instance != null) PetSpawner.Instance.SpawnPet(currentPet);
        UpdateAllStats();
    }

    public void UnequipPet()
    {
        if (currentPet == null) return;
        if (playerHealth != null && currentPet.bonusHealth > 0f)
            playerHealth.RemoveBonusHealth((int)currentPet.bonusHealth);

        if (PetSpawner.Instance != null) PetSpawner.Instance.DespawnPet();
        currentPet = null;
        UpdateAllStats();
    }

    // --- МАГІЯ СУМУВАННЯ СТАТІВ ---
    public void UpdateAllStats()
    {
        // Стати амулета
        int amDmg = currentAmulet?.bonusDamage ?? 0;
        float amAtkSpd = currentAmulet?.bonusAttackSpeed ?? 0f;
        float amMovSpd = currentAmulet?.bonusMoveSpeed ?? 0f;
        float amCrit = currentAmulet?.bonusCritChance ?? 0f;
        float amCritM = (currentAmulet != null && currentAmulet.bonusCritMultiplier > 2f) ? currentAmulet.bonusCritMultiplier - 2f : 0f;
        int amRegen = currentAmulet?.healthRegenPerSecond ?? 0;
        float amArmor = currentAmulet?.bonusArmor ?? 0f;

        // Стати пояса
        int bDmg = currentBelt?.bonusDamage ?? 0;
        float bAtkSpd = currentBelt?.bonusAttackSpeed ?? 0f;
        float bMovSpd = currentBelt?.bonusMoveSpeed ?? 0f;
        float bCrit = currentBelt?.bonusCritChance ?? 0f;
        float bCritM = (currentBelt != null && currentBelt.bonusCritMultiplier > 2f) ? currentBelt.bonusCritMultiplier - 2f : 0f;
        int bRegen = currentBelt?.healthRegenPerSecond ?? 0;
        float bArmor = currentBelt?.bonusArmor ?? 0f;

        // Стати шолома
        float hCrit = currentHelmet?.bonusCritChance ?? 0f;
        float hCritM = (currentHelmet != null && currentHelmet.bonusCritMultiplier > 2f) ? currentHelmet.bonusCritMultiplier - 2f : 0f;
        int hRegen = currentHelmet?.healthRegenPerSecond ?? 0;
        float hArmor = currentHelmet?.bonusArmor ?? 0f;

        // Стати нагрудника
        int cpDmg = currentChestplate?.bonusDamage ?? 0;
        float cpAtkSpd = currentChestplate?.bonusAttackSpeed ?? 0f;
        float cpMovSpd = currentChestplate?.bonusMoveSpeed ?? 0f;
        float cpCrit = currentChestplate?.bonusCritChance ?? 0f;
        float cpCritM = (currentChestplate != null && currentChestplate.bonusCritMultiplier > 2f) ? currentChestplate.bonusCritMultiplier - 2f : 0f;
        int cpRegen = currentChestplate?.healthRegenPerSecond ?? 0;
        float cpArmor = currentChestplate?.bonusArmor ?? 0f;

        // Стати наручів --- ДОДАНО ---
        int brDmg = currentBracers?.bonusDamage ?? 0;
        float brAtkSpd = currentBracers?.bonusAttackSpeed ?? 0f;
        float brMovSpd = currentBracers?.bonusMoveSpeed ?? 0f;
        float brCrit = currentBracers?.bonusCritChance ?? 0f;
        float brCritM = (currentBracers != null && currentBracers.bonusCritMultiplier > 2f) ? currentBracers.bonusCritMultiplier - 2f : 0f;
        int brRegen = currentBracers?.healthRegenPerSecond ?? 0;
        float brArmor = currentBracers?.bonusArmor ?? 0f;

        // Стати пета
        int petDmg = currentPet?.bonusDamage ?? 0;

        // Розрахунок кілець
        int rDmg = 0; float rAtkSpd = 0f; float rMovSpd = 0f;
        float rCrit = 0f; float rCritM = 0f; int rRegen = 0; float rArmor = 0f;

        void AddRingStats(RingData ring)
        {
            if (ring == null) return;
            rDmg += ring.bonusDamage;
            rAtkSpd += ring.bonusAttackSpeed;
            rMovSpd += ring.bonusMoveSpeed;
            rCrit += ring.bonusCritChance;
            rRegen += ring.healthRegenPerSecond;
            rArmor += ring.bonusArmor;
            if (ring.bonusCritMultiplier > 2f) rCritM += ring.bonusCritMultiplier - 2f;
        }

        AddRingStats(currentRing1);
        AddRingStats(currentRing2);

        // Передача в системи бою (Combat)
        if (playerCombat != null)
        {
            // Наручі та нагрудник додають базову шкоду та швидкість атаки
            playerCombat.extraAmuletDamage = amDmg + bDmg + petDmg + cpDmg + brDmg;
            playerCombat.extraRingDamage = rDmg;
            playerCombat.extraAttackSpeed = amAtkSpd + bAtkSpd + cpAtkSpd + brAtkSpd;
            playerCombat.extraRingAttackSpeed = rAtkSpd;

            // Сумуємо шанс кріта та множник (з урахуванням наручів)
            playerCombat.critChance = amCrit + bCrit + rCrit + hCrit + cpCrit + brCrit;
            playerCombat.critMultiplier = 2f + amCritM + bCritM + rCritM + hCritM + cpCritM + brCritM;
        }

        // Передача в системи руху (Movement)
        if (playerMovement != null)
        {
            // Наручі разом з нагрудником впливають на швидкість бігу
            playerMovement.extraSpeedMultiplier = amMovSpd + bMovSpd + cpMovSpd + brMovSpd;
            playerMovement.extraRingSpeedMultiplier = rMovSpd;
        }

        // Передача в системи здоров'я (Health)
        if (playerHealth != null)
        {
            playerHealth.StartBuffs(amRegen + bRegen, amArmor + bArmor, 0); // 0 = Амулет/Пояс
            playerHealth.StartBuffs(rRegen, rArmor, 1);                     // 1 = Кільця
            playerHealth.StartBuffs(hRegen, hArmor, 2);                     // 2 = Шолом
            playerHealth.StartBuffs(cpRegen, cpArmor, 3);                   // 3 = Нагрудник
            playerHealth.StartBuffs(brRegen, brArmor, 4);                   // 4 = Наручі (ДОДАНО новий індекс)
        }
    }
}