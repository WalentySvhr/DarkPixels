using UnityEngine;

public class WeaponTraderNPC : MonoBehaviour
{
    public static WeaponTraderNPC Instance;

    // Посилання на PlayerCombat більше не потрібне, бо ми одягаємо зброю з Інвентарю!

    void Awake() => Instance = this;

    public void TryProcessWeaponAction(ShopWeaponSlot slot)
    {
        // Якщо вже купили, нічого не робимо
        if (slot.isBought) return;

        // Перевіряємо, чи вистачає грошей
        if (InventoryManager.Instance.coins >= slot.price)
        {
            // 1. СПЕРШУ ПРОБУЄМО ДОДАТИ В ІНВЕНТАР
            // Якщо місця немає, метод Add поверне false
            bool added = InventoryManager.Instance.Add(slot.weaponToSell);

            if (added)
            {
                // 2. І ТІЛЬКИ ЯКЩО ПРЕДМЕТ ДОДАНО, ЗНІМАЄМО ГРОШІ
                InventoryManager.Instance.ChangeCoins(-slot.price);

                slot.isBought = true;
                slot.UpdateUI();

                // 3. Оновлюємо весь магазин (щоб оновити тексти кнопок і баланс)
                WeaponShopUI shopUI = FindFirstObjectByType<WeaponShopUI>();
                if (shopUI != null)
                {
                    shopUI.RefreshShopUI();
                }

                Debug.Log("Зброя додана в інвентар!");
            }
            else
            {
                Debug.Log("Інвентар повний! Немає місця для зброї.");
                // Тут можна додати виклик повідомлення на екран для гравця
            }
        }
        else
        {
            Debug.Log("Недостатньо монет!");
        }
    }
}