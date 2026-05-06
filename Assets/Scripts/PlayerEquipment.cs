using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Header("Посилання на системи гравця")]
    public PlayerHealth playerHealth;
    public PlayerCombat playerCombat;
    public PlayerMovement playerMovement;

    [Header("Поточна екіпіровка")]
    public AmuletData currentAmulet;
    public RingData currentRing;

    [Header("Тестування (T/U - Амулет, Y/I - Кільце)")]
    public AmuletData testAmulet;
    public RingData testRing;

    // --- ЛОГІКА АМУЛЕТА ---
    public void EquipAmulet(AmuletData newAmulet)
    {
        if (newAmulet == null || currentAmulet == newAmulet) return;
        if (currentAmulet != null) UnequipAmulet();

        currentAmulet = newAmulet;

        if (playerHealth != null)
        {
            playerHealth.AddBonusHealth(currentAmulet.bonusMaxHealth);
            if (currentAmulet.healthRegenPerSecond > 0)
                playerHealth.StartRegen(currentAmulet.healthRegenPerSecond);
        }

        if (playerCombat != null)
        {
            playerCombat.extraAmuletDamage = currentAmulet.bonusDamage;
            playerCombat.extraAttackSpeed = currentAmulet.bonusAttackSpeed;
        }

        if (playerMovement != null)
            playerMovement.extraSpeedMultiplier = currentAmulet.bonusMoveSpeed;

        Debug.Log($"<color=green>Одягнено амулет:</color> {currentAmulet.name}");
    }

    public void UnequipAmulet()
    {
        if (currentAmulet == null) return;

        if (playerHealth != null)
        {
            playerHealth.RemoveBonusHealth(currentAmulet.bonusMaxHealth);
            playerHealth.StopRegen();
        }

        if (playerCombat != null)
        {
            playerCombat.extraAmuletDamage = 0;
            playerCombat.extraAttackSpeed = 0f;
        }

        if (playerMovement != null)
            playerMovement.extraSpeedMultiplier = 0f;

        currentAmulet = null;
    }

    // --- ОНОВЛЕНА ЛОГІКА ДЛЯ КІЛЬЦЯ ---
    public void EquipRing(RingData newRing)
    {
        if (newRing == null || currentRing == newRing) return;
        if (currentRing != null) UnequipRing();

        currentRing = newRing;

        if (playerHealth != null)
        {
            playerHealth.AddBonusHealth(currentRing.bonusMaxHealth);
            if (currentRing.healthRegenPerSecond > 0)
                playerHealth.StartRegen(currentRing.healthRegenPerSecond);
        }

        if (playerCombat != null)
        {
            // ВАЖЛИВО: Переконайся, що в PlayerCombat є ці змінні
            playerCombat.extraRingDamage = currentRing.bonusDamage;
            playerCombat.extraRingAttackSpeed = currentRing.bonusAttackSpeed;
        }

        if (playerMovement != null)
        {
            // ВАЖЛИВО: Переконайся, що в PlayerMovement є ця змінна
            playerMovement.extraRingSpeedMultiplier = currentRing.bonusMoveSpeed;
        }

        Debug.Log($"<color=cyan>Одягнено кільце:</color> {currentRing.name}");
    }

    public void UnequipRing()
    {
        if (currentRing == null) return;

        if (playerHealth != null)
        {
            playerHealth.RemoveBonusHealth(currentRing.bonusMaxHealth);
            playerHealth.StopRegen();
        }

        if (playerCombat != null)
        {
            playerCombat.extraRingDamage = 0;
            playerCombat.extraRingAttackSpeed = 0f;
        }

        if (playerMovement != null)
            playerMovement.extraRingSpeedMultiplier = 0f;

        Debug.Log($"<color=red>Знято кільце:</color> {currentRing.name}");
        currentRing = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) if (testAmulet != null) EquipAmulet(testAmulet);
        if (Input.GetKeyDown(KeyCode.U)) UnequipAmulet();

        if (Input.GetKeyDown(KeyCode.Y)) if (testRing != null) EquipRing(testRing);
        if (Input.GetKeyDown(KeyCode.I)) UnequipRing();
    }
}