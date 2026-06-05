using UnityEngine;

// Цей скрипт відповідає за логіку покупки зброї та взаємодії з Інвентарем.
public class WeaponTraderNPC : MonoBehaviour
{
    public static WeaponTraderNPC Instance;

    void Awake() => Instance = this;

    public void TryProcessWeaponAction(ShopWeaponSlot slot)
    {
        if (slot.isBought) return;

        if (InventoryManager.Instance.coins >= slot.price)
        {
            bool added = InventoryManager.Instance.Add(slot.weaponToSell);

            if (added)
            {
                InventoryManager.Instance.ChangeCoins(-slot.price);
                slot.isBought = true;
                slot.UpdateUI();

                // Оновлено: використовуємо SendMessage для безпечної зупинки NPC.
                // Це спрацює для будь-якого компонента на цьому NPC, що має метод StartInteraction.
                SendMessage("StartInteraction", SendMessageOptions.DontRequireReceiver);

                // Оновлення UI
                WeaponShopUI shopUI = FindFirstObjectByType<WeaponShopUI>();
                if (shopUI != null) shopUI.RefreshShopUI();

                Debug.Log("Зброя додана в інвентар!");
            }
            else
            {
                Debug.Log("Інвентар повний!");
            }
        }
        else
        {
            Debug.Log("Недостатньо монет!");
        }
    }
}