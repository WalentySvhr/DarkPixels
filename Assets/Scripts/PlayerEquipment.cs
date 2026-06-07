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
    public HelmetData currentHelmet; // ДОДАНО
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

    // --- ЛОГІКА ШОЛОМА (ДОДАНО) ---
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
        // Допоміжні змінні
        int amDmg = currentAmulet?.bonusDamage ?? 0;
        float amAtkSpd = currentAmulet?.bonusAttackSpeed ?? 0f;
        float amMovSpd = currentAmulet?.bonusMoveSpeed ?? 0f;
        float amCrit = currentAmulet?.bonusCritChance ?? 0f;
        float amCritM = (currentAmulet != null && currentAmulet.bonusCritMultiplier > 2f) ? currentAmulet.bonusCritMultiplier - 2f : 0f;
        int amRegen = currentAmulet?.healthRegenPerSecond ?? 0;
        float amArmor = currentAmulet?.bonusArmorPercent ?? 0f;

        int bDmg = currentBelt?.bonusDamage ?? 0;
        float bAtkSpd = currentBelt?.bonusAttackSpeed ?? 0f;
        float bMovSpd = currentBelt?.bonusMoveSpeed ?? 0f;
        float bCrit = currentBelt?.bonusCritChance ?? 0f;
        float bCritM = (currentBelt != null && currentBelt.bonusCritMultiplier > 2f) ? currentBelt.bonusCritMultiplier - 2f : 0f;
        int bRegen = currentBelt?.healthRegenPerSecond ?? 0;
        float bArmor = currentBelt?.bonusArmorPercent ?? 0f;

        // Стати шолома
        float hCrit = currentHelmet?.bonusCritChance ?? 0f;
        float hCritM = (currentHelmet != null && currentHelmet.bonusCritMultiplier > 2f) ? currentHelmet.bonusCritMultiplier - 2f : 0f;
        int hRegen = currentHelmet?.healthRegenPerSecond ?? 0;
        float hArmor = currentHelmet?.bonusArmorPercent ?? 0f;

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
            rArmor += ring.bonusArmorPercent;
            if (ring.bonusCritMultiplier > 2f) rCritM += ring.bonusCritMultiplier - 2f;
        }

        AddRingStats(currentRing1);
        AddRingStats(currentRing2);

        // Передача в системи
        if (playerCombat != null)
        {
            playerCombat.extraAmuletDamage = amDmg + bDmg + petDmg;
            playerCombat.extraRingDamage = rDmg;
            playerCombat.extraAttackSpeed = amAtkSpd + bAtkSpd;
            playerCombat.extraRingAttackSpeed = rAtkSpd;
            playerCombat.critChance = amCrit + bCrit + rCrit + hCrit;
            playerCombat.critMultiplier = 2f + amCritM + bCritM + rCritM + hCritM;
        }

        if (playerMovement != null)
        {
            playerMovement.extraSpeedMultiplier = amMovSpd + bMovSpd;
            playerMovement.extraRingSpeedMultiplier = rMovSpd;
        }

        if (playerHealth != null)
        {
            playerHealth.StartBuffs(amRegen + bRegen, amArmor + bArmor, 0); // 0 = Амулет/Пояс
            playerHealth.StartBuffs(rRegen, rArmor, 1);                     // 1 = Кільця
            playerHealth.StartBuffs(hRegen, hArmor, 2);                     // 2 = Шолом
        }
    }
}