using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Header("Посилання на системи гравця")]
    public PlayerHealth playerHealth;
    public PlayerCombat playerCombat;
    public PlayerMovement playerMovement;

    [Header("Поточна екіпіровка")]
    public AmuletData currentAmulet;

    [Header("Тестування (Клавіші T та U)")]
    public AmuletData testAmulet;

    public void EquipAmulet(AmuletData newAmulet)
    {
        if (newAmulet == null) return;

        if (currentAmulet == newAmulet)
        {
            Debug.Log($"Амулет {newAmulet.name} вже одягнений!");
            return;
        }

        if (currentAmulet != null)
        {
            UnequipAmulet();
        }

        currentAmulet = newAmulet;

        // 1. Здоров'я та Регенерація
        if (playerHealth != null)
        {
            playerHealth.AddBonusHealth(currentAmulet.bonusMaxHealth);

            if (currentAmulet.healthRegenPerSecond > 0)
            {
                playerHealth.StartRegen(currentAmulet.healthRegenPerSecond);
            }
        }

        // 2. Система Бою (Урон ТА Швидкість атаки)
        if (playerCombat != null)
        {
            playerCombat.extraAmuletDamage = currentAmulet.bonusDamage;
            // ПЕРЕДАЄМО ШВИДКІСТЬ АТАКИ
            playerCombat.extraAttackSpeed = currentAmulet.bonusAttackSpeed;
        }

        // 3. Рух
        if (playerMovement != null)
        {
            playerMovement.extraSpeedMultiplier = currentAmulet.bonusMoveSpeed;
        }

        Debug.Log($"<color=green>Успішно одягнено:</color> {currentAmulet.name}. Швидкість атаки: +{currentAmulet.bonusAttackSpeed * 100}%");
    }

    public void UnequipAmulet()
    {
        if (currentAmulet == null) return;

        string lastAmuletName = currentAmulet.name;

        if (playerHealth != null)
        {
            playerHealth.RemoveBonusHealth(currentAmulet.bonusMaxHealth);
            playerHealth.StopRegen();
        }

        if (playerCombat != null)
        {
            playerCombat.extraAmuletDamage = 0;
            // ОБНУЛЯЄМО ШВИДКІСТЬ АТАКИ
            playerCombat.extraAttackSpeed = 0f;
        }

        if (playerMovement != null)
        {
            playerMovement.extraSpeedMultiplier = 0f;
        }

        Debug.Log($"<color=red>Знято амулет:</color> {lastAmuletName}");
        currentAmulet = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (testAmulet != null) EquipAmulet(testAmulet);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            UnequipAmulet();
        }
    }
}