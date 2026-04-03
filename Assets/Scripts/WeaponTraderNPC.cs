using UnityEngine;

public class WeaponTraderNPC : MonoBehaviour
{
    public static WeaponTraderNPC Instance;
    public PlayerCombat playerCombat;
    // Цей клас відповідає за логіку торговця зброєю. Він обробляє спроби купівлі зброї, перевіряє наявність грошей у гравця та передає інформацію про куплену зброю до PlayerCombat для екіпірування.
    void Awake() => Instance = this;

    public void TryProcessWeaponAction(ShopWeaponSlot slot)
    {
        // 1. Якщо зброя вже куплена — просто беремо її в руки
        if (slot.isBought)
        {
            Equip(slot.weaponToSell);
            return;
        }

        // 2. Якщо не куплена — звертаємось до InventoryManager за грошима
        if (InventoryManager.Instance.coins >= slot.price)
        {
            // Вираховуємо гроші через твій метод
            InventoryManager.Instance.ChangeCoins(-slot.price);

            slot.isBought = true;
            slot.UpdateUI();

            Equip(slot.weaponToSell);
            Debug.Log("Придбано: " + slot.weaponToSell.weaponName);
        }
        else
        {
            Debug.Log("Недостатньо золота!");
            // Тут можна додати виклик вікна "Немає грошей", як у рибака
        }
    }

    private void Equip(WeaponData weapon)
    {
        if (playerCombat != null)
        {
            playerCombat.EquipWeapon(weapon);
        }
    }
}