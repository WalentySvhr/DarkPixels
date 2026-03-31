using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FisherShopSlot : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI amountText; // НОВЕ: перетягни сюди об'єкт тексту кількості

    private Item currentItem;
    private bool isSellSlot;

    // Додаємо параметр amount, за замовчуванням він буде 1
    public void Setup(Item item, bool isSelling, int amount = 1)
    {
        currentItem = item;
        isSellSlot = isSelling;

        if (icon != null) icon.sprite = item.icon;
        if (nameText != null) nameText.text = item.itemName;
        if (priceText != null) priceText.text = item.price.ToString() + " золота";

        // Логіка відображення кількості
        if (amountText != null)
        {
            // Якщо предметів більше 1, показуємо число, якщо 1 — ховаємо текст (або пиши x1)
            if (amount > 1)
            {
                amountText.text = amount.ToString();
                amountText.gameObject.SetActive(true);
            }
            else
            {
                amountText.gameObject.SetActive(false);
            }
        }
    }

    public void OnClick()
    {
        if (currentItem == null) return;

        if (isSellSlot)
            FisherShopManager.Instance.SellItem(currentItem);
        else
            FisherShopManager.Instance.BuyItem(currentItem);
    }
}