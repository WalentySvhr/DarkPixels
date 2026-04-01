using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeaponManager : MonoBehaviour
{
    public enum WeaponType { Sword, Bow }
    public enum WeaponState { Locked, Purchased, Equipped }

    [Header("Current Status")]
    public WeaponType currentWeapon = WeaponType.Sword;

    [System.Serializable]
    public class WeaponData
    {
        public string name;
        public WeaponType type;
        public WeaponState state;
        public int price;
        public GameObject weaponModel;
        public GameObject shopCheckmark;
        public TMPro.TextMeshProUGUI priceText;
        public Button actionButton;
    }

    [Header("Weapon Settings")]
    public WeaponData[] allWeapons;

    [Header("UI Feedback")]
    public TMPro.TextMeshProUGUI warningText;
    public float warningDuration = 2f;

    // ВИДАЛИЛИ стару змінну private PlayerInventory inventory;

    void Start()
    {
        // Прибираємо пошук компонента GetComponent<PlayerInventory>(), 
        // бо тепер працюємо через Singleton InventoryManager.Instance

        if (warningText != null)
            warningText.gameObject.SetActive(false);

        for (int i = 0; i < allWeapons.Length; i++)
        {
            if (allWeapons[i].state == WeaponState.Equipped)
            {
                EquipWeapon(i);
                break;
            }
        }
        UpdateUI();
    }

    public void OnWeaponButtonClick(int index)
    {
        WeaponData w = allWeapons[index];

        if (w.state == WeaponState.Locked)
        {
            // ЗАМІНА: Перевіряємо монети в InventoryManager.Instance
            if (InventoryManager.Instance != null && InventoryManager.Instance.coins >= w.price)
            {
                BuyWeapon(index);
            }
            else
            {
                ShowWarning("НЕ ВИСТАЧАЄ КОШТІВ!");
            }
        }
        else if (w.state == WeaponState.Purchased)
        {
            EquipWeapon(index);
        }
    }

    void BuyWeapon(int index)
    {
        // ЗАМІНА: Знімаємо гроші через метод ChangeCoins
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ChangeCoins(-allWeapons[index].price);
            allWeapons[index].state = WeaponState.Purchased;
            EquipWeapon(index);
        }
    }

    public void EquipWeapon(int index)
    {
        for (int i = 0; i < allWeapons.Length; i++)
        {
            if (allWeapons[i].state == WeaponState.Equipped)
                allWeapons[i].state = WeaponState.Purchased;

            if (allWeapons[i].weaponModel != null)
                allWeapons[i].weaponModel.SetActive(false);
        }

        allWeapons[index].state = WeaponState.Equipped;
        currentWeapon = allWeapons[index].type;

        if (allWeapons[index].weaponModel != null)
            allWeapons[index].weaponModel.SetActive(true);

        UpdateUI();
    }

    public void UpdateUI()
    {
        foreach (var w in allWeapons)
        {
            if (w.shopCheckmark != null)
                w.shopCheckmark.SetActive(w.state == WeaponState.Equipped);

            if (w.priceText != null)
            {
                if (w.state == WeaponState.Locked)
                    w.priceText.text = w.price.ToString() + " <color=black>Монет</color>";
                else if (w.state == WeaponState.Purchased)
                    w.priceText.text = "<color=black>Взяти</color>";
                else if (w.state == WeaponState.Equipped)
                    w.priceText.text = "<color=black>У руках</color>";
            }

            if (w.actionButton != null)
            {
                w.actionButton.interactable = (w.state != WeaponState.Equipped);
            }
        }
    }

    public void ShowWarning(string message)
    {
        if (warningText == null) return;
        StopAllCoroutines();
        StartCoroutine(WarningRoutine(message));
    }

    private IEnumerator WarningRoutine(string message)
    {
        warningText.text = message;
        warningText.gameObject.SetActive(true);
        yield return new WaitForSeconds(warningDuration);
        warningText.gameObject.SetActive(false);
    }
}