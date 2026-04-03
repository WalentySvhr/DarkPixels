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

        // 1. Заповнюємо візуал даними з ScriptableObject
        if (nameText != null) nameText.text = weaponToSell.weaponName;
        if (weaponIconImage != null) weaponIconImage.sprite = weaponToSell.weaponIcon;

        // 2. Логіка стану кнопки (Куплено чи Ціна)
        if (isBought)
        {
            if (priceText != null) priceText.text = "Куплено";
            if (soldCheckmark != null) soldCheckmark.SetActive(true);

            // Якщо куплено, можна змінити колір кнопки або іконки
            // actionButton.image.color = Color.gray; 
        }
        else
        {
            if (priceText != null) priceText.text = price.ToString() + " Золота";
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