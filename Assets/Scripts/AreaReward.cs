using UnityEngine;

public class AreaReward : MonoBehaviour
{
    [Header("Reward Settings")]
    public GameObject chestPrefab; // Префаб скрині
    public int goldBonus = 500;    // Пряме нарахування золота (опціонально)

    [Header("Effects")]
    public GameObject rewardEffect; // Ефект сяйва або салюту

    public void SpawnReward(Vector2 spawnPosition)
    {
        // 1. Створюємо ефект (якщо є)
        if (rewardEffect != null)
        {
            Instantiate(rewardEffect, spawnPosition, Quaternion.identity);
        }

        // 2. Створюємо фізичну скриню
        if (chestPrefab != null)
        {
            GameObject chest = Instantiate(chestPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("<color=gold>Скриня з нагородою з'явилася!</color>");
        }

        // 3. Можна додати прямий бонус, якщо у тебе є скрипт Inventory чи Wallet
        // Wallet.Instance.AddGold(goldBonus);
    }
}