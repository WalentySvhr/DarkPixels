using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopWeaponSlot : MonoBehaviour
{
    [Header("Налаштування товару")]
    public WeaponData weaponToSell;
    public int price; // Можна брати з weaponToSell.price або задавати вручну
    public bool isBought = false;

    [Header("UI Елементи")]
    public Image weaponIconImage;      // Сюди перетягни Image для іконки
    public TextMeshProUGUI nameText;   // Сюди назву зброї
    public TextMeshProUGUI priceText;  // Сюди текст ціни
    public GameObject soldCheckmark;   // Об'єкт галочки
    public Button actionButton;        // Сама кнопка

    // Викликається один раз при створенні або старті
    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (weaponToSell == null) return;
        // Увага: подивись у свій скрипт Item.cs, як точно там називаються ці змінні. 
        // Зазвичай це просто itemName та icon
        nameText.text = weaponToSell.itemName;
        weaponIconImage.sprite = weaponToSell.icon;

        if (isBought)
        {
            priceText.text = "Куплено";
            weaponIconImage.color = Color.white; // Повертаємо нормальний колір зброї
            actionButton.interactable = false;   // Вимикаємо кнопку
            if (soldCheckmark != null) soldCheckmark.SetActive(true);
        }
        else
        {
            priceText.text = price.ToString() + " Золота";
            weaponIconImage.color = Color.white;
            actionButton.interactable = true;
            if (soldCheckmark != null) soldCheckmark.SetActive(false);
        }
    }

    public void OnClickSlot()
    {
        // Викликаємо метод торговця, який ми писали раніше
        if (WeaponTraderNPC.Instance != null)
        {
            WeaponTraderNPC.Instance.TryProcessWeaponAction(this);
        }
    }
}